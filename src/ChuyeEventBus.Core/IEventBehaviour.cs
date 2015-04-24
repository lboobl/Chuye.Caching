using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Core {

    public interface IEventBehaviour {
        Int32 GetDequeueQuantity();
        String GetMessagePath();
        IMessageFormatter GetMessageFormatter();
    }
}
