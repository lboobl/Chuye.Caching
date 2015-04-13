using Chuye.Persistent;
using Chuye.Persistent.Mongo;
using ChuyeEventBus.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    internal class HostRunningService {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly MongoRepositoryContext _context;

        public HostRunningService() {
            var conStr = ConfigurationManager.ConnectionStrings["Chuye"].ConnectionString;
            var dbName = "ChuyeEventBus";
            _context = new MongoRepositoryContext(conStr, dbName);
        }

        public Int32 LogServerStatus(ServerStatus status) {
            var runningHistoryRepo = new MongoRepository<RunningHistory>(_context);
            var runningHistoryEntry = new RunningHistory {
                Status = status,
                CreateAt = DateTime.UtcNow,
            };
            if (status != ServerStatus.Start) {
                var startRuningHistoryEntry = runningHistoryRepo.All
                    .OrderByDescending(r => r.Id)
                    .FirstOrDefault();
                runningHistoryEntry.ParentId = startRuningHistoryEntry != null
                    ? startRuningHistoryEntry.ParentId : -1;
                runningHistoryRepo.Create(runningHistoryEntry);
            }
            else {
                runningHistoryRepo.Create(runningHistoryEntry);
                runningHistoryEntry.ParentId = runningHistoryEntry.Id;
                runningHistoryRepo.Update(runningHistoryEntry);
            }
            return runningHistoryEntry.Id;
        }

        public void LogError(IEventHandler handler, Exception error, IList<IEvent> events) {
            var runningHistoryRepo = new MongoRepository<RunningHistory>(_context);
            var runningHistoryEntry = runningHistoryRepo.All
                   .OrderByDescending(r => r.Id)
                   .FirstOrDefault();

            var lastRunningHistoryId = runningHistoryEntry != null ? runningHistoryEntry.ParentId : 0;
            var errorSummaryRepo = new MongoRepository<ErrorSummary>(_context);
            var handlerType = handler.GetType().FullName;
            var errorSummaryEntry = errorSummaryRepo.All
                .FirstOrDefault(r => r.HistoryId == lastRunningHistoryId && r.Source == handlerType);

            if (errorSummaryEntry == null) {
                errorSummaryEntry = new ErrorSummary {
                    Number = 0,
                    Source = handlerType,
                    HistoryId = lastRunningHistoryId,
                };
                errorSummaryRepo.Create(errorSummaryEntry);
            }
            else {
                errorSummaryEntry.Number++;
                errorSummaryRepo.Update(errorSummaryEntry);
            }
            var errorDetailRepo = new MongoRepository<ErrorDetail>(_context);
            var errorDetailEntry = new ErrorDetail {
                Source = handlerType,
                Error = new {
                    error.Message,
                    error.StackTrace,
                },
                Event = events,
                CreateAt = DateTime.UtcNow,
            };
            errorDetailRepo.Create(errorDetailEntry);
        }
    }

    public class RunningHistory : IAggregate {
        public Int32 Id { get; set; }
        public ServerStatus Status { get; set; }
        public Int32 ParentId { get; set; }
        public DateTime CreateAt { get; set; }
    }

    public enum ServerStatus {
        Start, Suspend, Stop
    }

    public class ErrorSummary : IAggregate {
        public Int32 Id { get; set; }
        public String Source { get; set; }
        public Int32 Number { get; set; }
        public Int32 HistoryId { get; set; }
    }

    public class ErrorDetail : IAggregate {
        public Int32 Id { get; set; }
        public String Source { get; set; }
        public IList<IEvent> Event { get; set; }
        public Object Error { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
