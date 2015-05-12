using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Plugin {
    [InheritedExport]
    public interface IPlugin {
    }

    public class PluginCatalog : MarshalByRefObject {
        private IEnumerable<Object> _plugins;

        public String PluginFolder { get; set; }

        public IEnumerable<Object> Plugins {
            get {
                if (_plugins == null) {
                    _plugins = OnInitilized();
                }
                return _plugins;
            }
        }

        protected virtual IEnumerable<Object> OnInitilized() {
            var resolver = new ReflectionPluginResolver();
            return resolver.FindAll<IPlugin>(PluginFolder);
        }
    }
}
