using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Plugin {

    public interface IPluginCatalog {
        String PluginFolder { get; set; }
    }

    public interface IPluginCatalog<out T> : IPluginCatalog {
        IEnumerable<T> FindPlugins();
    }

    public abstract class PluginCatalog<T> : MarshalByRefObject, IPluginCatalog<T> {
        public String PluginFolder { get; set; }

        public override object InitializeLifetimeService() {
            //return base.InitializeLifetimeService();
            return null;
        }

        public virtual IEnumerable<T> FindPlugins() {
            var resolver = new ReflectionPluginResolver();
            return resolver.FindAll<T>(PluginFolder);
        }
    }
}
