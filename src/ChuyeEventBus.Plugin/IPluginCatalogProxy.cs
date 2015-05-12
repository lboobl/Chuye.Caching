using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Plugin {
    public interface IPluginCatalogProxy {
        Type PluginCatalogType { get; set; }
        PluginCatalog Construct(String pluginFolder);
        void Release(String pluginFolder);
        void ReleaseAll();
    }
}
