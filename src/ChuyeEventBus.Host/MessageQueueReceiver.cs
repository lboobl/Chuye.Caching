using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    public class MessageQueueReceiver {
        private readonly MessageQueue _messageQueue;
        public const Int32 WaitSpan = 10;

        public MessageQueueReceiver(MessageQueue messageQueue) {
            _messageQueue = messageQueue;
        }

        public async Task<Message> ReceiveAsync() {
            Message msg = null;
            try {
                msg = await Task.Factory.FromAsync<Message>(
                  asyncResult: _messageQueue.BeginReceive(TimeSpan.FromSeconds(WaitSpan)),
                  endMethod: ir => _messageQueue.EndReceive(ir));
            }
            catch (MessageQueueException ex) {
                if (ex.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout) {
                    throw;
                }
            }
            return await Task.FromResult(msg);
        }
    }
}
