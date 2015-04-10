using ChuyeEventBus.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    public class MessageChannel : IMessageChannel {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        protected static MessageQueueFactory _messageQueueFactory = new MessageQueueFactory();
        
        public const Int32 WaitSpan = 10;
        public String Path { get; private set; }

        public event Action<Message> MessageQueueReceived;

        public MessageChannel(String path) {
            Path = path;
        }

        public virtual void Startup() {
            Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} EventChannel \"{1}\" Startup",
                DateTime.Now, Path));
            var messageQueue = _messageQueueFactory.Apply(Path);
            messageQueue.BeginReceive(TimeSpan.FromSeconds(WaitSpan), messageQueue, new AsyncCallback(MessageQueueEndReceive));
        }

        protected virtual void MessageQueueEndReceive(IAsyncResult ir) {
            ((IMessageChannel)this.Clone()).Startup();

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
        }

        public virtual Object Clone() {
            var channel = new MessageChannel(Path);
            channel.MessageQueueReceived = this.MessageQueueReceived;
            return channel;
        }
    }
}
