using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChuyeEventBus.Core {
    public class MessageReceiver {
        protected readonly MessageQueue _messageQueue;
        protected const Int32 WaitSpan = 5;

        public MessageReceiver(MessageQueue messageQueue) {
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

        public async Task<List<Message>> ReceiveAsync(Int32 dequeueQuantity, CancellationToken token) {
            var localMessages = new List<Message>(dequeueQuantity);
            while (!token.IsCancellationRequested && localMessages.Count < dequeueQuantity) {
                Message message = await ReceiveAsync();
                if (message != null) {
                    localMessages.Add(message);
                }
                else {
                    break;
                }
            }
            return await Task.FromResult(localMessages);
        }
    }
}
