using ChuyeEventBus.Core;
using ChuyeEventBus.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    internal class MessageChannelServerHost : IDisposable {
        private readonly PluginCatalogProxy _pluginCatalogProxy = new PluginCatalogProxy();
        private readonly Dictionary<String, MessageChannelServer> _messageChannelServers
            = new Dictionary<String, MessageChannelServer>();

        public void BuildAsync(String pluginPath) {
            var messageChannelServer = _pluginCatalogProxy.Construct<MessageChannelServer>(pluginPath);
            _messageChannelServers.Add(pluginPath, messageChannelServer);
            messageChannelServer.StartAsync();
        }

        public void BuildAllAsync(String pluginFolder) {
            var pluginPaths = Directory.EnumerateDirectories(pluginFolder, "*", SearchOption.TopDirectoryOnly);
            foreach (var pluginPath in pluginPaths) {
                BuildAsync(pluginPath);
            }
        }

        public void Stop(String pluginPath) {
            MessageChannelServer messageChannelServer;
            if (_messageChannelServers.TryGetValue(pluginPath, out messageChannelServer)) {
                messageChannelServer.StopChannels();
                _messageChannelServers.Remove(pluginPath);
            }
            _pluginCatalogProxy.Release(pluginPath);
        }

        public void StopAll() {
            try {
                var stopTasks = _messageChannelServers
                    .Select(async p => await Task.Run(action: p.Value.StopChannels))
                    .ToArray();
                Task.WaitAll(stopTasks);
            }
            finally {
                _messageChannelServers.Clear();
                _pluginCatalogProxy.ReleaseAll();
            }
        }

        public void Dispose() {
            StopAll();
        }
    }
}
