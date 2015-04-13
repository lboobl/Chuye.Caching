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

        public void Handle(FansFollowEvent eventEntry) {
            _logger.Trace("FansFollowEventHandler:  用户 {0} Follow 用户 {1}",
                eventEntry.FromId, eventEntry.ToId);
        }
        public void Handle(IEvent eventEntry) {
            Handle((FansFollowEvent)eventEntry);
        }

        public void Handle(IList<IEvent> events) {
            throw new NotImplementedException();
        }
    }
}
