using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Plugin {
    public interface IPluginResolver {
        IEnumerable<T> FindAll<T>(String pluginFolder);
    }

}
