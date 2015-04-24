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
        private readonly List<Message> _localMsgs = new List<Message>();

        public event Action<Message> MessageReceived;
        public event Action<IList<Message>> MultipleMessageReceived;
        public String FriendlyName { get; private set; }

        public MessageChannel(IEventBehaviour eventBehaviour) {
            var msgQueue = MessageQueueFactory.Build(eventBehaviour);
            FriendlyName = msgQueue.Path.Split('\\').Last();

            _msgReceiver = new MessageReceiver(msgQueue);
            _eventBehaviour = eventBehaviour;
            _ctx = new CancellationTokenSource();
            _ctx.Token.Register(OnMultipleMessageQueueReceived); 

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

        //private async Task ListenMutipleAsync(Int32 dequeueQuantity) {
        //    //为了达到 Stop()方法调用时能立即处理局部 Message 列表的能力,
        //    //放弃了 MessageReceiver.ReceiveAsync() 获取集合的使用
        //    var msgs = await _msgReceiver.ReceiveAsync(dequeueQuantity, _ctx.Token);
        //    OnMultipleMessageQueueReceived(msgs);
        //}

        public async Task ListenMutipleAsync(Int32 dequeueQuantity) {
            while (!_ctx.IsCancellationRequested) {
                var msg = await _msgReceiver.ReceiveAsync();
                if (msg != null) {
                    _localMsgs.Add(msg);
                }
                if (msg == null || _localMsgs.Count >= dequeueQuantity) {
                    OnMultipleMessageQueueReceived();
                }
            }
            OnMultipleMessageQueueReceived(); //保证取消后，处理已经出列的数据
            _logger.Debug("MessageChannel: {0} stoped", FriendlyName);
        }

        private void OnMessageQueueReceived(Message msg) {
            if (msg != null) {
                MessageReceived(msg);
            }
        }

        private void OnMultipleMessageQueueReceived() {
            if (_localMsgs.Count > 0) {
                MultipleMessageReceived(_localMsgs);
                ReleaseMessages();
            }
        }

        private void ReleaseMessages() {
            _localMsgs.ForEach(m => m.Dispose());
            _localMsgs.Clear();
        }

        public virtual void Stop() {
            if (!_ctx.IsCancellationRequested) {
                _logger.Debug("MessageChannel: {0} suspend", FriendlyName);
                _ctx.Cancel();
            }
        }
    }
}
