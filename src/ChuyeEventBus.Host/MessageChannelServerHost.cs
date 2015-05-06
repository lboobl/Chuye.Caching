using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    internal class MessageChannelServerHost {
        private readonly List<AppDomain> _pluginDomains = new List<AppDomain>();
        private readonly List<MessageChannelServer> _messageChannelServers = new List<MessageChannelServer>();

        public void SearchAsync(String folder) {
            var pluginFolders = new DirectoryInfo(folder).EnumerateDirectories("*", SearchOption.TopDirectoryOnly);
            var messageChannelServerType = typeof(MessageChannelServer);

            foreach (var pluginFolder in pluginFolders) {
                //get .config file
                var config = Path.Combine(pluginFolder.FullName, "main.config");
                if (!File.Exists(config)) {
                    config = Path.Combine(pluginFolder.FullName, pluginFolder.Name + ".dll.config");
                }
                if (!File.Exists(config)) {
                    var configs = pluginFolder.GetFiles("*.config", SearchOption.TopDirectoryOnly);
                    if (configs.Length > 1) {
                        throw new Exception("Unknown config file for " + pluginFolder.Name);
                    }
                    if (config.Length == 1) {
                        config = configs[0].FullName;
                    }
                }

                var bins = new[] { pluginFolder.FullName.Substring(AppDomain.CurrentDomain.BaseDirectory.Length) };
                var setup = new AppDomainSetup();
                if (File.Exists(config)) {
                    setup.ConfigurationFile = config;
                }
                setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
                setup.PrivateBinPath = String.Join(";", bins);

                //注意 bindingRedirect 中指向的dll版本和实际使用的dll版本
                var pluginDomain = AppDomain.CreateDomain(pluginFolder.Name, AppDomain.CurrentDomain.Evidence, setup);
                var messageChannelServer = (MessageChannelServer)pluginDomain.CreateInstanceAndUnwrap(
                    messageChannelServerType.Assembly.FullName, messageChannelServerType.FullName);
                messageChannelServer.StartAsync(pluginFolder.FullName);

                _messageChannelServers.Add(messageChannelServer);
                _pluginDomains.Add(pluginDomain);
            }
        }

        public void Stop() {
            _messageChannelServers.ForEach(s => s.Stop());
            _messageChannelServers.Clear();
            _pluginDomains.ForEach(d => AppDomain.Unload(d));
            _pluginDomains.Clear();
        }
    }
}
