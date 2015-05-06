using ChuyeEventBus.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;

namespace ChuyeEventBus.Demo {

    [Export(typeof(IEventHandler))]
    public class FansFollowEventHandler1 : IEventHandler<FansFollowEvent> {
        public void Handle(FansFollowEvent eventEntry) {
            Thread.Sleep(5000);
            Console.WriteLine("FansFollowEventHandler1 : 用户 {0} Follow 用户 {1}",
                eventEntry.FromId, eventEntry.ToId);
        }
        public void Handle(IEvent eventEntry) {
            Handle((FansFollowEvent)eventEntry);
        }

        public void Handle(IEnumerable<IEvent> events) {
            throw new NotImplementedException();
        }
    }

    [Export(typeof(IEventHandler))]
    public class FansFollowEventHandler2 : IEventHandler<FansFollowEvent> {
        public void Handle(FansFollowEvent eventEntry) {
            Thread.Sleep(5000);
            Console.WriteLine("FansFollowEventHandler2 : 用户 {0} Follow 用户 {1}",
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
