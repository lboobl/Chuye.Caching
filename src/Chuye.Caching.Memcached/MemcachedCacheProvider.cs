﻿using System;
using System.Configuration;
using System.Threading;
using Enyim.Caching;
using Enyim.Caching.Memcached;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Chuye.Caching.Memcached {
    public class MemcachedCacheProvider : CacheProvider, IRegionCacheProvider, IDistributedLock {
        private static MemcachedCacheProvider _default;
        private readonly MemcachedClient _client;
        private readonly CacheConfig _config;
        private readonly String _region;

        public static MemcachedCacheProvider Default {
            get {
                if (_default != null) {
                    return _default;
                }

                var defaultInstance = new MemcachedCacheProvider("enyim.com/memcached", null);
                Interlocked.CompareExchange(ref _default, defaultInstance, null);
                return _default;
            }
        }
        public String Region {
            get { return _region; }
        }

        public MemcachedCacheProvider()
            : this("enyim.com/memcached", null) {
        }

        public MemcachedCacheProvider(String configSection)
            : this(configSection, null) {
        }

        public MemcachedCacheProvider(String configSection, String region)
            : this(new MemcachedClient(configSection), region) {
        }

        public MemcachedCacheProvider(MemcachedClient client, String region)
            : this(client, region, CacheConfigBuilder.Build(typeof(MemcachedCacheProvider), region)) {
        }

        public MemcachedCacheProvider(MemcachedClient client, String region, CacheConfig config) {
            _client = client;
            _region = region;
            _config = config;
        }

        public ICacheProvider Switch(String region) {
            if (!String.IsNullOrWhiteSpace(Region)) {
                throw new InvalidOperationException();
            }
            var config = CacheConfigBuilder.Build(typeof(MemcachedCacheProvider), region);
            return new MemcachedCacheProvider(_client, region, config);
        }

        protected override String BuildCacheKey(String key) {
            return _config.BuildCacheKey(Region, key);
        }

        public override bool TryGet<T>(string key, out T value) {
            String cacheKey = BuildCacheKey(key);
            Object cacheValue;

            var exists = _client.TryGet(cacheKey, out cacheValue);
            if (!exists) {
                value = default(T);
                return false;
            }
            if (cacheValue is T) {
                value = (T)cacheValue;
                return true;
            }
            if (cacheValue == null) {
                value = (T)((Object)null);
                return true;
            }
            //使用与不使用 NewtonsoftJsonTranscoder 的情况下都支持
            SlidingCacheWrapper<T> slidingCache;
            if (SlidingCacheWrapper<T>.IsSlidingCache(cacheValue, out slidingCache)) {
                //尝试以 SlidingCacheWrapper<T> 处理
                var diffSpan = DateTime.Now.Subtract(slidingCache.SettingTime);
                //当前时间-设置时间>滑动时间, 已经过期
                if (diffSpan > slidingCache.SlidingExpiration) {
                    Expire(key);
                    value = default(T);
                    return false;
                }

                //当前时间-设置时间> 滑动时间/2, 更新缓存
                if (diffSpan.Add(diffSpan) > slidingCache.SlidingExpiration) {
                    Overwrite(key, slidingCache.Value, slidingCache.SlidingExpiration);
                }
                value = slidingCache.Value;
            }
            else {
                //尝试以普通JSON处理
                value = NewtonsoftJsonUtil.EnsureObjectType<T>(cacheValue);
            }
            return true;
        }

        public override void Overwrite<T>(String key, T value) {
            if (_config.Readonly) {
                throw new InvalidOperationException();
            }
            if (_config.MaxExpiration.HasValue) {
                Overwrite(key, value, _config.MaxExpiration.Value);
            }
            else {
                _client.Store(StoreMode.Set, BuildCacheKey(key), value);
            }
        }


        //slidingExpiration 时间内无访问则过期
        public override void Overwrite<T>(String key, T value, TimeSpan slidingExpiration) {
            if (_config.Readonly) {
                throw new InvalidOperationException();
            }
            //_client.Store(StoreMode.Set, BuildCacheKey(key), value, slidingExpiration);
            var cacheWraper = new SlidingCacheWrapper<T>(value, slidingExpiration);
            _client.Store(StoreMode.Set, BuildCacheKey(key), cacheWraper,
                TimeSpan.FromSeconds(slidingExpiration.TotalSeconds * 1.5));
        }

        //absoluteExpiration UTC或本地时间均可
        public override void Overwrite<T>(String key, T value, DateTime absoluteExpiration) {
            if (_config.Readonly) {
                throw new InvalidOperationException();
            }
            _client.Store(StoreMode.Set, BuildCacheKey(key), value, absoluteExpiration);
        }

        public override void Expire(String key) {
            if (_config.Readonly) {
                throw new InvalidOperationException();
            }
            _client.Remove(BuildCacheKey(key));  // Could check result
        }

        public IDisposable ReleasableLock(String key, Int32 expire = DistributedLockTime.DisposeMillisecond) {
            while (!TryLock(key, expire)) {
                Thread.Sleep(DistributedLockTime.IntervalMillisecond);
            }
            return new MemcachedLockReleaser(this, key);
        }

        public void Lock(String key, Int32 expire) {
            while (!TryLock(key, expire)) {
                Thread.Sleep(DistributedLockTime.IntervalMillisecond);
            }
        }

        public Boolean TryLock(String key, Int32 expire) {
            var result = _client.ExecuteStore(StoreMode.Add, BuildCacheKey(key), 1, TimeSpan.FromMilliseconds(expire));
            return result.Success;
        }

        public void UnLock(String key) {
            Expire(key);
        }

        private struct MemcachedLockReleaser : IDisposable {
            private MemcachedCacheProvider _client;
            private String _key;

            public MemcachedLockReleaser(MemcachedCacheProvider client, String key) {
                _client = client;
                _key = key;
            }

            public void Dispose() {
                _client.UnLock(_key);
            }
        }

        [Serializable]
        public class SlidingCacheWrapper<T> {
            private const String SlidingExpirationProp = "0091081c219a456982dc7c881cce70c1";
            private const String SettingTimeProp = "fb7ec3ccf9764c7f9c4d4abe0878286b";

            public T Value { get; private set; }
            [JsonProperty(SlidingExpirationProp)]
            public TimeSpan SlidingExpiration { get; private set; }
            [JsonProperty(SettingTimeProp)]
            public DateTime SettingTime { get; set; }

            public SlidingCacheWrapper(T value, TimeSpan slidingExpiration) {
                Value = value;
                SlidingExpiration = slidingExpiration;
                SettingTime = DateTime.Now;
            }

            public static Boolean IsSlidingCache(Object obj, out SlidingCacheWrapper<T> cacheEntry) {
                cacheEntry = null;
                if (obj is SlidingCacheWrapper<T>) {
                    cacheEntry = (SlidingCacheWrapper<T>)obj;
                    return true;
                }
                if (obj is JObject) {
                    var jobj = (JObject)obj;
                    if (jobj.Property(SlidingExpirationProp) != null && jobj.Property(SettingTimeProp) != null) {
                        cacheEntry = jobj.ToObject<SlidingCacheWrapper<T>>();
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
