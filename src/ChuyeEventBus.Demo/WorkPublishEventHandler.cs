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

        public void Handle(WorkPublishEvent eventEntry) {
            throw new NotImplementedException();
        }

        public void Handle(IEvent eventEntry) {
            throw new NotImplementedException();
        }

        public void Handle(IEnumerable<IEvent> events) {
            foreach (WorkPublishEvent eventEntry in events) {
                _logger.Trace("WorkPublishEventHandler: 作品 [{0}] 发布", eventEntry.WorkId);
            }
        }
    }
}
