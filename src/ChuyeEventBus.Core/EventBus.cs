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
        private static readonly EventHandlerEqualityComparer _comparer = new EventHandlerEqualityComparer();
        private Dictionary<Type, List<IEventHandler>> _eventHandlers = new Dictionary<Type, List<IEventHandler>>();
        public Action<IEventHandler, Exception> ErrorHandler;

        public static EventBus Singleton {
            get { return _singleton; }
        }

        private EventBus() {
        }

        public void Subscribe(IEventHandler eventHandler) {
            if (typeof(IEventHandler<>).IsAssignableFrom(eventHandler.GetType())) {
                throw new ArgumentOutOfRangeException("eventHandler",
                    String.Format("eventHandler must instance of IEventHandler<{0}>", eventHandler.EventType.FullName));
            }

            List<IEventHandler> eventHandlers;
            if (_eventHandlers.TryGetValue(eventHandler.EventType, out eventHandlers)) {
                if (!eventHandlers.Contains(eventHandler, _comparer)) {
                    Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} EventBus: 添加订阅 {1} for {2}",
                        DateTime.Now, eventHandler.GetType().Name, eventHandler.EventType.Name));
                    eventHandlers.Add(eventHandler);
                }
            }
            else {
                eventHandlers = new List<IEventHandler>();
                eventHandlers.Add(eventHandler);
                Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} EventBus: 注册事件 {1}",
                    DateTime.Now, eventHandler.EventType.Name));
                Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} EventBus: 添加订阅 {1} for {2}",
                    DateTime.Now, eventHandler.GetType().Name, eventHandler.EventType.Name));
                _eventHandlers.Add(eventHandler.EventType, eventHandlers);
            }
        }

        public void Unsubscribe(IEventHandler eventHandler) {
            List<IEventHandler> eventHandlers;
            if (_eventHandlers.TryGetValue(eventHandler.EventType, out eventHandlers)) {
                Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} EventBus: 取消订阅 {1} for {2}",
                    DateTime.Now, eventHandler.GetType().Name, eventHandler.EventType.Name));
                eventHandlers.RemoveAll(r => _comparer.Equals(r, eventHandler));
            }
        }

        public void Publish(IEvent eventEntry) {
            var eventType = eventEntry.GetType();
            Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} EventBus: 发布事件 {1}",
                DateTime.Now, eventType.Name));
            List<IEventHandler> eventhandlers;
            if (_eventHandlers.TryGetValue(eventType, out eventhandlers)) {
                foreach (var eventHandler in eventhandlers) {
                    try {
                        eventHandler.Handle(eventEntry);
                    }
                    catch (Exception ex) {
                        OnErrorOccur(eventHandler, ex);
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
            List<IEventHandler> eventhandlers;
            if (_eventHandlers.TryGetValue(eventType, out eventhandlers)) {
                foreach (var eventHandler in eventhandlers) {
                    try {
                        eventHandler.Handle(eventEntries);
                    }
                    catch (Exception ex) {
                        OnErrorOccur(eventHandler, ex);
                    }
                }
            }
        }

        public void UnsubscribeAll() {
            //foreach (var eventHandlers in _eventHandlers.Values) {
            //    foreach (var eventHandler in eventHandlers) {
            //        try {
            //            //?
            //        }
            //        catch (Exception ex) {
            //            OnErrorOccur(ex);
            //        }
            //    }
            //}
            _eventHandlers.Clear();
        }

        public void OnErrorOccur(IEventHandler eventHandler, Exception error) {
            Debug.WriteLine(error.ToString());
            if (ErrorHandler != null) {
                ErrorHandler(eventHandler, error);
            }
        }

        internal class EventHandlerEqualityComparer : IEqualityComparer<IEventHandler> {
            private static readonly Type BaseEventHandlerType = typeof(IEventHandler);

            public bool Equals(IEventHandler x, IEventHandler y) {
                return x.EventType == y.EventType && x.GetType() == y.GetType();
            }

            public int GetHashCode(IEventHandler obj) {
                return obj.GetHashCode();
            }
        }
    }
}
