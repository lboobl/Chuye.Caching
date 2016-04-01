using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Caching.Redis {

    public class RedisCacheProvider : CacheProvider, IHttpRuntimeCacheProvider, IRegion {
        private readonly StackExchangeRedis _redis;

        public String Region { get; private set; }

        public RedisCacheProvider(StackExchangeRedis redis)
            : this(redis, null) {
        }

        public RedisCacheProvider(StackExchangeRedis redis, String region) {
            _redis = redis;
            Region = region;
        }

        protected override String BuildCacheKey(String key) {
            return Region == null ? key : String.Concat(Region, "_", key);
        }

        public override void Expire(String key) {
            _redis.Database.KeyDelete(BuildCacheKey(key));
        }

        public override bool TryGet<T>(String key, out T entry) {
            var val = _redis.Database.StringGet(BuildCacheKey(key));
            if (!val.HasValue) {
                entry = default(T);
                return false;
            }
            entry = NewtonsoftJsonUtil.Parse<T>(val);
            return true;

        }

        public T GetOrCreate<T>(String key, Func<T> function, DateTime absoluteExpiration) {
            T value;
            if (TryGet<T>(key, out value)) {
                return value;
            }
            value = function();
            Overwrite(key, value, absoluteExpiration);
            return value;
        }

        public T GetOrCreate<T>(String key, Func<T> function, TimeSpan slidingExpiration) {
            T value;
            if (TryGet<T>(key, out value)) {
                return value;
            }
            value = function();
            Overwrite(key, value, slidingExpiration);
            return value;
        }

        public override void Overwrite<T>(String key, T value) {
            _redis.Database.StringSet(BuildCacheKey(key), NewtonsoftJsonUtil.Stringify(value));
        }

        public void Overwrite<T>(String key, T value, DateTime absoluteExpiration) {
            var key2 = BuildCacheKey(key);
            _redis.Database.StringSet(key2, NewtonsoftJsonUtil.Stringify(value));
            _redis.Database.KeyExpire(key2, absoluteExpiration);
        }

        public void Overwrite<T>(String key, T value, TimeSpan slidingExpiration) {
            var key2 = BuildCacheKey(key);
            _redis.Database.StringSet(key2, NewtonsoftJsonUtil.Stringify(value));
            _redis.Database.KeyExpire(key2, slidingExpiration);
        }
    }
}
