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
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly MessageReceiver _msgReceiver;
        private readonly CancellationTokenSource _ctx;
        private readonly IEventBehaviour _eventBehaviour;

        public event Action<Message> MessageReceived;
        public event Action<IList<Message>> MultipleMessageReceived;
        public String FriendlyName { get; private set; }

        public MessageChannel(IEventBehaviour eventBehaviour) {
            var msgQueue = MessageQueueFactory.Build(eventBehaviour);
            FriendlyName = msgQueue.Path.Split('\\').Last();

            _msgReceiver = new MessageReceiver(msgQueue);
            _eventBehaviour = eventBehaviour;
            _ctx = new CancellationTokenSource();

            //不可以想办法在取消时立即处理临时存储的消息,
            //因为可能某个消息正在出列
            //_ctx.Token.Register(OnMultipleMessageQueueReceived); 

            MessageReceived += x => { };
            MultipleMessageReceived += x => { };
        }

        public async virtual Task ListenAsync() {
            while (!_ctx.IsCancellationRequested) {
                var dequeueQuantity = _eventBehaviour.GetDequeueQuantity();
                if (dequeueQuantity == 1) {
                    await ListenOneAsync();
                }
                else {
                    await ListenMutipleAsync(dequeueQuantity);
                }
            }
            _logger.Debug("MessageChannel: {0} stoped", FriendlyName);
        }

        private async Task ListenOneAsync() {
            using (var msg = await _msgReceiver.ReceiveAsync()) {
                OnMessageQueueReceived(msg);
            }
        }

        private async Task ListenMutipleAsync(Int32 dequeueQuantity) {
            // MessageReceiver 的出队时间动态修改, 故可以使用 ReceiveAsync() 获取集合
            var msgs = await _msgReceiver.ReceiveAsync(dequeueQuantity, _ctx.Token);
            OnMultipleMessageQueueReceived(msgs);
        }

        private void OnMessageQueueReceived(Message msg) {
            if (msg != null) {
                MessageReceived(msg);
            }
        }

        private void OnMultipleMessageQueueReceived(List<Message> msgs) {
            if (msgs != null && msgs.Count > 0) {
                MultipleMessageReceived(msgs);
                ReleaseMessages(msgs);
            }
        }

        private void ReleaseMessages(List<Message> msgs) {
            msgs.ForEach(m => m.Dispose());
        }

        public virtual void Stop() {
            if (!_ctx.IsCancellationRequested) {
                _logger.Debug("MessageChannel: {0} suspend", FriendlyName);
                _ctx.Cancel();
            }
        }
    }
}
