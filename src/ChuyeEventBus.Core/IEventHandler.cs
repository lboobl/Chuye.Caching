using System;
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
        void Handle(IEvent @event);
    }

    public interface IEventHandler<T> : IEventHandler where T : IEvent {
        void Handle(T @event);
    }

    public class SampleEvent : IEvent {
    }

    public class SampleEventHandler : IEventHandler<SampleEvent> {
        public Type EventType {
            get { return typeof(SampleEvent); }
        }

        public void Handle(SampleEvent @event) {
            Debug.WriteLine(String.Format("{0}: Handle {1}",
                this.GetType().Name, @event.GetType().Name));
        }

        public void Handle(IEvent @event) {
            if (!(@event is SampleEvent)) {
                throw new ArgumentOutOfRangeException(String.Format("Expect 'ShareEvent', got '{0}'", @event.GetType().FullName));
            }
            Handle((SampleEvent)@event);
        }
    }
}
