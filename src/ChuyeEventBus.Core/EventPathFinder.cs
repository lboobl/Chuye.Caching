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
            String label;
            if (eventAttr != null) {
                label = eventAttr.Label;
            }
            else {
                label = eventType.FullName.ToString().Replace('.', '_').ToLower();
            }

            return String.Format(@".\Private$\{0}", label);
            //return String.Format(@"FormatName:Direct=TCP:192.168.0.230\private$\{0}", label);
            //return String.Format(@"FormatName:DIRECT=TCP:192.168.0.230\{0}", label);
        }
    }
}
