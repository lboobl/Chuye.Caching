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

        public Type EventType {
            get { return typeof(WorkPublishEvent); }
        }

        public void Handle(WorkPublishEvent @event) {
            _logger.Trace("WorkPublishEventHandler 作品 [{0}] 发布", @event.WorkId);
        }

        public void Handle(IEvent @event) {
            Handle((WorkPublishEvent)@event);
        }
    }
}
