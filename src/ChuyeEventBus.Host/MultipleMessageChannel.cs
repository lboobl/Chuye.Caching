using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Messaging;

namespace ChuyeEventBus.Host {
    public class MultipleMessageChannel : MessageChannel, IMultipleMessageChannel {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly ConcurrentQueue<Message> _messageBag = new ConcurrentQueue<Message>();
        private static readonly Object _sync = new Object();

        public Int32 Quantity { get; private set; }
        public event Action<IEnumerable<Message>> MultipleMessageQueueReceived;

        public MultipleMessageChannel(Func<MessageQueue> messageQueueFunction, Int32 quantity)
            : base(messageQueueFunction) {
            if (quantity < 1 || quantity > 10000) {
                throw new ArgumentOutOfRangeException("quantity");
            }
            Quantity = quantity;
        }

        public override void Startup() {
            var messageQueue = _messageQueueFunction();
            messageQueue.BeginReceive(TimeSpan.FromSeconds(WaitSpan), messageQueue, new AsyncCallback(MessageQueueEndReceive));
        }

        protected override void MessageQueueEndReceive(IAsyncResult ir) {
            try {
                var messageQueue = (MessageQueue)ir.AsyncState;
                var message = messageQueue.EndReceive(ir);

                if (MultipleMessageQueueReceived != null) {
                    if (_messageBag.Count < Quantity) {
                        _messageBag.Enqueue(message);
                    }
                    else {
                        HandleMultipleMessages();
                    }
                }
            }
            catch (MessageQueueException ex) {
                if (ex.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout) {
                    _logger.Error(ex);
                }
                HandleMultipleMessages();
            }

            ((IMessageChannel)this.Clone()).Startup();
        }

        private void HandleMultipleMessages() {
            if (MultipleMessageQueueReceived != null) {
                if (_messageBag.Count > 0) {
                    lock (_sync) {
                        var messages = _messageBag.ToArray();
                        MultipleMessageQueueReceived(messages);
                        ClearMessageBag();
                    }
                }
            }
        }

        public override object Clone() {
            var channel = new MultipleMessageChannel(_messageQueueFunction, Quantity);
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
