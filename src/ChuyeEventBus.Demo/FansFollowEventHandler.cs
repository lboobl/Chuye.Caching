using ChuyeEventBus.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChuyeEventBus.Demo {

    [Export(typeof(IEventHandler))]
    public class FansFollowEventHandler : IEventHandler<FansFollowEvent> {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public bool SupportMultiple {
            get { return false; }
        }

        public Type EventType {
            get { return typeof(FansFollowEvent); }
        }

        public void Handle(FansFollowEvent @event) {
            _logger.Trace("FansFollowEventHandler: 用户 {0} Follow 用户 {1}",
                @event.FromId, @event.ToId);
        }
        public void Handle(IEvent @event) {
            Handle((FansFollowEvent)@event);
        }

        public void Handle(IEnumerable<IEvent> events) {
            throw new NotImplementedException();
        }
    }
}
