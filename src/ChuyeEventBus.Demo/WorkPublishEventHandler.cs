using ChuyeEventBus.Core;
using NLog;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace ChuyeEventBus.Demo {

    [Export(typeof(IEventHandler))]
    public class WorkPublishEventHandler : IEventHandler<WorkPublishEvent> {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public void Handle(WorkPublishEvent eventEntry) {
            _logger.Trace("WorkPublishEventHandler: 作品 [{0:d2}]", eventEntry.WorkId);
        }

        public void Handle(IEvent eventEntry) {
            Handle((WorkPublishEvent)eventEntry);
        }

        public void Handle(IEnumerable<IEvent> eventEntries) {
            //throw new Exception("Random error");
            var eventGroups = eventEntries.Cast<WorkPublishEvent>().GroupBy(e => e.WorkId);
            foreach (var eventGroup in eventGroups) {
                _logger.Trace("WorkPublishEventHandler: 作品 [{0:d2}] 发布 {1,3}",
                    eventGroup.Key, eventGroup.Count());
            }
        }
    }
}
