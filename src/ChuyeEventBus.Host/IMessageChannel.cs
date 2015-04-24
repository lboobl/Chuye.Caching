using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    public interface IMessageChannel {
        event Action<Message> MessageReceived;
        Task ListenAsync();
        void Stop();
    }
}
