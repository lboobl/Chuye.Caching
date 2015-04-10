using System;
using System.Diagnostics;
using System.Threading;

namespace ChuyeEventBus.Core {
    public static class MessageQueueUtil {
        public static void Send(IEvent eventEntry) {
            Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} Send {1}",
                    DateTime.Now, eventEntry.GetType().Name));
            var factory = new MessageQueueFactory();
            var pathFinder = new EventPathFinder();
            var path = pathFinder.FindPath(eventEntry);
            factory.Apply(path).Send(eventEntry);
        }
    }
}
