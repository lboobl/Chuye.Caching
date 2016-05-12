using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Caching {
    public interface IDistributedLock {
        IDisposable ReleasableLock(String key, Int32 milliseconds = DistributedLockTime.DisposeMillisecond);
        void Lock(String key, Int32 milliseconds);
        Boolean TryLock(String key, Int32 milliseconds);
        void UnLock(String key);
    }

    public class DistributedLockTime {
        public const Int32 IntervalMillisecond = 5;
        public const Int32 DisposeMillisecond = 60000;
    }
}
