using ChuyeEventBus.Core;
using NLog;
using System;
using System.Diagnostics;
using System.Messaging;

namespace ChuyeEventBus.Host {
    public class MessageChannel : IMessageChannel {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        protected Boolean _cancelSuspend = false;
        protected readonly EventBehaviourAttribute _eventBehaviour;

        public const Int32 WaitSpan = 10;
        public event EventHandler<Message> MessageQueueReceived;

        public MessageChannel(EventBehaviourAttribute eventBehaviour) {
            _eventBehaviour = eventBehaviour;
        }

        public virtual void Startup() {
            var messageQueue = MessageQueueUtil.ApplyQueue(_eventBehaviour);
            messageQueue.BeginReceive(TimeSpan.FromSeconds(WaitSpan), messageQueue, new AsyncCallback(MessageQueueEndReceive));
        }

        public virtual void Stop() {
            _cancelSuspend = true;
        }

        protected virtual void MessageQueueEndReceive(IAsyncResult ir) {
            MessageQueue messageQueue = null;
            Message message = null;
            try {
                messageQueue = (MessageQueue)ir.AsyncState;
                message = messageQueue.EndReceive(ir);
                if (MessageQueueReceived != null) {
                    MessageQueueReceived(this, message);
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
            if (!_cancelSuspend) {
                ((IMessageChannel)this.Clone()).Startup();
            }
        }

        public virtual Object Clone() {
            var channel = new MessageChannel(_eventBehaviour);
            channel.MessageQueueReceived = this.MessageQueueReceived;
            return channel;
        }
    }
}
