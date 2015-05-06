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
        private static String pluginFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
        private static String tempFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
        private static MessageChannelServer _server = new MessageChannelServer();

        static void Main(string[] args) {
            if (!ProcessSingleton.CreateMutex()) {
                Console.WriteLine("Process is already running, exit");
                return;
            }

            var form = new CommandParser().ParseAsForm(args);
            if (form.AllKeys.Contains("debug", StringComparer.OrdinalIgnoreCase)) {
                Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            }

            StartServer();
            _logger.Trace("Press <ctrl + c> to abort, <Enter> to stop");

            //ChuyeEventBus.Demo.EventMocker.MockClient();
            //ChuyeEventBus.Demo.EventMocker.MockClientAsync();

            Console.ReadLine();
            _server.Stop();
            _logger.Trace("Press <Ctrl + c> to abort, or waiting for task finish");
            Console.ReadLine();

            ProcessSingleton.ReleaseMutex();
        }

        static void StartServer() {
            var folderTracker = new FolderTracker(pluginFolder, "*.dll");
            folderTracker.FolderChanged += folderTracker_FolderChanged;
            folderTracker.WatchAsync();

            var eventHandlers = BuildEventHandlers();
            _server.StartAsync(eventHandlers);
        }

        private static IEnumerable<IEventHandler> BuildEventHandlers() {
            var tempFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
            //Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(folder1, folder2);
            var startInfo = new ProcessStartInfo("ROBOCOPY", String.Format("\"{0}\" \"{1}\" /mir", pluginFolder, tempFolder));
            startInfo.CreateNoWindow = false;
            startInfo.RedirectStandardOutput = false;
            startInfo.UseShellExecute = false;
            Process.Start(startInfo).WaitForExit();

            var finder = new EventHandlerFinder();
            finder.Folder = tempFolder;
            var eventHandlers = finder.GetEventHandlers();
            return eventHandlers;
        }


        static void folderTracker_FolderChanged() {
            _server.Stop();

            _server = new MessageChannelServer();
            var eventHandlers = BuildEventHandlers();
            _server.StartAsync(eventHandlers);
        }
    }
}
