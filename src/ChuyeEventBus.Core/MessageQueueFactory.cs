using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Reflection;
using System.Text;

namespace ChuyeEventBus.Core {
    public class MessageQueueFactory : IDisposable {
        private Dictionary<String, MessageQueue> _queues
            = new Dictionary<String, MessageQueue>(StringComparer.OrdinalIgnoreCase);

        ~MessageQueueFactory() {
            Dispose();
        }

        public MessageQueue ApplyQueue(IEvent eventEntry) {
            var eventType = eventEntry.GetType();
            return ApplyQueue(eventType);
        }

        public MessageQueue ApplyQueue(Type eventType) {
            var path = FindMessagePath(eventType);
            MessageQueue messageQueue;
            if (_queues.TryGetValue(path, out messageQueue)) {
                return messageQueue;
            }
            else {
                if (!path.StartsWith("FormatName:") && !MessageQueue.Exists(path)) {
                    MessageQueue.Create(path);
                }
                messageQueue = new MessageQueue(path);
                messageQueue.Formatter = FindMessageFormatter(eventType);
                return messageQueue;
            }
        }

        public String FindMessagePath(Type eventType) {
            var eventAttr = eventType.GetCustomAttribute<EventAttribute>();
            String label;
            if (eventAttr != null && !String.IsNullOrWhiteSpace(eventAttr.Label)) {
                label = eventAttr.Label;
            }
            else {
                label = eventType.FullName.ToString().Replace('.', '_').ToLower();
            }
            if (ConfigurationManager.ConnectionStrings["MsmqHost"] != null) {
                var hostAddress = ConfigurationManager.ConnectionStrings["MsmqHost"].ConnectionString;
                if (!String.IsNullOrWhiteSpace(hostAddress)) {
                    //todo: 使用格式写入配置文件
                    return String.Format(@"FormatName:DIRECT=TCP:{0}\Private$\{1}", hostAddress, label);
                    //return String.Format(@"FormatName:Direct=http://{0}/msmq/private$/{1}", hostAddress, label);
                }
            }
            return String.Format(@".\Private$\{0}", label);
            //return String.Format(@"FormatName:Direct=TCP:192.168.0.230\private$\{0}", label);
            //return String.Format(@"FormatName:DIRECT=TCP:192.168.0.230\{0}", label);
        }

        private IMessageFormatter FindMessageFormatter(Type eventType) {
            var eventAttr = eventType.GetCustomAttribute<EventAttribute>();
            if (eventAttr == null || eventAttr.Formatter == null) {
                return new BinaryMessageFormatter();
            }
            return (IMessageFormatter)Activator.CreateInstance(eventAttr.Formatter);
        }

        public void Dispose() {
            foreach (var queue in _queues) {
                queue.Value.Close();
                queue.Value.Dispose();
            }
            _queues.Clear();
        }
    }


    public class InnerMessageFormatter : IMessageFormatter {
        private static readonly ConcurrentDictionary<String, Type> _knownTypes
            = new ConcurrentDictionary<String, Type>();

        public bool CanRead(Message message) {
            var stream = message.BodyStream;
            return stream != null && stream.CanRead && stream.Length > 0;
        }

        public object Read(Message message) {
            if (!CanRead(message)) {
                throw new NotSupportedException("Message can not be readed");
            }

            using (var reader = new StreamReader(message.BodyStream, Encoding.UTF8)) {
                var msgTypeName = reader.ReadLine();
                Type msgType = _knownTypes.GetOrAdd(msgTypeName, key => {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    var eventType = typeof(IEvent);
                    msgType = assemblies
                        .Where(t => !t.IsDynamic)
                        .SelectMany(t => t.ExportedTypes)
                        .Where(r => eventType.IsAssignableFrom(r))
                        .First(r => r.FullName == key);
                    if (msgType == null) {
                        throw new Exception(String.Format("Unknonw type \"{0}\"", key));
                    }
                    return msgType;
                });

                String json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject(json, msgType);
            }
        }

        public void Write(Message message, object obj) {
            message.BodyStream = new MemoryStream();
            var writer = new StreamWriter(message.BodyStream);
            writer.WriteLine(obj.GetType().FullName);
            String json = JsonConvert.SerializeObject(obj, Formatting.None);
            writer.WriteLine(json);
            writer.Flush();
            //message.BodyStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            //Need to reset the body type, in case the same message is reused by some other formatter.
            message.BodyType = 0;
        }

        public object Clone() {
            return new InnerMessageFormatter();
        }
    }
}
