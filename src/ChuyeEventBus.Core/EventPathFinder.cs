using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace ChuyeEventBus.Core {
    public class EventPathFinder {
        public String FindPath(IEvent eventEntry) {
            return FindPath(eventEntry.GetType());
        }

        public String FindPath(Type eventType) {
            var eventAttr = eventType.GetCustomAttribute<EventAttribute>();
            if (eventAttr != null) {
                return eventAttr.Path;
            }
            var label = eventType.FullName.ToString().Replace('.', '_').ToLower();
            return String.Format(@".\Private$\{0}", label);
        }
    }
}
