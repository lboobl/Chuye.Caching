using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Core {
    public class EventAttribute : Attribute {
        public String Label { get; private set; }
        public EventAttribute(String label) {
            Label = label;
        }
    }
}
