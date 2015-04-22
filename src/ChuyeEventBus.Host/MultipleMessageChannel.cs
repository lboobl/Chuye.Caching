using ChuyeEventBus.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    public class MultipleMessageChannel : MessageChannel, IMultipleMessageChannel {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly EventBehaviourAttribute _eventBehaviour;
        private readonly MessageQueueReceiver _messageReceiver;
        private readonly List<Message> _localMessages = new List<Message>();
        private readonly ReaderWriterLock _sync = new ReaderWriterLock();

        public event Action<IList<Message>> MultipleMessageQueueReceived;

        public MultipleMessageChannel(EventBehaviourAttribute eventBehaviour)
            : base(eventBehaviour) {
            _eventBehaviour = eventBehaviour;
            _messageReceiver = new MessageQueueReceiver(MessageQueueUtil.ApplyQueue(eventBehaviour));
            //确保 Stop() 方法调用时，本地暂存的 _localMessages 得到处理
            _ctx.Token.Register(OnMultipleMessageQueueReceived);
        }

        public async override Task ListenAsync() {
            while (!_ctx.IsCancellationRequested) {
                Message message = await _messageReceiver.ReceiveAsync();
                if (message != null) {
                    _localMessages.Add(message);
                }
                if (message == null || _localMessages.Count >= _eventBehaviour.DequeueQuantity) {
                    OnMultipleMessageQueueReceived();
                }
            }
            _logger.Debug("MessageChannel: {0} stoped", FriendlyName);
        }

        private void OnMultipleMessageQueueReceived() {
            if (MultipleMessageQueueReceived != null && _localMessages.Count > 0) {
                var array = _localMessages.ToArray();
                ClearLocalMessage();
                MultipleMessageQueueReceived(array);
            }
        }

        private void ClearLocalMessage() {
            _localMessages.ForEach(m => m.Dispose());
            _localMessages.Clear();
        }
    }
}
