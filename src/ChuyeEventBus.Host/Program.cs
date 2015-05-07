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
        private static MessageChannelServerProxy _pluginHost;

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

            var folderTracker = new FileTracker(_pluginFolder, "*.dll");
            folderTracker.FolderChanged += folderTracker_FolderChanged;
            folderTracker.WatchAsync();

            Console.WriteLine("Press <Enter> to exit");
            Console.ReadLine();

            _logger.Trace("Press <Ctrl + c> to abort, or waiting for task finish");
            _pluginHost.ReleasePluginBatch();

            ProcessSingleton.ReleaseMutex();
        }

        static void BuildServerAndStartAsync() {
            if (!Directory.Exists(_pluginFolder)) {
                throw new Exception("Create your plugin folder and get dll copied");
            }
            //todo: 对根级别dll与plugin级别dll进行差集复制
            var startInfo = new ProcessStartInfo("ROBOCOPY",
                String.Format("\"{0}\" \"{1}\" /mir /NFL /NDL /NJS", _pluginFolder, _tempFolder));
            startInfo.CreateNoWindow = false;
            startInfo.RedirectStandardOutput = false;
            startInfo.UseShellExecute = false;
            Process.Start(startInfo).WaitForExit();

            _pluginHost = new MessageChannelServerProxy();
            _pluginHost.BuildPluginBatchAsync(_tempFolder);
        }

        static void folderTracker_FolderChanged(String file) {
            var trackFolder = Path.GetDirectoryName(file);
            var tempFolder = Path.Combine(_tempFolder, Path.GetFileName(trackFolder));
            if (trackFolder != _pluginFolder) {
                _pluginHost.ReleasePlugin(tempFolder);
                _pluginHost.BuildPluginAsync(tempFolder);
            }
        }
    }
}