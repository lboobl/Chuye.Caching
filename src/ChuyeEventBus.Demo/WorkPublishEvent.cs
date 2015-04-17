using ChuyeEventBus.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Demo {
    [Serializable]
    [EventBehaviour(Formatter = typeof(InnerMessageFormatter), DequeueQuantity = 4)]
    public class WorkPublishEvent : IEvent {
        public Int32 WorkId { get; set; }
        public Object WorkDto { get; set; }
    }
}
