using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Core {
    public class MessageQueueFactory {
        private static readonly Dictionary<IEventBehaviour, MessageQueue> _msgQueues
            = new Dictionary<IEventBehaviour, MessageQueue>();

        public MessageQueue Build(IEventBehaviour eventBehaviour) {
            MessageQueue msgQueue;
            if (!_msgQueues.TryGetValue(eventBehaviour, out msgQueue)) {
                var msgPath = eventBehaviour.GetMessagePath();
                var dequeueQuantity = eventBehaviour.GetDequeueQuantity();

                if (!msgPath.StartsWith("FormatName:") && !MessageQueue.Exists(msgPath)) {
                    MessageQueue.Create(msgPath);
                }

                msgQueue = new MessageQueue(msgPath);
                msgQueue.Formatter = eventBehaviour.GetMessageFormatter();
                _msgQueues.Add(eventBehaviour, msgQueue);
            }
            return msgQueue;
        }
    }
}
