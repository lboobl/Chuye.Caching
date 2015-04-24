using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ChuyeEventBus.Core {
    public class EventBus {
        private static readonly EventBus _singleton = new EventBus();
        private readonly EventHandlerEqualityComparer _comparer = new EventHandlerEqualityComparer();
        private Dictionary<Type, List<IEventHandler>> _eventHandlers = new Dictionary<Type, List<IEventHandler>>();
        private Dictionary<IEventHandler, Int32> _errors = new Dictionary<IEventHandler, Int32>();

        public event EventHandler<ErrorOccuredEventArgs> ErrorOccured;

        public static EventBus Singleton {
            get { return _singleton; }
        }

        private EventBus() {
            ErrorOccured += (x, y) => { };
        }

        public void Subscribe(IEventHandler eventHandler) {
            var eventType = eventHandler.GetEventType();
            List<IEventHandler> eventHandlers;
            if (_eventHandlers.TryGetValue(eventType, out eventHandlers)) {
                if (!eventHandlers.Contains(eventHandler, _comparer)) {
                    Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} EventBus: 添加订阅 {1} <- {2}",
                        DateTime.Now, eventType.FullName, eventHandler.GetType().FullName));
                    eventHandlers.Add(eventHandler);
                }
            }
            else {
                eventHandlers = new List<IEventHandler>();
                eventHandlers.Add(eventHandler);
                Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} EventBus: 注册事件 {1}",
                    DateTime.Now, eventType.FullName));
                Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} EventBus: 添加订阅 {1} <- {2}",
                    DateTime.Now, eventType.FullName, eventHandler.GetType().FullName));
                _eventHandlers.Add(eventType, eventHandlers);
            }
        }

        public void Unsubscribe(IEventHandler eventHandler) {
            var eventType = eventHandler.GetEventType();
            List<IEventHandler> eventHandlers;
            if (_eventHandlers.TryGetValue(eventType, out eventHandlers)) {
                Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} EventBus: 取消订阅 {1} <- {2}",
                    DateTime.Now, eventType.FullName, eventHandler.GetType().FullName));
                eventHandlers.RemoveAll(r => _comparer.Equals(r, eventHandler));
                if (eventHandlers.Count == 0) {
                    _eventHandlers.Remove(eventType);
                }
            }
        }

        public void Publish(IEvent eventEntry) {
            var eventType = eventEntry.GetType();
            List<IEventHandler> eventHandlers;
            if (_eventHandlers.TryGetValue(eventType, out eventHandlers)) {
                Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} EventBus: 发布事件 {1}",
                    DateTime.Now, eventType.FullName));

                //Task.Factory.StartNew(() => {
                //    foreach (var eh in eventHandlers) {
                //        new Task(() => SafelyHandle(eh, eventEntry), TaskCreationOptions.AttachedToParent).Start();
                //    }
                //}).Wait();

                var tasks = eventHandlers.Select(async eh => await Task.Run(() => SafelyHandle(eh, eventEntry)));
                Task.WaitAll(tasks.ToArray());

            }
        }

        private void SafelyHandle(IEventHandler eventHandler, IEvent eventEntry) {
            try {
                eventHandler.Handle(eventEntry);
            }
            catch (Exception ex) {
                OnErrorOccur(eventHandler, new[] { eventEntry }, ex);
            }
        }

        private void SafelyHandle(IEventHandler eventHandler, IList<IEvent> eventEntries) {
            try {
                eventHandler.Handle(eventEntries);
            }
            catch (Exception ex) {
                OnErrorOccur(eventHandler, eventEntries, ex);
            }
        }


        public void Publish(IList<IEvent> eventEntries) {
            var eventType = eventEntries.First().GetType();
            List<IEventHandler> eventHandlers;
            if (_eventHandlers.TryGetValue(eventType, out eventHandlers)) {
                Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} EventBus: 发布事件 {1}",
                    DateTime.Now, eventType.FullName));

                var tasks = eventHandlers.Select(async eh => await Task.Run(() => SafelyHandle(eh, eventEntries)));
                Task.WaitAll(tasks.ToArray());
            }
        }

        public void UnsubscribeAll() {
            Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} EventBus: 取消全部订阅", DateTime.Now));
            _eventHandlers.Clear();
        }

        private void OnErrorOccur(IEventHandler eventHandler, IList<IEvent> events, params Exception[] errors) {
            Int32 number;
            if (!_errors.TryGetValue(eventHandler, out number)) {
                number = 1;
                _errors.Add(eventHandler, 1);
            }
            else {
                number = ++_errors[eventHandler];
            }
            ErrorOccured(this, new ErrorOccuredEventArgs(eventHandler, events, number, errors));
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

    public class ErrorOccuredEventArgs : EventArgs {
        public IEventHandler EventHandler { get; private set; }
        public IList<IEvent> Events { get; private set; }
        public Int32 TotoalErrors { get; private set; }
        public IList<Exception> Errors { get; private set; }

        public ErrorOccuredEventArgs(IEventHandler eventHandler, IList<IEvent> events, Int32 errorNumber, params Exception[] errors) {
            EventHandler = eventHandler;
            Events = events;
            TotoalErrors = errorNumber;
            Errors = errors;
        }
    }
}
