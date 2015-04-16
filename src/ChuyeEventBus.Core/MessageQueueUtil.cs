using System;
using System.Diagnostics;
using System.Threading;

namespace ChuyeEventBus.Core {
    public static class MessageQueueUtil {
        public static void Send(IEvent eventEntry) {
            var factory = new MessageQueueFactory();
            var queue = factory.ApplyQueue(eventEntry);
            //Debug.WriteLine(String.Format("Sending {0} from {1}", eventEntry.GetType().FullName, queue.Path));
            queue.Send(eventEntry);
        }
    }
}
