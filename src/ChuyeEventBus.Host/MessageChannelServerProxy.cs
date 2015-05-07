using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    internal class MessageChannelServerProxy {
        private readonly Dictionary<String, AppDomain> _pluginDomains
            = new Dictionary<String, AppDomain>();
        private readonly Dictionary<String, MessageChannelServer> _messageChannelServers
            = new Dictionary<String, MessageChannelServer>();

        public void BuildPluginBatchAsync(String folder) {
            var pluginPaths = Directory.EnumerateDirectories(folder, "*", SearchOption.TopDirectoryOnly);
            foreach (var pluginPath in pluginPaths) {
                BuildPluginAsync(pluginPath);
            }
        }

        public void BuildPluginAsync(String pluginPath) {
            var messageChannelServerType = typeof(MessageChannelServer);
            var config = GetConfigurationFile(pluginPath);

            var bins = new[] { pluginPath.Substring(AppDomain.CurrentDomain.BaseDirectory.Length) };
            var setup = new AppDomainSetup();
            if (File.Exists(config)) {
                setup.ConfigurationFile = config;
            }
            setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
            setup.PrivateBinPath = String.Join(";", bins);

            //注意 bindingRedirect 中指向的dll版本和实际使用的dll版本
            var pluginDomain = AppDomain.CreateDomain(Path.GetFileName(pluginPath), AppDomain.CurrentDomain.Evidence, setup);
            var messageChannelServer = (MessageChannelServer)pluginDomain.CreateInstanceAndUnwrap(
                  messageChannelServerType.Assembly.FullName, messageChannelServerType.FullName);
            messageChannelServer.StartAsync(pluginPath);

            _messageChannelServers.Add(pluginPath, messageChannelServer);
            _pluginDomains.Add(pluginPath, pluginDomain);
        }

        public String GetConfigurationFile(String pluginPath) {
            var config = Path.Combine(pluginPath, "main.config");
            if (!File.Exists(config)) {
                config = Path.Combine(pluginPath, Path.GetFileName(pluginPath) + ".dll.config");
            }
            if (!File.Exists(config)) {
                var configs = Directory.GetFiles(pluginPath, "*.config", SearchOption.TopDirectoryOnly);
                if (configs.Length > 1) {
                    throw new Exception("Unknown config file for " + Path.GetFileName(pluginPath));
                }
                if (config.Length == 1) {
                    config = configs[0];
                }
            }
            return config;
        }

        public void ReleasePlugin(String pluginPath) {
            MessageChannelServer messageChannelServer;
            if (_messageChannelServers.TryGetValue(pluginPath, out messageChannelServer)) {
                messageChannelServer.Stop();
                _messageChannelServers.Remove(pluginPath);
            }
            AppDomain pluginDomain;
            if (_pluginDomains.TryGetValue(pluginPath, out pluginDomain)) {
                AppDomain.Unload(pluginDomain);
                _pluginDomains.Remove(pluginPath);
            }
        }

        public void ReleasePluginBatch() {
            var stopTasks = _messageChannelServers
                .Select(async p => await Task.Run(action: p.Value.Stop))
                .ToArray();
            Task.WaitAll(stopTasks);
            _messageChannelServers.Clear();
            foreach (var ad in _pluginDomains) {
                AppDomain.Unload(ad.Value);
            }
            _pluginDomains.Clear();
        }
    }
}
