using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace ChuyeEventBus.Core {
    public static class MessageQueueUtil {
        public static void Send(IEvent eventEntry) {
            if (Boolean.FalseString.Equals(ConfigurationManager.AppSettings.Get("chuye:SendEvent"), StringComparison.OrdinalIgnoreCase)) {
                return;
            }
            var eventBehaviour = EventExtension.GetEventBehaviour(eventEntry.GetType());
            var msgQueue = MessageQueueFactory.Build(eventBehaviour);
            msgQueue.Send(eventEntry);
        }

        public static async Task<IEvent> ReceiveAsync(Type eventType) {
            var eventBehaviour = EventExtension.GetEventBehaviour(eventType);
            var msgQueue = MessageQueueFactory.Build(eventBehaviour);
            var msg = await new MessageReceiver(msgQueue).ReceiveAsync();
            return (IEvent)msg.Body;
        }
    }
}
