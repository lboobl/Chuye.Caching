using ChuyeEventBus.Core;
using ChuyeEventBus.Plugin;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading;

namespace ChuyeEventBus.Host {
    public class MessageChannelServer : PluginCatalog<IEventHandler> {
        private const Int32 ERROR_CAPACITY = 3;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly List<IMessageChannel> _channels = new List<IMessageChannel>();
        private readonly Dictionary<Type, IMessageChannel> _channelMaps = new Dictionary<Type, IMessageChannel>();
        
        public void StartAsync() {
            EventBus.Singleton.UnsubscribeAll();
            EventBus.Singleton.ErrorOccured += Singleton_ErrorOccured;

            var eventHandlers = FindPlugins();
            foreach (var handler in eventHandlers) {
                EventBus.Singleton.Subscribe(handler);
            }

            var hgs = eventHandlers.GroupBy(r => r.GetEventType());
            foreach (var hg in hgs) {
                var eventBehaviour = EventExtension.GetEventBehaviour(hg.Key);
                var msgChannel = new MessageChannel(eventBehaviour);
                msgChannel.MessageReceived += Channel_MessageReceived;
                msgChannel.MultipleMessageReceived += Channel_MultipleMessageReceived;

                _channels.Add(msgChannel);
                _channelMaps.Add(hg.Key, msgChannel);
            }
            _channels.ForEach(c => c.ListenAsync());
        }

        private void Singleton_ErrorOccured(Object sender, ErrorOccuredEventArgs e) {
            var errorDetailBuilder = new StringBuilder();
            errorDetailBuilder.AppendFormat("Error occured in {0}\r\n", e.EventHandler.GetType().FullName);
            errorDetailBuilder.AppendFormat("Event: {0}\r\n", JsonConvert.SerializeObject(e.Events));
            foreach (var ex in e.Errors) {
                errorDetailBuilder.AppendLine(ex.ToString());
            }
            _logger.Error(errorDetailBuilder);

            if (e.TotalErrors >= ERROR_CAPACITY) {
                EventBus.Singleton.Unsubscribe(e.EventHandler);
                var eventType = e.EventHandler.GetEventType();
                _channels.Remove(_channelMaps[eventType]);
                _channelMaps.Remove(eventType);
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

        public void StopChannels() {
            _channels.ForEach(c => c.Stop());
            var allStoped = _channels.All(c => c.GetStatus() == MessageChannelStatus.Stoped);
            while (!allStoped) {
                Thread.Sleep(1000);
                allStoped = _channels.All(c => c.GetStatus() == MessageChannelStatus.Stoped);
            }
        }
    }
}
