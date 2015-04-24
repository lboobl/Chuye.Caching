using ChuyeEventBus.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Demo {
    [Serializable]
    //[EventBehaviour(DequeueQuantity = 4)]
    [CustomEventBehaviour(typeof(CustomWorkPublishEventBehaviour))]
    public class WorkPublishEvent : IEvent {
        public Int32 WorkId { get; set; }
        public Object WorkDto { get; set; }
    }

    public class CustomWorkPublishEventBehaviour : DefaultEventBehaviour {
        public override int GetDequeueQuantity() {
            return 2;
        }

        public CustomWorkPublishEventBehaviour() {
            EventType = typeof(WorkPublishEvent);
        }
    }
}
