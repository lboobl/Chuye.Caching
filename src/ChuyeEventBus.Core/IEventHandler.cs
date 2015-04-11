using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Core {   
    public interface IEventHandler {
        Type EventType { get; }
        void Handle(IEvent eventEntry);
        void Handle(IList<IEvent> eventEntries);
    }
}
