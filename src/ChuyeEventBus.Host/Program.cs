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
        private static HostRunningService _runningLog;
        private static Boolean _useMongo = false;

        static void Main(string[] args) {
            var form = new CommandParser().ParseAsForm(args);
            _useMongo = form.AllKeys.Contains("log", StringComparer.OrdinalIgnoreCase);
            if (_useMongo) {
                _runningLog = new HostRunningService();
            }

            LogUtil.Trace = _logger.Trace;
            LogUtil.Debug = _logger.Debug;
            LogUtil.Warn = _logger.Warn;
            _logger.Info("MessageChannelServer startup, press <ENTER> to cancel");
            var server = StartServer();

            //MockClient();
            //MockClientAsync();
            StopServer(server);
        }

        static void StopServer(MessageChannelServer server) {
            Console.ReadLine();
            _logger.Info("MessageChannelServer cancel suspend");
            if (_useMongo) {
                _runningLog.LogServerStatus(ServerStatus.Suspend);
            }
            server.Stop();

            _logger.Info("MessageChannelServer wating for stop");
            Thread.Sleep(10000);
            if (_useMongo) {
                _runningLog.LogServerStatus(ServerStatus.Stop);
            }
            _logger.Info("MessageChannelServer stoped, press <ENTER> to exit");
            Console.ReadLine();
        }

        static MessageChannelServer StartServer() {
            if (_useMongo) {
                EventBus.Singleton.ErrorHandler = (h, err, events) => _runningLog.LogError(h, err, events);
            }
            else {
                EventBus.Singleton.ErrorHandler = (h, err, events) => _logger.Error(err);
            };

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
