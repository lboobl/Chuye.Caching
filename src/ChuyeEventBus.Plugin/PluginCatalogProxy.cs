using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Plugin {
    public class PluginCatalogProxy : IPluginCatalogProxy, IDisposable {
        private readonly ConcurrentDictionary<String, AppDomain> _pluginDomains
            = new ConcurrentDictionary<String, AppDomain>();

        public T Construct<T, P>(String pluginFolder) where T : IPluginCatalog<P>, new() {
            var pluginCatalogType = typeof(T);
            //todo: 从同一目录获取不同的 IPluginCatalog<P> 实例如何处理
            var pluginDomain = CreatePluginDomain(pluginFolder);
            var pluginCatalog = (IPluginCatalog)pluginDomain.CreateInstanceAndUnwrap(
                  pluginCatalogType.Assembly.FullName,
                  pluginCatalogType.FullName);
            pluginCatalog.PluginFolder = pluginFolder;
            return (T)pluginCatalog;
        }

        protected virtual AppDomain CreatePluginDomain(String pluginFolder) {
            return _pluginDomains.GetOrAdd(pluginFolder, pf => {
                var cfg = GetPluginConfiguration(pluginFolder);
                var bins = new[] { pluginFolder.Substring(AppDomain.CurrentDomain.BaseDirectory.Length) };
                var setup = new AppDomainSetup();
                if (File.Exists(cfg)) {
                    setup.ConfigurationFile = cfg;
                }
                setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
                setup.PrivateBinPath = String.Join(";", bins);
                return AppDoaminHelper.CreateAppDomain(pf, setup);
            });

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
            var poped = _pluginDomains.TryRemove(pluginFolder, out pluginDoamin);
            if (poped) {
                AppDoaminHelper.UnloadAppDomain(pluginDoamin);
            }
        }

        public void ReleaseAll() {
            var unloadTasks = _pluginDomains.Select(async p =>
                await Task.Run(action: () => Release(p.Key))).ToArray();
            Task.WaitAll(unloadTasks);
            _pluginDomains.Clear();
        }

        public void Dispose() {
            ReleaseAll();
        }
    }
}
