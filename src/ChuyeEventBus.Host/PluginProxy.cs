using ChuyeEventBus.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    public class PluginProxy {
        private static readonly PluginProxy _singleton = new PluginProxy();
        private AppDomain _pluginDomain;

        public static PluginProxy Singleton {
            get { return _singleton; }
        }

        private PluginProxy() {
        }

        public Object Build(Type targetType) {
            if (_pluginDomain == null) {
                _pluginDomain = AppDomain.CreateDomain("PluginDomain");
            }

            return _pluginDomain.CreateInstanceAndUnwrap(
                targetType.Assembly.FullName,
                targetType.FullName);
        }

        public void ReleaseHost() {
            if (_pluginDomain != null) {
                AppDomain.Unload(_pluginDomain);
                _pluginDomain = null;
            }
        }
    }
}
