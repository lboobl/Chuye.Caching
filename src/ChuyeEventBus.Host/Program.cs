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
            //Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            StartServer();
            //MockClient();
            MockClientAsync();

            _logger.Trace("Press <ENTER> to exit");
            Console.ReadLine();
        }

        static void ShowUsage() {
            _logger.Trace("Usage:");
            _logger.Trace("    -host  ");
            _logger.Trace("    -client");
        }

        static void StartServer() {
            EventBus.Singleton.ErrorHandler = _logger.Error;

            var server = new MessageChannelServer();
            server.Folder = AppDomain.CurrentDomain.BaseDirectory;
            server.Initialize();
        }

        static void MockClient() {
            var works = new[] { 67, 75, 92, 99 };
            var id = works[Math.Abs(Guid.NewGuid().GetHashCode()) % works.Length];
            _logger.Trace("WorkPublishEvent 入队, id {0}", id);
            MessageQueueUtil.Send(new WorkPublishEvent() {
                WorkId = id
            });
            id = works[Math.Abs(Guid.NewGuid().GetHashCode()) % works.Length];
            _logger.Trace("WorkUpdateEvent  入队, id {0}", id);
            MessageQueueUtil.Send(new WorkUpdateEvent() {
                WorkId = id,
                UpdateType = WorkUpdateType.Access
            });
            id = works[Math.Abs(Guid.NewGuid().GetHashCode()) % works.Length];
            _logger.Trace("[{0:D2}] WorkUpdateEvent  入队, id {1}",
                Thread.CurrentThread.ManagedThreadId, id); 
            MessageQueueUtil.Send(new WorkUpdateEvent() {
                WorkId = id,
                UpdateType = WorkUpdateType.Share
            });
        }

        static void MockClientAsync() {
            var works = new[] { 67, 75, 92, 99 };
            Task.Run(action: () => {
                while (true) {
                    var id = works[Math.Abs(Guid.NewGuid().GetHashCode()) % works.Length];
                    _logger.Trace("[{0:D2}] WorkPublishEvent 入队, id {1}", 
                        Thread.CurrentThread.ManagedThreadId, id);
                    MessageQueueUtil.Send(new WorkPublishEvent() { WorkId = id });
                    Thread.Sleep(Math.Abs(Guid.NewGuid().GetHashCode() % 3000 + 2000));
                }
            });

            Task.Run(action: () => {
                while (true) {
                    var id = works[Math.Abs(Guid.NewGuid().GetHashCode()) % works.Length];
                    _logger.Trace("[{0:D2}] WorkUpdateEvent  入队, id {1}",
                        Thread.CurrentThread.ManagedThreadId, id);
                    MessageQueueUtil.Send(new WorkUpdateEvent() { WorkId = id, UpdateType = WorkUpdateType.Access });
                    Thread.Sleep(Math.Abs(Guid.NewGuid().GetHashCode() % 3000 + 2000));
                }
            });

            Task.Run(action: () => {
                while (true) {
                    var id = works[Math.Abs(Guid.NewGuid().GetHashCode()) % works.Length];
                    _logger.Trace("[{0:D2}] WorkUpdateEvent  入队, id {1}",
                        Thread.CurrentThread.ManagedThreadId, id);
                    MessageQueueUtil.Send(new WorkUpdateEvent() { WorkId = id, UpdateType = WorkUpdateType.Share });
                    Thread.Sleep(Math.Abs(Guid.NewGuid().GetHashCode() % 3000 + 2000));
                }
            });
        }
    }
}
