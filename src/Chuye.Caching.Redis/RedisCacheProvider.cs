using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Chuye.Caching.Redis {
    public class RedisCacheProvider : CacheProvider, IDistributedLock, IHttpRuntimeCacheProvider, IRegion {
        private readonly IConnectionMultiplexer _connection;
        private readonly String LOCK = "lock";
        private readonly CacheItemBuilder _cacheItemBuilder;

        public String Region { get; private set; }

        public RedisCacheProvider(IConnectionMultiplexer connection)
            : this(connection, null) {
        }

        public RedisCacheProvider(IConnectionMultiplexer connection, String region) {
            _connection = connection;
            Region = region;
            _cacheItemBuilder = new CacheItemBuilder(this.GetType());
        }

        protected override String BuildCacheKey(String key) {
            return _cacheItemBuilder.BuildCacheKey(Region, key);
        }

        public override void Expire(String key) {
            var db = _connection.GetDatabase();
            db.KeyDelete(BuildCacheKey(key));
        }

        public override bool TryGet<T>(String key, out T value) {
            var db = _connection.GetDatabase();
            var entry = db.StringGet(BuildCacheKey(key));
            if (!entry.HasValue) {
                value = default(T);
                return false;
            }
            value = NewtonsoftJsonUtil.Parse<T>(entry);
            return true;

        }

        public T GetOrCreate<T>(String key, Func<String, T> func, DateTime absoluteExpiration) {
            T value;
            if (TryGet<T>(key, out value)) {
                return value;
            }
            value = func(key);
            Overwrite(key, value, absoluteExpiration);
            return value;
        }

        public T GetOrCreate<T>(String key, Func<String, T> func, TimeSpan slidingExpiration) {
            T value;
            if (TryGet<T>(key, out value)) {
                return value;
            }
            value = func(key);
            Overwrite(key, value, slidingExpiration);
            return value;
        }

        public override void Overwrite<T>(String key, T value) {
            var expiration = _cacheItemBuilder.GetMaxExpiration();
            if (expiration.HasValue) {
                Overwrite(key, value, expiration.Value);
            }
            else {
                var db = _connection.GetDatabase();
                db.StringSet(BuildCacheKey(key), NewtonsoftJsonUtil.Stringify(value));
            }
        }

        public void Overwrite<T>(String key, T value, DateTime absoluteExpiration) {
            var cacheKey = BuildCacheKey(key);
            var db = _connection.GetDatabase();
            db.StringSet(cacheKey, NewtonsoftJsonUtil.Stringify(value));
            db.KeyExpire(cacheKey, absoluteExpiration);
        }

        public void Overwrite<T>(String key, T value, TimeSpan slidingExpiration) {
            var cacheKey = BuildCacheKey(key);
            var db = _connection.GetDatabase();
            db.StringSet(cacheKey, NewtonsoftJsonUtil.Stringify(value));
            db.KeyExpire(cacheKey, slidingExpiration);
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
            var db = _connection.GetDatabase();
            return db.LockTake(key, LOCK, TimeSpan.FromMilliseconds(milliseconds));
        }

        public void UnLock(String key) {
            var db = _connection.GetDatabase();
            db.LockRelease(key, LOCK);
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
