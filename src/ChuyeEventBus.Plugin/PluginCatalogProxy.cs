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

        public T Construct<T, P>(String pluginFolder) where T : IPluginCatalog<P> {
            var pluginCatalogType = typeof(T);
            //todo: 从同一目录获取不同的 IPluginCatalog<P> 实例如何处理
            //var pluginKey = String.Concat(Path.GetFileName(pluginFolder), "_", pluginCatalogType.FullName);
            var pluginDomain = CreatePluginDomain(pluginFolder);
            var pluginCatalog = (IPluginCatalog)pluginDomain.CreateInstanceAndUnwrap(
                  pluginCatalogType.Assembly.FullName,
                  pluginCatalogType.FullName);
            pluginCatalog.PluginFolder = pluginFolder;
            return (T)pluginCatalog;
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

                if (config.Length > 1) {
                    Debug.WriteLine(String.Format("Unknown configuration as too many .dll.config files in \"{0}\""
                        , Path.GetFileName(pluginFolder)));
                }
                else if (config.Length == 1) {
                    config = configs[0];
                }
            }
            return config;
        }

        public void Release(String pluginFolder) {
            AppDomain pluginDoamin;
            if (_pluginDomains.TryGetValue(pluginFolder, out pluginDoamin)) {
                AppDomain.Unload(pluginDoamin);
                _pluginDomains.Remove(pluginFolder);
            }
        }

        public void ReleaseAll() {
            var unloadTasks = _pluginDomains.Select(async p =>
                await Task.Run(action: () => AppDomain.Unload(p.Value))).ToArray();
            Task.WaitAll(unloadTasks);
            _pluginDomains.Clear();
        }

        public void Dispose() {
            ReleaseAll();
        }
    }
}
