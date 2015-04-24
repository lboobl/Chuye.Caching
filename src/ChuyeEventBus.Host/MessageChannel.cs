using ChuyeEventBus.Core;
using NLog;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    public class MessageChannel : IMessageChannel {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        protected readonly MessageReceiver _msgReceiver;
        protected readonly CancellationTokenSource _ctx;

        public event Action<Message> MessageReceived;
        public String FriendlyName { get; private set; }

        public MessageChannel(String friendlyName, MessageReceiver messageReceiver) {
            FriendlyName = friendlyName;
            _msgReceiver = messageReceiver;
            _ctx = new CancellationTokenSource();
            MessageReceived += x => { };
        }

        public async virtual Task ListenAsync() {
            while (!_ctx.IsCancellationRequested) {
                using (Message message = await _msgReceiver.ReceiveAsync()) {
                    if (message != null) {
                        OnMessageQueueReceived(message);
                    }
                }
            }
            _logger.Debug("MessageChannel: {0} stoped", FriendlyName);
        }

        private void OnMessageQueueReceived(Message message) {
            MessageReceived(message);
        }

        public virtual void Stop() {
            if (!_ctx.IsCancellationRequested) {
                _logger.Debug("MessageChannel: {0} suspend", FriendlyName);
                _ctx.Cancel();
            }
        }
    }
}
