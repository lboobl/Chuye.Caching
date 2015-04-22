using ChuyeEventBus.Core;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Messaging;
using System.Text;

namespace ChuyeEventBus.Host {
    public class MessageChannelServer {
        private const Int32 ERROR_CAPACITY = 3;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

#pragma warning disable
        [ImportMany]
        private IEnumerable<IEventHandler> _handlers;
#pragma warning disable
        private Boolean _initialized = false;
        private readonly List<IMessageChannel> _channels = new List<IMessageChannel>();
        private readonly Dictionary<Type, IMessageChannel> _maps = new Dictionary<Type, IMessageChannel>();

        public String Folder { get; set; }

        public void Initialize() {
            Initialize(false);
        }

        public void Initialize(Boolean rescan) {
            EventBus.Singleton.UnsubscribeAll();
            EventBus.Singleton.ErrorOccured += Singleton_ErrorOccured;

            if (!_initialized || rescan) {
                var catalog = new AggregateCatalog();
                catalog.Catalogs.Add(new DirectoryCatalog(Folder));
                var container = new CompositionContainer(catalog);
                container.ComposeParts(this);

                foreach (var handler in _handlers) {
                    EventBus.Singleton.Subscribe(handler);
                }

                var hgs = _handlers.GroupBy(r => r.GetEventType());
                foreach (var hg in hgs) {
                    var eventType = hg.Key;
                    var eventBehaviour = EventExtension.BuildEventBehaviour(eventType);
                    if (eventBehaviour.DequeueQuantity == 1) {
                        ISingleMessageChannel channel = new MessageChannel(eventBehaviour);
                        channel.MessageReceived += Channel_MessageReceived;
                        _channels.Add(channel);
                        _maps.Add(eventType, channel);
                    }
                    else {
                        IMultipleMessageChannel channel = new MultipleMessageChannel(eventBehaviour);
                        channel.MultipleMessageReceived += Channel_MultipleMessageReceived;
                        _channels.Add(channel);
                        _maps.Add(eventType, channel);
                    }
                }
                _channels.ForEach(c => c.ListenAsync());
                _initialized = true;
            }
        }

        private void Singleton_ErrorOccured(Object sender, ErrorOccuredEventArgs e) {
            var errorDetailBuilder = new StringBuilder();
            errorDetailBuilder.AppendFormat("Error occured in {0}\r\n", e.EventHandler.GetType().FullName);
            errorDetailBuilder.AppendFormat("Event: {0}\r\n", JsonConvert.SerializeObject(e.Events));
            foreach (var ex in e.Errors) {
                errorDetailBuilder.AppendLine(ex.ToString());
            }
            _logger.Error(errorDetailBuilder);
            if (e.TotoalErrors >= ERROR_CAPACITY) {
                EventBus.Singleton.Unsubscribe(e.EventHandler);
                var eventType = e.EventHandler.GetEventType();
                _channels.Remove(_maps[eventType]);
                _maps.Remove(eventType);
            }
        }

        private void Channel_MessageReceived(Message message) {
            var eventEntry = (IEvent)message.Body;
            EventBus.Singleton.Publish(eventEntry);
        }

        private void Channel_MultipleMessageReceived(IList<Message> messages) {
            var eventEntries = messages.Select(m => m.Body).Cast<IEvent>().ToList();
            EventBus.Singleton.Publish(eventEntries);
        }

        public void Stop() {
            _channels.ForEach(c => c.Stop());
        }
    }
}
