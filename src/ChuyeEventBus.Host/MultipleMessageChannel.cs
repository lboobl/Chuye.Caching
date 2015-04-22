using ChuyeEventBus.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    public class MultipleMessageChannel : MessageChannel, IMultipleMessageChannel {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly EventBehaviourAttribute _eventBehaviour;
        private readonly MessageQueueReceiver _messageReceiver;
        private readonly List<Message> _localMessages = new List<Message>();

        public event EventHandler<IList<Message>> MultipleMessageQueueReceived;

        public MultipleMessageChannel(EventBehaviourAttribute eventBehaviour)
            : base(eventBehaviour) {
            _messageReceiver = new MessageQueueReceiver(MessageQueueUtil.ApplyQueue(eventBehaviour));
        }

        public async override Task ListenAsync() {
            while (!_ctx.IsCancellationRequested) {
                Message message = await _messageReceiver.ReceiveAsync();
                if (message != null) {
                    _localMessages.Add(message);
                }
                if ((message == null && _localMessages.Count > 0) || _localMessages.Count >= _eventBehaviour.DequeueQuantity) {
                    OnMultipleMessageQueueReceived();
                }
            }
            _logger.Debug("MessageChannel: {0,-50}  stoped", FriendlyName);
        }

        public override void Stop() {
            base.Stop();
            OnMultipleMessageQueueReceived();
        }

        private void OnMultipleMessageQueueReceived() {
            if (MultipleMessageQueueReceived != null) {
                MultipleMessageQueueReceived(this, _localMessages);
                ClearLocalMessage();
            }
        }

        private void ClearLocalMessage() {
            foreach (var msg in _localMessages) {
                msg.Dispose();
            }
            _localMessages.Clear();
        }
    }
}
