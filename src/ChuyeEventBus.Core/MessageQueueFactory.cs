using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Core {
    public static class MessageQueueFactory {
        public static MessageQueue Build(IEventBehaviour eventBehaviour) {
            var msgPath = eventBehaviour.GetMessagePath();
            var dequeueQuantity = eventBehaviour.GetDequeueQuantity();

            if (!msgPath.StartsWith("FormatName:") && !MessageQueue.Exists(msgPath)) {
                MessageQueue.Create(msgPath);
            }

            var msgQueue = new MessageQueue(msgPath);
            msgQueue.Formatter = eventBehaviour.GetMessageFormatter();
            return msgQueue;
        }
    }
}
