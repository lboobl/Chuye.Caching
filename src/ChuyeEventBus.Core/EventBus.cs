using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChuyeEventBus.Core {
    public class EventBus {
        private static readonly EventBus _singleton = new EventBus();
        private readonly EventHandlerEqualityComparer _comparer = new EventHandlerEqualityComparer();
        private Dictionary<Type, List<IEventHandler>> _eventHandlers = new Dictionary<Type, List<IEventHandler>>();
        private Dictionary<IEventHandler, Int32> _errors = new Dictionary<IEventHandler, Int32>();
        public Action<IEventHandler, Exception, IList<IEvent>> ErrorHandler;
        public const Int32 ErrorCapacity = 3;

        public static EventBus Singleton {
            get { return _singleton; }
        }

        private EventBus() {
        }

        public void Subscribe(IEventHandler eventHandler) {
            var eventType = eventHandler.GetEventType();
            List<IEventHandler> eventHandlers;
            if (_eventHandlers.TryGetValue(eventType, out eventHandlers)) {
                if (!eventHandlers.Contains(eventHandler, _comparer)) {
                    Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} EventBus: 添加订阅 {1} for {2}",
                        DateTime.Now, eventHandler.GetType().Name, eventType.Name));
                    eventHandlers.Add(eventHandler);
                }
            }
            else {
                eventHandlers = new List<IEventHandler>();
                eventHandlers.Add(eventHandler);
                Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} EventBus: 注册事件 {1}",
                    DateTime.Now, eventType.Name));
                Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} EventBus: 添加订阅 {1} for {2}",
                    DateTime.Now, eventHandler.GetType().Name, eventType.Name));
                _eventHandlers.Add(eventType, eventHandlers);
            }
        }

        public void Unsubscribe(IEventHandler eventHandler) {
            var eventType = eventHandler.GetEventType();
            List<IEventHandler> eventHandlers;
            if (_eventHandlers.TryGetValue(eventType, out eventHandlers)) {
                Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} EventBus: 取消订阅 {1} for {2}",
                    DateTime.Now, eventHandler.GetType().Name, eventType.Name));
                eventHandlers.RemoveAll(r => _comparer.Equals(r, eventHandler));
            }
        }

        public void Publish(IEvent eventEntry) {
            var eventType = eventEntry.GetType();
            Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} EventBus: 发布事件 {1}",
                DateTime.Now, eventType.Name));
            List<IEventHandler> eventHandlers;
            if (_eventHandlers.TryGetValue(eventType, out eventHandlers)) {
                for (int i = 0; i < eventHandlers.Count; i++) {
                    try {
                        eventHandlers[i].Handle(eventEntry);
                    }
                    catch (Exception ex) {
                        OnErrorOccur(eventHandlers[i], ex, new[] { eventEntry });
                    }
                }
            }
        }

        public void Publish(IList<IEvent> eventEntries) {
            if (eventEntries == null || eventEntries.Count == 0) {
                return;
            }
            var eventType = eventEntries.First().GetType();
            Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} EventBus: 发布事件 {1}",
                DateTime.Now, eventType.Name));
            List<IEventHandler> eventHandlers;
            if (_eventHandlers.TryGetValue(eventType, out eventHandlers)) {
                for (int i = 0; i < eventHandlers.Count; i++) {
                    try {
                        eventHandlers[i].Handle(eventEntries);
                    }
                    catch (Exception ex) {
                        OnErrorOccur(eventHandlers[i], ex, eventEntries.ToArray());
                    }
                }
            }
        }

        public void UnsubscribeAll() {
            Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} EventBus: 取消全部订阅", DateTime.Now));
            _eventHandlers.Clear();
        }

        public void OnErrorOccur(IEventHandler eventHandler, Exception error, params IEvent[] events) {
            if (ErrorHandler != null) {
                ErrorHandler(eventHandler, error, events);
            }

            Int32 number;
            if (!_errors.TryGetValue(eventHandler, out number)) {
                number = 0;
            }
            _errors[eventHandler] = number++;
            if (number >= ErrorCapacity) {
                Unsubscribe(eventHandler);
            }

        }

        internal class EventHandlerEqualityComparer : IEqualityComparer<IEventHandler> {
            private static readonly Type BaseEventHandlerType = typeof(IEventHandler);

            public bool Equals(IEventHandler x, IEventHandler y) {
                return x.GetType() == y.GetType();
            }

            public int GetHashCode(IEventHandler obj) {
                return obj.GetHashCode();
            }
        }
    }
}
