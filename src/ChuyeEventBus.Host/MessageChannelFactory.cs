using ChuyeEventBus.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    public class MessageChannelFactory {
        private readonly MessageQueueFactory _msgQueueFactory = new MessageQueueFactory();

        public IMessageChannel Build(IEventBehaviour eventBehaviour) {
            var msgQueue = _msgQueueFactory.Build(eventBehaviour);
            var messageReceiver = new MessageReceiver(msgQueue);
            var friendlyName = msgQueue.Path.Split('/').Last();
            var dequeueQuantity = eventBehaviour.GetDequeueQuantity();

            if (dequeueQuantity == 1) {
                return new MessageChannel(friendlyName, messageReceiver);
            }
            else {
                return new MultipleMessageChannel(friendlyName, messageReceiver, dequeueQuantity);
            }
        }
    }
}
