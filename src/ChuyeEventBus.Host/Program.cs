using ChuyeEventBus.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    class Program {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static String _pluginFolder;
        private static String _tempFolder;
        private static readonly MessageChannelServerProxy _pluginHost = new MessageChannelServerProxy();

        static void Main(String[] args) {
            if (!ProcessSingleton.CreateMutex()) {
                Console.WriteLine("Process is already running, exit");
                return;
            }

            var form = new CommandParser().ParseAsForm(args);
            if (form.AllKeys.Contains("debug", StringComparer.OrdinalIgnoreCase)) {
                Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            }

            PrepareFolders(form);
            _pluginHost.BuildPluginBatchAsync(_tempFolder);

            var folderTracker = new FileTracker(_pluginFolder, "*.dll");
            folderTracker.FileChanged += fileTracker_FileChanged;
            folderTracker.WatchAsync();

            Console.WriteLine("Press <Enter> to exit");
            Console.ReadLine();

            _logger.Trace("Press <Ctrl + c> to abort, or waiting for task finish");
            _pluginHost.ReleasePluginBatch();

            ProcessSingleton.ReleaseMutex();
        }

        static void PrepareFolders(NameValueCollection args) {
            _pluginFolder = ConfigurationManager.AppSettings.Get("plugins");
            if (String.IsNullOrWhiteSpace(_pluginFolder)) {
                _pluginFolder = args.Get("plugins");
            }
            if (String.IsNullOrWhiteSpace(_pluginFolder) || !Directory.Exists(_pluginFolder)) {
                throw new Exception("Create your plugin folder, config it and get dll copied first");
            }
            if (!Path.IsPathRooted(_pluginFolder)) {
                _pluginFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _pluginFolder);
            }

            _tempFolder = ConfigurationManager.AppSettings.Get("temp");
            if (String.IsNullOrWhiteSpace(_tempFolder)) {
                _tempFolder = args.Get("temp");
            }
            if (String.IsNullOrWhiteSpace(_pluginFolder)) {
                _tempFolder = "temp";
            }
            if (!Path.IsPathRooted(_tempFolder)) {
                _tempFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _tempFolder);
            }
            RoboCopy.Mir(_pluginFolder, _tempFolder);
        }

        static void fileTracker_FileChanged(String file) {
            var pluginFolder = Path.GetDirectoryName(file);
            var tempFolder = Path.Combine(_tempFolder, Path.GetFileName(pluginFolder));
            if (pluginFolder != _pluginFolder) {

                RoboCopy.Mir(pluginFolder, tempFolder);
                _pluginHost.ReleasePlugin(tempFolder);
                _pluginHost.BuildPluginAsync(tempFolder);
            }
        }
    }
}