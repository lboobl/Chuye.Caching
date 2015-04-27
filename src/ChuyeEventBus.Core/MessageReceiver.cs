using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChuyeEventBus.Core {
    public class MessageReceiver {
        private readonly MessageQueue _messageQueue;
        private Int32 MIN_WAIT = 1;
        private Int32 MAX_WAIT = 10;
        private Int32 _currentWait = 2;

        public MessageReceiver(MessageQueue messageQueue) {
            _messageQueue = messageQueue;
        }

        public async Task<Message> ReceiveAsync() {
            Message msg = null;
            try {
                msg = await Task.Factory.FromAsync<Message>(
                  asyncResult: _messageQueue.BeginReceive(TimeSpan.FromSeconds(_currentWait)),
                  endMethod: ir => _messageQueue.EndReceive(ir));
            }
            catch (MessageQueueException ex) {
                if (ex.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout) {
                    throw;
                }
            }
            if (msg != null) {
                if (_currentWait > MIN_WAIT) {
                    _currentWait -= 1;
                }
            }
            else {
                if (_currentWait < MAX_WAIT) {
                    _currentWait += 1;
                }
            }
            return await Task.FromResult(msg);
        }

        public async Task<List<Message>> ReceiveAsync(Int32 dequeueQuantity, CancellationToken token) {
            var msgs = new List<Message>(dequeueQuantity);
            while (!token.IsCancellationRequested && msgs.Count < dequeueQuantity) {
                Message message = await ReceiveAsync();
                if (message != null) {
                    msgs.Add(message);
                }
                else {
                    break;
                }
            }
            return await Task.FromResult(msgs);
        }
    }
}
