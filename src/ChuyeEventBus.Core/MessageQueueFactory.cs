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

        public MessageQueue ApplyQueue(Type eventType) {
            var behaviour = EventExtension.BuildEventBehaviour(eventType);
            MessageQueue messageQueue;
            if (_queues.TryGetValue(behaviour.Label, out messageQueue)) {
                return messageQueue;
            }
            else {
                if (!behaviour.Label.StartsWith("FormatName:") && !MessageQueue.Exists(behaviour.Label)) {
                    MessageQueue.Create(behaviour.Label);
                }
                messageQueue = new MessageQueue(behaviour.Label);
                messageQueue.Formatter = (IMessageFormatter)Activator.CreateInstance(behaviour.Formatter);
                return messageQueue;
            }
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
