﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Chuye.Caching.Redis {
    public class RedisCacheProvider : BasicCacheProvider, IDistributedLock, IRegionCacheProvider {
        private readonly IConnectionMultiplexer _connection;
        private readonly String LOCK = "lock";
        private readonly String _region;
        private readonly CacheBuilder _cacheBuilder;

        public String Region {
            get { return _region; }
        }

        public IConnectionMultiplexer Connection {
            get { return _connection; }
        }

        public RedisCacheProvider(String configuration)
            : this(configuration, null) {
        }

        public RedisCacheProvider(String configuration, String region) {
            if (String.IsNullOrWhiteSpace(configuration)) {
                throw new ArgumentOutOfRangeException("configuration");
            }
            _connection   = ConnectionMultiplexer.Connect(configuration);
            _region       = region;
            _cacheBuilder = new CacheBuilder(this.GetType(), region);
        }

        internal RedisCacheProvider(String configuration, String region, CacheBuilder cacheBuilder) {
            if (String.IsNullOrWhiteSpace(configuration)) {
                throw new ArgumentOutOfRangeException("configuration");
            }
            _connection   = ConnectionMultiplexer.Connect(configuration);
            _region       = region;
            _cacheBuilder = cacheBuilder;
        }

        public RedisCacheProvider(IConnectionMultiplexer connection)
            : this(connection, null) {
        }

        public RedisCacheProvider(IConnectionMultiplexer connection, String region) {
            if (connection == null) {
                throw new ArgumentOutOfRangeException("connection");
            }

            _connection   = connection;
            _region       = region;
            _cacheBuilder = new CacheBuilder(this.GetType(), region);
        }

        internal RedisCacheProvider(IConnectionMultiplexer connection, String region, CacheBuilder cacheBuilder) {
            if (connection == null) {
                throw new ArgumentOutOfRangeException("connection");
            }

            _connection   = connection;
            _region       = region;
            _cacheBuilder = cacheBuilder;
        }

        public IRegionCacheProvider Switch(String region) {
            if (!String.IsNullOrWhiteSpace(Region)) {
                throw new InvalidOperationException();
            }
            var cacheBuilder = new CacheBuilder(_cacheBuilder, region);
            return new RedisCacheProvider(_connection, region, cacheBuilder);
        }

        protected override String BuildCacheKey(String key) {
            return _cacheBuilder.BuildCacheKey(key);
        }

        public override void Expire(String key) {
            if (_cacheBuilder.IsReadonly()) {
                throw new InvalidOperationException();
            }
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
            if (_cacheBuilder.IsReadonly()) {
                throw new InvalidOperationException();
            }
            var expiration = _cacheBuilder.GetMaxExpiration();
            if (expiration.HasValue) {
                Overwrite(key, value, expiration.Value);
            }
            else {
                var db = _connection.GetDatabase();
                db.StringSet(BuildCacheKey(key), NewtonsoftJsonUtil.Stringify(value));
            }
        }

        public void Overwrite<T>(String key, T value, DateTime absoluteExpiration) {
            if (_cacheBuilder.IsReadonly()) {
                throw new InvalidOperationException();
            }
            var cacheKey = BuildCacheKey(key);
            var db = _connection.GetDatabase();
            db.StringSet(cacheKey, NewtonsoftJsonUtil.Stringify(value));
            db.KeyExpire(cacheKey, absoluteExpiration);
        }

        public void Overwrite<T>(String key, T value, TimeSpan slidingExpiration) {
            if (_cacheBuilder.IsReadonly()) {
                throw new InvalidOperationException();
            }
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
