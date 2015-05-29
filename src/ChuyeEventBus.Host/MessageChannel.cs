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
    internal class MessageChannel : IMessageChannel {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly MessageReceiver _msgReceiver;
        private readonly CancellationTokenSource _ctx;
        private readonly IEventBehaviour _eventBehaviour;
        private MessageChannelStatus _status;

        public event Action<Message> MessageReceived;
        public event Action<IList<Message>> MultipleMessageReceived;
        public String FriendlyName { get; private set; }

        public MessageChannel(IEventBehaviour eventBehaviour) {
            var msgQueue = MessageQueueFactory.Build(eventBehaviour);
            FriendlyName = msgQueue.Path.Split('\\').Last();

            _msgReceiver = new MessageReceiver(msgQueue);
            _eventBehaviour = eventBehaviour;
            _ctx = new CancellationTokenSource();

            MessageReceived += x => { };
            MultipleMessageReceived += x => { };
        }

        public async virtual Task ListenAsync() {
            _logger.Debug("MessageChannel: {0} ListenAsync", FriendlyName);
            _status = MessageChannelStatus.Runing;

            while (!_ctx.IsCancellationRequested) {
                var dequeueQuantity = _eventBehaviour.GetDequeueQuantity();
                if (dequeueQuantity == 1) {
                    await ListenOneAsync();
                }
                else {
                    await ListenMutipleAsync(dequeueQuantity);
                }
            }
            _status = MessageChannelStatus.Stoped;
            _logger.Debug("MessageChannel: {0} stoped", FriendlyName);
        }

        private async Task ListenOneAsync() {
            using (var msg = await _msgReceiver.ReceiveAsync()) {
                OnMessageQueueReceived(msg);
            }
        }

        private async Task ListenMutipleAsync(Int32 dequeueQuantity) {
            // MessageReceiver 的出队时间动态修改, 故可以使用 ReceiveAsync() 获取集合
            List<Message> msgs = null;
            try {
                msgs = await _msgReceiver.ReceiveAsync(dequeueQuantity, _ctx.Token);
                OnMultipleMessageQueueReceived(msgs);
            }
            finally {
                if (msgs != null && msgs.Count > 0) {
                    msgs.ForEach(m => m.Dispose());
                }
            }
        }

        private void OnMessageQueueReceived(Message msg) {
            if (msg != null) {
                MessageReceived(msg);
            }
        }

        private void OnMultipleMessageQueueReceived(List<Message> msgs) {
            if (msgs != null && msgs.Count > 0) {
                MultipleMessageReceived(msgs);
            }
        }

        public virtual void Stop() {
            if (!_ctx.IsCancellationRequested) {
                _status = MessageChannelStatus.Suspended;
                _logger.Debug("MessageChannel: {0} suspend", FriendlyName);
                _ctx.Cancel();
            }
        }

        public MessageChannelStatus GetStatus() {
            return _status;
        }
    }
}
