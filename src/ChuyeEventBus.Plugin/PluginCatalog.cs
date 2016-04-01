using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Plugin {
    public class PluginCatalog<T> : MarshalByRefObject, IPluginCatalog<T> {
        public String PluginFolder { get; set; }

        public override object InitializeLifetimeService() {
            return null;
        }

        public virtual IEnumerable<T> FindPlugins() {
            var resolver = new MefPluginResolver();
            //var resolver = new ReflectionPluginResolver();
            return resolver.FindAll<T>(PluginFolder);
        }
    }
}
