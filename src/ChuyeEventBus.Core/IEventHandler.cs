﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Core {
    public interface IEvent {
    }

    public interface IEventHandler {
        Type EventType { get; }
        Boolean SupportMultiple { get; }
        void Handle(IEvent eventEntry);
        void Handle(IEnumerable<IEvent> events);
    }

    public interface IEventHandler<TEnvent> : IEventHandler where TEnvent : IEvent {
        void Handle(TEnvent eventEntry);
    }
}
