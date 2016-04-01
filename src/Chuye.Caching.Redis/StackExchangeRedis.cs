using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Chuye.Caching.Redis {
    public class StackExchangeRedis : IDistributedLock, IDisposable {
        private static StackExchangeRedis _default;
        private readonly ConnectionMultiplexer _connection;
        private readonly String LOCK = "lock";

        public static StackExchangeRedis Default {
            get {
                if (_default != null) {
                    return _default;
                }

                var connectionString = ConfigurationManager.AppSettings.Get("cache:redis");
                if (String.IsNullOrWhiteSpace(connectionString)) {
                    throw new ArgumentOutOfRangeException("cache:redis", "Configuration \"cache:redis\" missing");
                }
                var defaultInstance = new StackExchangeRedis(connectionString);
                if (Interlocked.CompareExchange(ref _default, defaultInstance, null) != null) {
                    defaultInstance.Dispose();
                }
                return _default;
            }
        }
        
        public StackExchangeRedis(String connectionString) {
            if (String.IsNullOrWhiteSpace(connectionString)) {
                throw new ArgumentOutOfRangeException("connectionString");
            }
            _connection = ConnectionMultiplexer.Connect(connectionString);
        }

        public IDatabase Database {
            get {
                return _connection.GetDatabase();
            }
        }

        public void Dispose() {
            using (_connection) { };
        }

        public IDisposable ReleasableLock(String key, Int32 milliseconds = DistributedLockTime.DisposeMillisecond) { 
            Lock(key, milliseconds);
            return new RedisLock(this, key);
        }

        public void Lock(String key, Int32 milliseconds) {
            while (!TryLock(key, milliseconds)) {
                Thread.SpinWait(1000);
            }
        }

        public Boolean TryLock(String key, Int32 milliseconds) {
            return Database.LockTake(key, LOCK, TimeSpan.FromMilliseconds(milliseconds));
        }

        public void UnLock(String key) {
            Database.LockRelease(key, LOCK);
        }

        class RedisLock : IDisposable {
            private readonly IDistributedLock _redis;
            private readonly String _key;
            public RedisLock(IDistributedLock redis, String key) {
                _redis = redis;
                _key = key;
            }

            public void Dispose() {
                _redis.UnLock(_key);
            }
        }
    }
}