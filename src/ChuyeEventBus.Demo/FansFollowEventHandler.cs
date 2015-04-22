﻿using ChuyeEventBus.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace ChuyeEventBus.Demo {

    [Export(typeof(IEventHandler))]
    public class FansFollowEventHandler : IEventHandler<FansFollowEvent> {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public void Handle(FansFollowEvent eventEntry) {
            //throw new Exception("Random error");
            _logger.Trace("FansFollowEventHandler : 用户 {0} Follow 用户 {1}",
                eventEntry.FromId, eventEntry.ToId);
        }
        public void Handle(IEvent eventEntry) {
            Handle((FansFollowEvent)eventEntry);
        }

        public void Handle(IEnumerable<IEvent> events) {
            throw new NotImplementedException();
        }
    }
}
