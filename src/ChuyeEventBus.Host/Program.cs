using ChuyeEventBus.Core;
using NLog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ChuyeEventBus.Demo;

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

            //MockClient();
            //MockClientAsync();

            Console.ReadLine();
            server.Stop();
            _logger.Trace("Press <Ctrl + c> to abort, or waiting for task finish");
            Console.ReadLine();
        }

        static MessageChannelServer StartServer() {
            EventBus.Singleton.ErrorHandler = (h, err, events) => _logger.Error(err);

            var server = new MessageChannelServer();
            server.Folder = AppDomain.CurrentDomain.BaseDirectory;
            server.Initialize();
            return server;
        }

        static void MockClient() {
            MessageQueueUtil.Send(new FansFollowEvent() { FromId = 1, ToId = 2 });
            var works = new[] { 67, 75, 92, 99 };
            MessageQueueUtil.Send(new WorkPublishEvent() { WorkId = works[Math.Abs(Guid.NewGuid().GetHashCode()) % works.Length] });
            MessageQueueUtil.Send(new WorkPublishEvent() { WorkId = works[Math.Abs(Guid.NewGuid().GetHashCode()) % works.Length] });
        }

        static void MockClientAsync() {
            var works = new[] { 67, 75, 92, 99 };
            Task.Run(action: () => {
                while (true) {
                    var id = works[Math.Abs(Guid.NewGuid().GetHashCode()) % works.Length];
                    MessageQueueUtil.Send(new WorkPublishEvent() { WorkId = id });
                    Thread.Sleep(Math.Abs(Guid.NewGuid().GetHashCode() % 1000 + 1000));
                }
            });

            Task.Run(action: () => {
                while (true) {
                    var id = works[Math.Abs(Guid.NewGuid().GetHashCode()) % works.Length];
                    MessageQueueUtil.Send(new FansFollowEvent() { FromId = 1, ToId = 2 });
                    Thread.Sleep(Math.Abs(Guid.NewGuid().GetHashCode() % 1000 + 1000));
                }
            });
        }
    }
}
