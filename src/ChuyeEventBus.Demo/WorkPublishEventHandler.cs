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
    public class WorkPublishEventHandler : IEventHandler<WorkPublishEvent> {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public bool SupportMultiple {
            get { return false; }
        }

        public Type EventType {
            get { return typeof(WorkPublishEvent); }
        }

        public void Handle(WorkPublishEvent eventEntry) {
            _logger.Trace("WorkPublishEventHandler: 作品 [{0}] 发布", eventEntry.WorkId);
        }

        public void Handle(IEvent eventEntry) {
            Handle((WorkPublishEvent)eventEntry);
        }
        
        public void Handle(IList<IEvent> events) {
            throw new NotImplementedException();
        }
    }
}
