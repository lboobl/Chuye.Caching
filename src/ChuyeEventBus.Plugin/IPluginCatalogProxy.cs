using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Plugin {
    public interface IPluginCatalogProxy {
        T Construct<T>(String pluginFolder) where T : IPluginCatalog;
        void Release(String pluginFolder);
        void ReleaseAll();
    }
}
