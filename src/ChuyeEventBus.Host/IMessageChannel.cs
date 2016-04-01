using ChuyeEventBus.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    internal interface IMessageChannel {
        event Action<IMessageChannel, Exception> ErrorOccured;
        event Action<Message> MessageReceived;
        event Action<IList<Message>> MultipleMessageReceived;

        String FriendlyName { get; }
        Task ListenAsync();
        void Stop(String reason);
        MessageChannelStatus GetStatus();
    }
}
