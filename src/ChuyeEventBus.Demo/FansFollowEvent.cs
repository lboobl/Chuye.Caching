using ChuyeEventBus.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Demo {
    [Serializable]
    public class FansFollowEvent : IEvent {
        public Int32 FromId { get; set; }
        public Int32 ToId { get; set; }
    }
}
