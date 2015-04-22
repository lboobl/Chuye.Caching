using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    public interface IMultipleMessageChannel : IMessageChannel {
        event Action<IList<Message>> MultipleMessageQueueReceived;
    }
}
