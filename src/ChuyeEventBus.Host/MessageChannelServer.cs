using ChuyeEventBus.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    public class MessageChannelServer {
#pragma warning disable
        [ImportMany]
        private IEnumerable<IEventHandler> _handlers;
#pragma warning disable
        private Boolean _initialized = false;

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
                //catalog.Catalogs.Add(new AssemblyCatalog(typeof(FirstPageWorks.ServiceLocator).Assembly));
                var container = new CompositionContainer(catalog);
                container.ComposeParts(this);

                EventBus.Singleton.UnsubscribeAll();
                foreach (var handler in Handlers) {
                    EventBus.Singleton.Subscribe(handler);
                }

                foreach (var handler in Handlers) {
                    var path = MessageQueueUtil.GetPath(handler.EventType);
                    //new EventChannel(path).Startup();

                    if (!handler.SupportMultiple) {
                        IMessageChannel channel = new MessageChannel(path);
                        channel.MessageQueueReceived += channel_MessageQueueReceived;
                    }
                    else {
                        IMultipleMessageChannel channel = new MultipleMessageChannel(path);
                        channel.MessageQueueReceived += channel_MessageQueueReceived;
                        channel.MultipleMessageQueueReceived += channel_MultipleMessageQueueReceived;
                    }
                }
            }
        }

        void channel_MessageQueueReceived(Message message) {
            var @event = message.Body as IEvent;
            if (@event == null) {
                throw new ArgumentOutOfRangeException(String.Format("Unexpected message type of '{0}'", message.Body.GetType()));
            }
            EventBus.Singleton.Publish(@event);
        }



        void channel_MultipleMessageQueueReceived(IEnumerable<Message> messages) {
            var events = messages.Select(m => m.Body as IEvent);
            EventBus.Singleton.Publish(events);
        }
    }
}
