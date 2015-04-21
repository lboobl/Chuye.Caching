using ChuyeEventBus.Core;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Messaging;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    public class MultipleMessageChannel : MessageChannel, IMultipleMessageChannel {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly ConcurrentQueue<Message> _messageBag = new ConcurrentQueue<Message>();
        private static readonly Object _sync = new Object();

        public event EventHandler<IList<Message>> MultipleMessageQueueReceived;

        public MultipleMessageChannel(EventBehaviourAttribute eventBehaviour)
            : base(eventBehaviour) {
        }

        public override async Task StartupAsync() {
            var messageQueue = MessageQueueUtil.ApplyQueue(EventBehaviour);
            //messageQueue.BeginReceive(TimeSpan.FromSeconds(WaitSpan), messageQueue, new AsyncCallback(MessageQueueEndReceive));

            Boolean errorOccured = false;
            Message message = null;
            try {
                message = await Task.Factory.FromAsync<Message>(
                   asyncResult: messageQueue.BeginReceive(TimeSpan.FromSeconds(WaitSpan)),
                   endMethod: ir => messageQueue.EndReceive(ir));
                _messageBag.Enqueue(message);
            }
            catch (MessageQueueException ex) {
                errorOccured = true;
                if (ex.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout) {
                    throw;
                }
            }

            //如果出列异常(包含超时)且已经存在临时消息, 则处理掉
            //如果临时消息已塞满,则处理掉
            if (errorOccured || _messageBag.Count >= EventBehaviour.DequeueQuantity) {
                HandleMultipleMessages();
            }

            if (!_cancelSuspend) {
                await ((IMessageChannel)this.Clone()).StartupAsync();
            }
        }

        public override void Stop() {
            HandleMultipleMessages();
            base.Stop();
        }

        private void HandleMultipleMessages() {
            if (_messageBag.Count > 0) {
                lock (_sync) {
                    if (MultipleMessageQueueReceived != null) {
                        var messages = _messageBag.ToArray();
                        ClearMessageBag();
                        MultipleMessageQueueReceived(this, messages);
                    }
                }
            }
        }

        public override object Clone() {
            var channel = new MultipleMessageChannel(EventBehaviour);
            channel.MultipleMessageQueueReceived = this.MultipleMessageQueueReceived;
            return channel;
        }

        private void ClearMessageBag() {
            Message message;
            while (_messageBag.TryDequeue(out message)) {
                message.Dispose();
            }
        }
    }
}
