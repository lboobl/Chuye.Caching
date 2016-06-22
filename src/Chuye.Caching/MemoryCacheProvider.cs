using System;
using System.Collections;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Linq;
using System.Web.Script.Serialization;

namespace Chuye.Caching {
    public class MemoryCacheProvider : BasicCacheProvider, IRegionCacheProvider {
        private static readonly Object _nullEntry = new Object();
        private readonly String _prefix;
        private readonly String _region;

        public String Region {
            get { return _region; }
        }

        public MemoryCacheProvider()
            : this(null) {
        }

        public MemoryCacheProvider(String region) {
            _region = region;
            _prefix = BuildCacheKey(null);
        }

        public IRegionCacheProvider Switch(String region) {
            if (!String.IsNullOrWhiteSpace(_region)) {
                throw new InvalidOperationException();
            }
            return new MemoryCacheProvider(region);
        }

        private Boolean InnerTryGet(String key, out Object value) {
            value = HttpRuntime.Cache.Get(key);
            return value != null;
        }

        public override bool TryGet<T>(String key, out T value) {
            String cacheKey = BuildCacheKey(key);
            Object cacheValue;

            var exists = InnerTryGet(cacheKey, out cacheValue);
            if (!exists) {
                value = default(T);
                return false;
            }
            if (cacheValue == _nullEntry) {
                value = (T)((Object)null);
                return true;
            }
            if (cacheValue is T) {
                value = (T)cacheValue;
                return true;
            }

            //cacheEntry is not a t
            throw new InvalidOperationException(String.Format("Cache entry`[{0}]` type error, {1} or {2} ?",
                key, cacheValue.GetType().FullName, typeof(T).FullName));
        }

        protected override String BuildCacheKey(String key) {
            return String.Concat("HRCP-", _region, "-", key);
        }

        protected override Object BuildCacheValue<T>(T value) {
            if (value == null) {
                return _nullEntry;
            }
            return value;
        }

        public T GetOrCreate<T>(String key, Func<String, T> func, TimeSpan slidingExpiration) {
            T value;
            if (TryGet(key, out value)) {
                return value;
            }
            value = func(key);
            Overwrite(key, value, slidingExpiration);
            return value;
        }

        public T GetOrCreate<T>(String key, Func<String, T> func, DateTime absoluteExpiration) {
            T value;
            if (TryGet(key, out value)) {
                return value;
            }
            value = func(key);
            Overwrite(key, value, absoluteExpiration);
            return value;
        }

        public override void Overwrite<T>(String key, T value) {
            HttpRuntime.Cache.Insert(BuildCacheKey(key), BuildCacheValue(value));
        }

        //slidingExpiration 时间内无访问则过期
        public void Overwrite<T>(String key, T value, TimeSpan slidingExpiration) {
            HttpRuntime.Cache.Insert(BuildCacheKey(key), BuildCacheValue(value), null,
                Cache.NoAbsoluteExpiration, slidingExpiration);
        }

        public void Overwrite<T>(String key, T value, TimeSpan slidingExpiration, CacheItemUpdateCallback expireCallback) {
            HttpRuntime.Cache.Insert(BuildCacheKey(key), BuildCacheValue(value), null,
                Cache.NoAbsoluteExpiration, slidingExpiration, expireCallback);
        }

        //absoluteExpiration 时过期
        public void Overwrite<T>(String key, T value, DateTime absoluteExpiration) {
            HttpRuntime.Cache.Insert(BuildCacheKey(key), BuildCacheValue(value), null,
                absoluteExpiration, Cache.NoSlidingExpiration);
        }

        public void Overwrite<T>(String key, T value, DateTime absoluteExpiration, CacheItemUpdateCallback expireCallback) {
            HttpRuntime.Cache.Insert(BuildCacheKey(key), BuildCacheValue(value), null,
                absoluteExpiration, Cache.NoSlidingExpiration, expireCallback);
        }

        public override void Expire(String key) {
            HttpRuntime.Cache.Remove(BuildCacheKey(key));
        }

        public void Flush(String file, Func<String, Boolean> predicate) {
            using (var stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            using (var writer = new StreamWriter(stream)) {
                stream.SetLength(0L);
                var entries = HttpRuntime.Cache.OfType<DictionaryEntry>().Where(Hit);
                if (predicate != null) {
                    entries.Where(r => predicate(RemovePrefix((String)r.Key)));
                }
                var json = new JavaScriptSerializer();
                foreach (var entry in entries) {
                    writer.WriteLine(json.Serialize(entry));
                }
                writer.Flush();
            }
        }

        internal String RemovePrefix(String key) {
            return key.Substring(_prefix.Length);
        }

        internal Boolean Hit(DictionaryEntry entry) {
            return (entry.Key is String) && ((String)entry.Key).StartsWith(_prefix);
        }
    }
}
