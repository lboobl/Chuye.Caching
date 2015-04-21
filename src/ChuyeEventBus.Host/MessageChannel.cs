using ChuyeEventBus.Core;
using NLog;
using System;
using System.Diagnostics;
using System.Messaging;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    public class MessageChannel : IMessageChannel {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly EventBehaviourAttribute _eventBehaviour;
        protected Boolean _cancelSuspend = false;

        public const Int32 WaitSpan = 10;
        public event EventHandler<Message> MessageQueueReceived;

        public EventBehaviourAttribute EventBehaviour {
            get { return _eventBehaviour; }
        }

        public MessageChannel(EventBehaviourAttribute eventBehaviour) {
            _eventBehaviour = eventBehaviour;
        }

        public async virtual Task StartupAsync() {
            var messageQueue = MessageQueueUtil.ApplyQueue(_eventBehaviour);
            Message message = null;
            try {
                message = await Task.Factory.FromAsync<Message>(
                   asyncResult: messageQueue.BeginReceive(TimeSpan.FromSeconds(WaitSpan)),
                   endMethod: ir => messageQueue.EndReceive(ir));

                if (MessageQueueReceived != null) {
                    MessageQueueReceived(this, message);
                }
            }
            catch (MessageQueueException ex) {
                if (ex.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout) {
                    throw;
                }
            }
            finally {
                if (message != null) {
                    message.Dispose();
                }
            }

            if (!_cancelSuspend) {
                await ((IMessageChannel)this.Clone()).StartupAsync();
            }
        }

        public virtual void Stop() {
            _cancelSuspend = true;
        }

        public virtual Object Clone() {
            var channel = new MessageChannel(_eventBehaviour);
            channel.MessageQueueReceived = this.MessageQueueReceived;
            return channel;
        }
    }
}
