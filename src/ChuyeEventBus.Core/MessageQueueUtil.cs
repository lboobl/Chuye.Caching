using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace ChuyeEventBus.Core {
    public static class MessageQueueUtil {
        private static readonly Dictionary<Type, MessageQueue> _msgQueue
            = new Dictionary<Type, MessageQueue>();

        public static void Send(IEvent eventEntry) {
            MessageQueue msgQueue;
            var eventType = eventEntry.GetType();
            if (!_msgQueue.TryGetValue(eventType, out msgQueue)) {
                if (Boolean.FalseString.Equals(ConfigurationManager.AppSettings.Get("chuye:SendEvent"), StringComparison.OrdinalIgnoreCase)) {
                    return;
                }
                var eventBehaviour = EventExtension.GetEventBehaviour(eventEntry.GetType());
                msgQueue = MessageQueueFactory.Build(eventBehaviour);
                _msgQueue.Add(eventType, msgQueue);
            }

            msgQueue.Send(eventEntry);
        }

        public static async Task<IEvent> ReceiveAsync(Type eventType) {
            MessageQueue msgQueue;
            if (!_msgQueue.TryGetValue(eventType, out msgQueue)) {
                var eventBehaviour = EventExtension.GetEventBehaviour(eventType);
                msgQueue = MessageQueueFactory.Build(eventBehaviour);
                _msgQueue.Add(eventType, msgQueue);
            }
            var msg = await new MessageReceiver(msgQueue).ReceiveAsync();
            return (IEvent)msg.Body;
        }
    }
}
