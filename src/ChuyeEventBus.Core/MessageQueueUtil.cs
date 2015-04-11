using System;
using System.Diagnostics;
using System.Threading;

namespace ChuyeEventBus.Core {
    public static class MessageQueueUtil {
        public static void Send(IEvent eventEntry) {
            var factory = new MessageQueueFactory();
            var queue = factory.ApplyQueue(eventEntry);
            queue.Send(eventEntry);
        }
    }
}
