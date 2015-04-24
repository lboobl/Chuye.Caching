using ChuyeEventBus.Core;
using ChuyeEventBus.Demo;
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
            var form = new CommandParser().ParseAsForm(args);
            if (form.AllKeys.Contains("debug", StringComparer.OrdinalIgnoreCase)) {
                Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            }

            var server = StartServer();
            _logger.Trace("Press <ctrl + c> to abort, <Enter> to stop");

            MockClient();
            //MockClientAsync();

            Console.ReadLine();
            server.Stop();
            _logger.Trace("Press <Ctrl + c> to abort, or waiting for task finish");
            Console.ReadLine();
        }

        static MessageChannelServer StartServer() {
            var server = new MessageChannelServer();
            var finder = new EventHandlerFinder();
            finder.Folder = AppDomain.CurrentDomain.BaseDirectory;
            server.EventHandlerFinder = finder;
            server.StartAsync();
            return server;
        }

        static void MockClient() {
            //for (int i = 0; i < 6; i++) {
            //    MessageQueueUtil.Send(new FansFollowEvent() { FromId = 1, ToId = i + 1 });
            //}
            Task.Run(action: () => {
                for (int i = 0; i < 4; i++) {
                    MessageQueueUtil.Send(new WorkPublishEvent() { WorkId = 60 + i * 10 });
                    Thread.Sleep(1000);
                }
            });
        }

        static void MockClientAsync() {
            var random = new Random();
            var works = Enumerable.Range(0, 100).Select(r => random.Next(1, 100)).ToArray();
            Task.Run(action: () => {
                for (int i = 0; i < 100; i++) {
                    MessageQueueUtil.Send(new WorkPublishEvent() { WorkId = i });
                    Thread.Sleep(Math.Abs(Guid.NewGuid().GetHashCode()) % 200 + 300);
                }
            });

            //Task.Run(action: () => {
            //    while (true) {
            //        var id = works[Math.Abs(Guid.NewGuid().GetHashCode()) % works.Length];
            //        MessageQueueUtil.Send(new FansFollowEvent() { FromId = 1, ToId = 2 });
            //        Thread.Sleep(Math.Abs(Guid.NewGuid().GetHashCode() % 1000 + 1000));
            //    }
            //});
        }
    }
}
