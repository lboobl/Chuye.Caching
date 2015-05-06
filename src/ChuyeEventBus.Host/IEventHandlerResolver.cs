using ChuyeEventBus.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    interface IEventHandlerResolver {
        IEnumerable<IEventHandler> FindAll(String folder);
    }
}
