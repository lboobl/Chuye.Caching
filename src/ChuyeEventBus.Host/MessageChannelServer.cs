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
        private readonly List<IMessageChannel> _channels = new List<IMessageChannel>();

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
                foreach (var handler in _handlers) {
                    EventBus.Singleton.Subscribe(handler);
                }

                var hgs = _handlers.GroupBy(r => r.GetEventType());
                foreach (var hg in hgs) {
                    var eventType = hg.Key;
                    var eventBehaviour = EventExtension.BuildEventBehaviour(eventType);
                    if (eventBehaviour.DequeueQuantity == 1) {
                        foreach (var handler in hg) {
                            ISingleMessageChannel channel = new MessageChannel(eventBehaviour);
                            channel.MessageQueueReceived += channel_MessageQueueReceived;
                            _channels.Add(channel);
                        }
                    }
                    else {
                        foreach (var handler in hg) {
                            IMultipleMessageChannel channel = new MultipleMessageChannel(eventBehaviour);
                            channel.MultipleMessageQueueReceived += channel_MultipleMessageQueueReceived;
                            _channels.Add(channel);
                        }
                    }
                }
                _channels.ForEach(c => c.ListenAsync());
                _initialized = true;
            }
        }

        private void channel_MessageQueueReceived(Message message) {
            var eventEntry = (IEvent)message.Body;
            EventBus.Singleton.Publish(eventEntry);
        }

        private void channel_MultipleMessageQueueReceived(IList<Message> messages) {
            var eventEntries = messages.Select(m => m.Body).Cast<IEvent>().ToList();
            EventBus.Singleton.Publish(eventEntries);
        }

        public void Stop() {
            _channels.ForEach(c => c.Stop());
        }
    }
}
