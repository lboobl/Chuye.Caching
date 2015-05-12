using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Plugin {
    public class MefPluginResolver : IPluginResolver {
        public IEnumerable<T> FindAll<T>(String pluginFolder) {
            var catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog(pluginFolder));
            var container = new CompositionContainer(catalog);
            return container.GetExportedValues<T>();
        }
    }
}
