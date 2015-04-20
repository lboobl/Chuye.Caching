using ChuyeEventBus.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Messaging;

namespace ChuyeEventBus.Host {
    public class MessageChannelServer {
#pragma warning disable
        [ImportMany]
        private IEnumerable<IEventHandler> _handlers;
#pragma warning disable
        private Boolean _initialized = false;
        private CancellationStatus _cancellationStatus = CancellationStatus.None;

        public IEnumerable<IEventHandler> Handlers {
            get {
                return _handlers;
            }
        }

        public String Folder { get; set; }

        public void Initialize() {
            Initialize(false);
        }

        public void Initialize(Boolean rescan) {
            if (!_initialized || rescan) {
                var catalog = new AggregateCatalog();
                catalog.Catalogs.Add(new DirectoryCatalog(Folder));
                var container = new CompositionContainer(catalog);
                container.ComposeParts(this);

                EventBus.Singleton.UnsubscribeAll();
                foreach (var handler in Handlers) {
                    EventBus.Singleton.Subscribe(handler);
                }

                var handlerGroups = Handlers.GroupBy(r => r.GetEventType());
                foreach (var handlerGroup in handlerGroups) {
                    var eventType = handlerGroup.Key;
                    var eventBehaviour = EventExtension.BuildEventBehaviour(eventType);
                    if (eventBehaviour.DequeueQuantity == 1) {
                        foreach (var handler in handlerGroups) {
                            IMessageChannel channel = new MessageChannel(eventBehaviour);
                            channel.MessageQueueReceived += channel_MessageQueueReceived;
                            channel.Startup();
                        }
                    }
                    else {
                        foreach (var handler in handlerGroups) {
                            IMultipleMessageChannel channel = new MultipleMessageChannel(eventBehaviour);
                            channel.MessageQueueReceived += channel_MessageQueueReceived;
                            channel.MultipleMessageQueueReceived += channel_MultipleMessageQueueReceived;
                            channel.Startup();
                        }
                    }


                }
                _initialized = true;
            }
        }

        private void channel_MessageQueueReceived(Object sender, Message message) {
            var eventEntry = message.Body as IEvent;
            if (eventEntry == null) {
                throw new ArgumentOutOfRangeException(String.Format("Unexpected message type of '{0}'", message.Body.GetType()));
            }

            EventBus.Singleton.Publish(eventEntry);
            if (_cancellationStatus == CancellationStatus.CancelSuspend) {
                ((IMessageChannel)sender).Stop();
            }
        }

        private void channel_MultipleMessageQueueReceived(Object sender, IList<Message> messages) {
            var eventEntries = messages.Select(m => m.Body as IEvent).ToList();
            EventBus.Singleton.Publish(eventEntries);
            if (_cancellationStatus == CancellationStatus.CancelSuspend) {
                ((IMessageChannel)sender).Stop();
            }
        }

        public void Stop() {
            if (_cancellationStatus == CancellationStatus.None) {
                _cancellationStatus = CancellationStatus.CancelSuspend;
            }
            else if (_cancellationStatus == CancellationStatus.CancelSuspend) {
                //EventBus.Singleton.UnsubscribeAll();
            }
        }

        internal enum CancellationStatus {
            None, CancelSuspend, Canceled
        }
    }
}
