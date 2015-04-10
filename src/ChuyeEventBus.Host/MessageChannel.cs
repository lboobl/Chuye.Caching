using ChuyeEventBus.Core;
using NLog;
using System;
using System.Diagnostics;
using System.Messaging;

namespace ChuyeEventBus.Host {
    public class MessageChannel : IMessageChannel {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        protected Func<MessageQueue> _messageQueueFunction;

        public const Int32 WaitSpan = 10;
        public event Action<Message> MessageQueueReceived;

        public MessageChannel(Func<MessageQueue> messageQueueFunction) {
            _messageQueueFunction = messageQueueFunction;
        }

        public virtual void Startup() {
            var messageQueue = _messageQueueFunction();
            messageQueue.BeginReceive(TimeSpan.FromSeconds(WaitSpan), messageQueue, new AsyncCallback(MessageQueueEndReceive));
        }

        protected virtual void MessageQueueEndReceive(IAsyncResult ir) {
            MessageQueue messageQueue = null;
            Message message = null;
            try {
                messageQueue = (MessageQueue)ir.AsyncState;
                message = messageQueue.EndReceive(ir);
                if (MessageQueueReceived != null) {
                    MessageQueueReceived(message);
                }

            }
            catch (MessageQueueException ex) {
                if (ex.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout) {
                    _logger.Error(ex);
                }
            }
            finally {
                if (message != null) {
                    message.Dispose();
                }
            }
            ((IMessageChannel)this.Clone()).Startup();
        }

        public virtual Object Clone() {
            var channel = new MessageChannel(_messageQueueFunction);
            channel.MessageQueueReceived = this.MessageQueueReceived;
            return channel;
        }
    }
}
