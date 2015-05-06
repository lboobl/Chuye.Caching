using ChuyeEventBus.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    class Program {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static String _pluginFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
        private static String _tempFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
        private static MessageChannelServer _messageChannelServer;

        static void Main(string[] args) {
            if (!ProcessSingleton.CreateMutex()) {
                Console.WriteLine("Process is already running, exit");
                return;
            }

            var form = new CommandParser().ParseAsForm(args);
            if (form.AllKeys.Contains("debug", StringComparer.OrdinalIgnoreCase)) {
                Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            }

            BuildServerAndStartAsync();

            var folderTracker = new FolderTracker(_pluginFolder, "*.dll");
            folderTracker.FolderChanged += folderTracker_FolderChanged;
            folderTracker.WatchAsync();

            Console.WriteLine("Press <Enter> to exit");
            Console.ReadLine();

            _logger.Trace("Press <Ctrl + c> to abort, or waiting for task finish");
            _messageChannelServer.Stop();

            ProcessSingleton.ReleaseMutex();
        }
        static void BuildServerAndStartAsync() {
            if (!Directory.Exists(_pluginFolder)) {
                throw new Exception("Create your plugin folder and get dll copied");
            }
            var startInfo = new ProcessStartInfo("ROBOCOPY",
                String.Format("\"{0}\" \"{1}\" /mir /NFL /NDL /NJS", _pluginFolder, _tempFolder));
            startInfo.CreateNoWindow = false;
            startInfo.RedirectStandardOutput = false;
            startInfo.UseShellExecute = false;
            Process.Start(startInfo).WaitForExit();

            _messageChannelServer = (MessageChannelServer)PluginProxy.Singleton.Build(typeof(MessageChannelServer));
            _messageChannelServer.StartAsync(_tempFolder);
        }

        static void folderTracker_FolderChanged() {
            _messageChannelServer.Stop();
            PluginProxy.Singleton.ReleaseHost();
            BuildServerAndStartAsync();
        }
    }
}