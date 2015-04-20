using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Messaging;
using System.Threading;

namespace ChuyeEventBus.Core {
    public static class MessageQueueUtil {
        private static readonly Dictionary<String, MessageQueue> _queues
            = new Dictionary<String, MessageQueue>(StringComparer.OrdinalIgnoreCase);

        public static MessageQueue ApplyQueue(Type eventType) {
            if (!typeof(IEvent).IsAssignableFrom(eventType)) {
                throw new ArgumentOutOfRangeException("eventType");
            }
            var eventBehaviour = EventExtension.BuildEventBehaviour(eventType);
            return ApplyQueue(eventBehaviour);
        }

        public static MessageQueue ApplyQueue(EventBehaviourAttribute eventBehaviour) {
            MessageQueue messageQueue;
            if (_queues.TryGetValue(eventBehaviour.Label, out messageQueue)) {
                return messageQueue;
            }
            else {
                if (!eventBehaviour.Label.StartsWith("FormatName:") && !MessageQueue.Exists(eventBehaviour.Label)) {
                    MessageQueue.Create(eventBehaviour.Label);
                }
                messageQueue = new MessageQueue(eventBehaviour.Label);
                messageQueue.Formatter = (IMessageFormatter)Activator.CreateInstance(eventBehaviour.Formatter);
                return messageQueue;
            }
        }

        public static void Send(IEvent eventEntry) {
            var queue = ApplyQueue(eventEntry.GetType());
            Debug.WriteLine("Sending {0} via {1}", eventEntry.GetType().Name, queue.Path);
            queue.Send(eventEntry);
        }
    }
}
