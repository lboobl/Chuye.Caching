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
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args) {
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            _logger.Trace("MessageChannelServer startup, press <ENTER> to cancel");
            var server = StartServer();
            MockClient();
            //MockClientAsync();

            StopServer(server);
        }

        private static void StopServer(MessageChannelServer server) {
            Console.ReadLine();
            _logger.Trace("MessageChannelServer cancel suspend");
            server.Stop();

            _logger.Trace("MessageChannelServer wating for stop");
            Thread.Sleep(10000);

            _logger.Trace("MessageChannelServer stoped, press <ENTER> to exit");
            Console.ReadLine();
        }

        static void ShowUsage() {
            _logger.Trace("Usage:");
            _logger.Trace("    -host  ");
            _logger.Trace("    -client");
        }

        static MessageChannelServer StartServer() {
            EventBus.Singleton.ErrorHandler = _logger.Error;

            var server = new MessageChannelServer();
            server.Folder = AppDomain.CurrentDomain.BaseDirectory;
            server.Initialize();

            return server;
        }

        static void MockClient() {
            var works = new[] { 67, 75, 92, 99 };
            MessageQueueUtil.Send(new WorkPublishEvent() { WorkId = works[Math.Abs(Guid.NewGuid().GetHashCode()) % works.Length] });
            MessageQueueUtil.Send(new FansFollowEvent() { FromId = 1, ToId = 2 });
        }

        static void MockClientAsync() {
            var works = new[] { 67, 75, 92, 99 };
            Task.Run(action: () => {
                while (true) {
                    var id = works[Math.Abs(Guid.NewGuid().GetHashCode()) % works.Length];
                    MessageQueueUtil.Send(new WorkPublishEvent() { WorkId = id });
                    Thread.Sleep(Math.Abs(Guid.NewGuid().GetHashCode() % 2000 + 1000));
                }
            });

            Task.Run(action: () => {
                while (true) {
                    var id = works[Math.Abs(Guid.NewGuid().GetHashCode()) % works.Length];
                    MessageQueueUtil.Send(new FansFollowEvent() { FromId = 1, ToId = 2 });
                    Thread.Sleep(Math.Abs(Guid.NewGuid().GetHashCode() % 2000 + 1000));
                }
            });
        }
    }
}
