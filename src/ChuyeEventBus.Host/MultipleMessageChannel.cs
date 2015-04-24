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
        private List<Message> _localMsgs = new List<Message>();
        private Int32 _dequeueQuantity;

        public event Action<IList<Message>> MultipleMessageReceived;

        public MultipleMessageChannel(String friendlyName, MessageReceiver messageReceiver, Int32 dequeueQuantity)
            : base(friendlyName, messageReceiver) {
            _dequeueQuantity = dequeueQuantity;
            _ctx.Token.Register(OnMultipleMessageQueueReceived);
            MultipleMessageReceived += x => { };
        }

        //为了达到 Stop()方法调用时能立即处理局部 Message 列表的能力 
        //放弃了 MessageReceiver.ReceiveAsync(Int32 dequeueQuantity, CancellationTokenSource ctx) 方法的使用
        public async override Task ListenAsync() {
            while (!_ctx.IsCancellationRequested) {
                Message message = await _msgReceiver.ReceiveAsync();
                if (message != null) {
                    _localMsgs.Add(message);
                }
                if (message == null || _localMsgs.Count >= _dequeueQuantity) {
                    OnMultipleMessageQueueReceived();
                }
            }
            _logger.Debug("MessageChannel: {0} stoped", FriendlyName);
        }


        private void OnMultipleMessageQueueReceived() {
            if (_localMsgs.Count > 0) {
                MultipleMessageReceived(_localMsgs);
                ClearLocalMessage();
            }
        }

        private void ClearLocalMessage() {
            _localMsgs.ForEach(m => m.Dispose());
            _localMsgs.Clear();
        }
    }
}
