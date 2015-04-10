using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Core {
    public class EventHandlerAttribute : Attribute {
        public Boolean SupportMultiple { get; set; }
        public Boolean SupportConcurrent { get; set; }
        public Int32 Quantity { get; set; }
    }
}
