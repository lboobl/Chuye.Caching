using ChuyeEventBus.Core;
using ChuyeEventBus.Demo;
using NLog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace ChuyeEventBus.Host {
    class Program {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        static HostRunningLog _runningLog;
        static Boolean _logToMongo = false;

        static void Main(string[] args) {
            var form = new CommandParser().ParseAsForm(args);
            _logToMongo = form.AllKeys.Contains("logToMongo", StringComparer.OrdinalIgnoreCase);
            if (_logToMongo) {
                _runningLog = new HostRunningLog();
            }
            if (form.AllKeys.Contains("debug", StringComparer.OrdinalIgnoreCase)) {
                Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            }
            _logger.Trace("MessageChannelServer startup, press <ENTER> to cancel");
            var server = StartServer();

            //MockClient();
            MockClientAsync();

            StopServer(server);
        }

        private static void StopServer(MessageChannelServer server) {
            Console.ReadLine();
            _logger.Trace("MessageChannelServer cancel suspend");
            if (_logToMongo) {
                _runningLog.LogServerStatus(ServerStatus.Suspend);
            }
            server.Stop();

            _logger.Trace("MessageChannelServer wating for stop");
            Thread.Sleep(10000);
            if (_logToMongo) {
                _runningLog.LogServerStatus(ServerStatus.Stop);
            }
            _logger.Trace("MessageChannelServer stoped, press <ENTER> to exit");
            Console.ReadLine();
        }

        static void ShowUsage() {
            _logger.Trace("Usage:");
            _logger.Trace("    -host  ");
            _logger.Trace("    -client");
        }

        static MessageChannelServer StartServer() {
            if (_logToMongo) {
                EventBus.Singleton.ErrorHandler = _runningLog.LogHandlerError;
                _runningLog.LogServerStatus(ServerStatus.Start);
            }
            else {
                EventBus.Singleton.ErrorHandler = (h, err) => _logger.Error(err);
            }

            var server = new MessageChannelServer();
            server.Folder = AppDomain.CurrentDomain.BaseDirectory;
            server.Initialize();
            return server;
        }

        static void MockClient() {
            var works = new[] { 67, 75, 92, 99 };
            _logger.Trace("MessageQueueUtil: Send WorkPublishEvent");
            MessageQueueUtil.Send(new WorkPublishEvent() { WorkId = works[Math.Abs(Guid.NewGuid().GetHashCode()) % works.Length] });
            _logger.Trace("MessageQueueUtil: Send FansFollowEvent");
            MessageQueueUtil.Send(new FansFollowEvent() { FromId = 1, ToId = 2 });
        }

        static void MockClientAsync() {
            var works = new[] { 67, 75, 92, 99 };
            Task.Run(action: () => {
                while (true) {
                    var id = works[Math.Abs(Guid.NewGuid().GetHashCode()) % works.Length];
                    _logger.Trace("MessageQueueUtil: Send WorkPublishEvent");
                    MessageQueueUtil.Send(new WorkPublishEvent() { WorkId = id });
                    Thread.Sleep(Math.Abs(Guid.NewGuid().GetHashCode() % 2000 + 1000));
                }
            });

            Task.Run(action: () => {
                while (true) {
                    var id = works[Math.Abs(Guid.NewGuid().GetHashCode()) % works.Length];
                    _logger.Trace("MessageQueueUtil: Send FansFollowEvent");
                    MessageQueueUtil.Send(new FansFollowEvent() { FromId = 1, ToId = 2 });
                    Thread.Sleep(Math.Abs(Guid.NewGuid().GetHashCode() % 2000 + 1000));
                }
            });
        }
    }
}
