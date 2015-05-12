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
        IEnumerable<T> Plugins { get; }
    }

    public abstract class PluginCatalog<T> : MarshalByRefObject, IPluginCatalog<T> {
        private IEnumerable<T> _plugins;

        public String PluginFolder { get; set; }

        public override object InitializeLifetimeService() {
            //return base.InitializeLifetimeService();
            return null;
        }

        public IEnumerable<T> Plugins {
            get {
                if (_plugins == null) {
                    _plugins = OnInitilized();
                }
                return _plugins;
            }
        }

        protected abstract IEnumerable<T> OnInitilized();
    }
}
