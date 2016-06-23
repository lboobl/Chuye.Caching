using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Caching {
    public abstract class CacheProvider : ICacheProvider {
        protected virtual String BuildCacheKey(String key) {
            return key;
        }

        protected virtual Object BuildCacheValue<T>(T value) {
            return value;
        }

        public abstract Boolean TryGet<T>(String key, out T value);

        public virtual T GetOrCreate<T>(String key, Func<String, T> func) {
            T entry;
            if (TryGet(key, out entry)) {
                return entry;
            }
            entry = func(key);
            Overwrite(key, entry);
            return entry;
        }

        public virtual T GetOrCreate<T>(String key, Func<string, T> func, TimeSpan slidingExpiration) {
            T entry;
            if (TryGet(key, out entry)) {
                return entry;
            }
            entry = func(key);
            Overwrite(key, entry, slidingExpiration);
            return entry;
        }

        public virtual T GetOrCreate<T>(String key, Func<string, T> func, DateTime absoluteExpiration) {
            T entry;
            if (TryGet(key, out entry)) {
                return entry;
            }
            entry = func(key);
            Overwrite(key, entry, absoluteExpiration);
            return entry;
        }

        public abstract void Overwrite<T>(String key, T value);
        public abstract void Overwrite<T>(String key, T value, TimeSpan slidingExpiration);
        public abstract void Overwrite<T>(String key, T value, DateTime absoluteExpiration);
        public abstract void Expire(String key);
    }
}
