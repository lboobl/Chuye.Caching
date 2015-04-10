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

    [EventHandler(Quantity = 10, SupportMultiple = true)]
    [Export(typeof(IEventHandler))]
    public class WorkUpdateEventHandler : IEventHandler<WorkUpdateEvent> {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public bool SupportMultiple {
            get { return true; }
        }

        public Type EventType {
            get { return typeof(WorkUpdateEvent); }
        }

        public void Handle(WorkUpdateEvent eventEntry) {
            if (eventEntry.UpdateType == WorkUpdateType.Access) {
                _logger.Trace("WorkUpdateEventHandler  作品 [{0}] 计数增加", eventEntry.WorkId);
            }
            else {
                _logger.Trace("WorkUpdateEventHandler  作品 [{0}] 分享增加", eventEntry.WorkId);
            }
        }

        public void Handle(IEvent eventEntry) {
            Handle((WorkUpdateEvent)eventEntry);
        }

        public void Handle(IEnumerable<IEvent> events) {
            _logger.Trace("WorkUpdateEventHandler 批量更新 {0}", events.Count());
        }
    }
}
