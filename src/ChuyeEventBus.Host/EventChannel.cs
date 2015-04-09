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
    public class EventChannel {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static MessageQueueFactory _messageQueueFactory
            = new MessageQueueFactory();
        public const Int32 WaitSpan = 10;
        private String _path;

        public EventChannel(String path) {
            _path = path;
        }

        public void Startup() {
            Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} EventChannel \"{1}\" Startup",
                DateTime.Now, _path));
            var messageQueue = _messageQueueFactory.Apply(_path);
            messageQueue.BeginReceive(TimeSpan.FromSeconds(WaitSpan), messageQueue, new AsyncCallback(MessageQueueReceived));
        }

        private void MessageQueueReceived(IAsyncResult ir) {
            //new EventChannel(_path).Startup();

            MessageQueue messageQueue = null;
            Message message = null;
            try {
                messageQueue = (MessageQueue)ir.AsyncState;
                message = messageQueue.EndReceive(ir);

                var @event = message.Body as IEvent;
                if (@event == null) {
                    throw new ArgumentOutOfRangeException(String.Format("Unexpected message type of '{0}'", message.Body.GetType()));
                }
                EventBus.Singleton.Publish(@event);
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
                new EventChannel(_path).Startup();
            }
        }
    }
}
