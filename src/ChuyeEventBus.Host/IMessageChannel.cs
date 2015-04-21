using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    public interface IMessageChannel : ICloneable {
        event EventHandler<Message> MessageQueueReceived;
        Task StartupAsync();
        void Stop();
    }
}
