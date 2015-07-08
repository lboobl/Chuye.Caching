using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Messaging;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ChuyeEventBus.Core {
    public static class MessageQueueUtil {
        private static readonly ConcurrentDictionary<Type, MessageQueue> _msgQueue
            = new ConcurrentDictionary<Type, MessageQueue>();


        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Send(IEvent eventEntry) {
            if (Boolean.FalseString.Equals(ConfigurationManager.AppSettings.Get("chuye:EnableEvent"), StringComparison.OrdinalIgnoreCase)) {
                return;
            }
            var eventType = eventEntry.GetType();
            var msgQueue = _msgQueue.GetOrAdd(eventType, et => {
                var eventBehaviour = EventExtension.GetEventBehaviour(et);
                return MessageQueueFactory.Build(eventBehaviour);
            });
            msgQueue.Send(eventEntry);
        }

        public static async Task<IEvent> ReceiveAsync(Type eventType) {
            var msgQueue = _msgQueue.GetOrAdd(eventType, et => {
                var eventBehaviour = EventExtension.GetEventBehaviour(et);
                return MessageQueueFactory.Build(eventBehaviour);
            });
            var msg = await new MessageReceiver(msgQueue).ReceiveAsync();
            if (msg != null && msg.Body != null) {
                return (IEvent)msg.Body;
            }
            return null;
        }
    }
}
