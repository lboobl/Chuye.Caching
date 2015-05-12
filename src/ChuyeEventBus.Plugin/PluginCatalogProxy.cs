using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Plugin {
    public class PluginCatalogProxy : IPluginCatalogProxy, IDisposable {
        private readonly Dictionary<String, AppDomain> _pluginDomains
            = new Dictionary<String, AppDomain>();
        private readonly Dictionary<String, PluginCatalog> _pluginCatalogs
            = new Dictionary<String, PluginCatalog>();
        private Type _pluginCatalogType = typeof(PluginCatalog);


        public virtual Type PluginCatalogType {
            get {
                return _pluginCatalogType;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException("PluginCatalogType");
                }
                if (!typeof(PluginCatalog).IsAssignableFrom(value) || !value.IsSubclassOf(typeof(MarshalByRefObject))) {
                    throw new ArgumentOutOfRangeException("PluginCatalogType");
                }
                _pluginCatalogType = value;
            }
        }

        public PluginCatalog Construct(String pluginFolder) {
            PluginCatalog pluginCatalog;
            if (!_pluginCatalogs.TryGetValue(pluginFolder, out pluginCatalog)) {
                var pluginDomain = CreatePluginDomain(pluginFolder);
                pluginCatalog = (PluginCatalog)pluginDomain.CreateInstanceAndUnwrap(
                    PluginCatalogType.Assembly.FullName,
                    PluginCatalogType.FullName);
                pluginCatalog.PluginFolder = pluginFolder;
                _pluginCatalogs.Add(pluginFolder, pluginCatalog);
            }
            return pluginCatalog;
        }

        protected virtual AppDomain CreatePluginDomain(String pluginFolder) {
            var cfg = GetPluginConfiguration(pluginFolder);
            var bins = new[] { pluginFolder.Substring(AppDomain.CurrentDomain.BaseDirectory.Length) };
            var setup = new AppDomainSetup();
            if (File.Exists(cfg)) {
                setup.ConfigurationFile = cfg;
            }
            setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
            setup.PrivateBinPath = String.Join(";", bins);

            AppDomain pluginDoamin;
            if (!_pluginDomains.TryGetValue(pluginFolder, out pluginDoamin)) {
                pluginDoamin = AppDomain.CreateDomain(pluginFolder, null, setup);
                _pluginDomains.Add(pluginFolder, pluginDoamin);
            }
            return pluginDoamin;
        }

        protected virtual String GetPluginConfiguration(String pluginFolder) {
            var config = Path.Combine(pluginFolder, "main.config");
            if (!File.Exists(config)) {
                config = Path.Combine(pluginFolder, Path.GetFileName(pluginFolder) + ".dll.config");
            }
            if (!File.Exists(config)) {
                var configs = Directory.GetFiles(pluginFolder, "*.dll.config", SearchOption.TopDirectoryOnly);
                Debug.WriteLine(String.Format("Unknown configuration as too many .dll.config files in \"{0}\"", pluginFolder));
                if (config.Length == 1) {
                    config = configs[0];
                }
            }
            return config;
        }

        public void Release(String pluginFolder) {
            AppDomain pluginDoamin;
            if (_pluginDomains.TryGetValue(pluginFolder, out pluginDoamin)) {
                AppDomain.Unload(pluginDoamin);
                _pluginCatalogs.Remove(pluginFolder);
                _pluginDomains.Remove(pluginFolder);
            }
        }

        public void ReleaseAll() {
            var unloadTasks = _pluginDomains.Select(async p =>
                await Task.Run(action: () => AppDomain.Unload(p.Value))).ToArray();
            Task.WaitAll(unloadTasks);
            _pluginCatalogs.Clear();
            _pluginDomains.Clear();
        }

        public void Dispose() {
            ReleaseAll();
        }
    }
}
