using System;
using System.Diagnostics;
using System.Threading;

namespace ChuyeEventBus.Core {
    public static class MessageQueueUtil {

        public static String GetPath(Type eventType) {
            var label = eventType.FullName.ToLower().Replace('.', '_');
            return String.Format(@".\Private$\{0}", label);
        }

        public static void Send(IEvent @event) {
            var path = GetPath(@event.GetType());
            Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} Send {1}",
                    DateTime.Now, @event.GetType().Name));
            var factory = new MessageQueueFactory();
            factory.Apply(path).Send(@event);
        }
    }
}
