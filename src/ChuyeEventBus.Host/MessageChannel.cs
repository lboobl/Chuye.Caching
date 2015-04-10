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
    public interface IMessageChannel {
        void Startup();
        event Action<Message> MessageQueueReceived;
    }

    public interface IMultipleMessageChannel : IMessageChannel {
        event Action<IEnumerable<Message>> MultipleMessageQueueReceived;
    }

    public class MessageChannel : IMessageChannel {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static MessageQueueFactory _messageQueueFactory
            = new MessageQueueFactory();
        public const Int32 WaitSpan = 10;
        private String _path;

        public event Action<Message> MessageQueueReceived;

        public MessageChannel(String path) {
            _path = path;
        }

        public virtual void Startup() {
            Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} EventChannel \"{1}\" Startup",
                DateTime.Now, _path));
            var messageQueue = _messageQueueFactory.Apply(_path);
            messageQueue.BeginReceive(TimeSpan.FromSeconds(WaitSpan), messageQueue, new AsyncCallback(MessageQueueEndReceive));
        }

        protected virtual void MessageQueueEndReceive(IAsyncResult ir) {
            //new EventChannel(_path).Startup();

            MessageQueue messageQueue = null;
            Message message = null;
            try {
                messageQueue = (MessageQueue)ir.AsyncState;
                message = messageQueue.EndReceive(ir);

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
                new MessageChannel(_path).Startup();
            }
        }
    }

    public class MultipleMessageChannel : MessageChannel, IMultipleMessageChannel {
        public event Action<IEnumerable<Message>> MultipleMessageQueueReceived;

        public MultipleMessageChannel(String path)
            : base(path) {
        }

        public override void Startup() {
            base.Startup();
        }

        protected override void MessageQueueEndReceive(IAsyncResult ir) {
            base.MessageQueueEndReceive(ir);
        }
    }
}
