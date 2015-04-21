using ChuyeEventBus.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    public class MessageChannel : ISingleMessageChannel {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CancellationTokenSource _ctx;
        private readonly String _label;
        private readonly MessageQueueReceiver _messageReceiver;

        public event EventHandler<Message> MessageQueueReceived;

        public MessageChannel(EventBehaviourAttribute eventBehaviour) {
            _label = eventBehaviour.Label;
            _ctx = new CancellationTokenSource();
            _messageReceiver = new MessageQueueReceiver(MessageQueueUtil.ApplyQueue(eventBehaviour));
        }

        public async virtual Task ListenAsync() {
            while (!_ctx.IsCancellationRequested) {
                using (Message message = await _messageReceiver.ReceiveAsync()) {
                    if (message != null && MessageQueueReceived != null) {
                        MessageQueueReceived(this, message);
                    }
                }
            }
            _logger.Debug("MessageChannel {0} stoped", _label);
        }

        public virtual void Stop() {
            if (!_ctx.IsCancellationRequested) {
                _logger.Debug("MessageChannel {0} pending stop", _label);
                _ctx.Cancel();
            }
        }
    }
}
