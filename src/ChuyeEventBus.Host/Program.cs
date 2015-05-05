using ChuyeEventBus.Core;
using NLog;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    class Program {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args) {
            if (!ProcessSingleton.CreateMutex()) {
                Console.WriteLine("Process is already running, exit");
                return;
            }

            var form = new CommandParser().ParseAsForm(args);
            if (form.AllKeys.Contains("debug", StringComparer.OrdinalIgnoreCase)) {
                Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            }

            var server = StartServer();
            _logger.Trace("Press <ctrl + c> to abort, <Enter> to stop");

            //ChuyeEventBus.Demo.EventMocker.MockClient();
            //ChuyeEventBus.Demo.EventMocker.MockClientAsync();

            Console.ReadLine();
            server.Stop();
            _logger.Trace("Press <Ctrl + c> to abort, or waiting for task finish");
            Console.ReadLine();

            ProcessSingleton.ReleaseMutex();
        }

        static MessageChannelServer StartServer() {
            var server = new MessageChannelServer();
            var finder = new EventHandlerFinder();
            finder.Folder = AppDomain.CurrentDomain.BaseDirectory;
            server.EventHandlerFinder = finder;
            server.StartAsync();
            return server;
        }
    }
}
