using ChuyeEventBus.Core;
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
        public event EventHandler<IList<Message>> MultipleMessageQueueReceived;

        public MultipleMessageChannel(EventBehaviourAttribute eventBehaviour)
            : base(eventBehaviour) {
        }

        public override void Startup() {
            var messageQueue = MessageQueueUtil.ApplyQueue(_eventBehaviour);
            messageQueue.BeginReceive(TimeSpan.FromSeconds(WaitSpan), messageQueue, new AsyncCallback(MessageQueueEndReceive));
        }

        public override void Stop() {
            HandleMultipleMessages();
            base.Stop();
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
            if (!_cancelSuspend) {
                ((IMessageChannel)this.Clone()).Startup();
            }
        }

        private void HandleMultipleMessages() {
            if (MultipleMessageQueueReceived != null) {
                if (_messageBag.Count > 0) {
                    lock (_sync) {
                        var messages = _messageBag.ToArray();
                        ClearMessageBag();
                        MultipleMessageQueueReceived(this, messages);
                    }
                }
            }
        }

        public override object Clone() {
            var channel = new MultipleMessageChannel(_eventBehaviour);
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
