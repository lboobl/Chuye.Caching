using System;
using System.Web;

namespace Chuye.Caching {
    public class HttpContextCacheProvider : BasicCacheProvider {
        private static readonly Object _nullEntry = new Object();
        private const String _prefix = "HCCP-";

        public HttpContextCacheProvider() {
            if (HttpContext.Current == null) {
#if DEBUG
                HttpContext.Current = new HttpContext(new HttpRequest(null, "http://localhost", null), new HttpResponse(null));
#else
                throw new InvalidOperationException("Must under web environment");
#endif

            }
        }

        protected override String BuildCacheKey(String key) {
            return String.Concat(_prefix, key);
        }

        protected override Object BuildCacheValue<T>(T value) {
            if (value == null) {
                return _nullEntry;
            }
            return value;
        }

        private Boolean InnerTryGet(String key, out Object value) {
            value = HttpContext.Current.Items[key];
            return value != null;
        }

        public override Boolean TryGet<T>(String key, out T value) {
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

        public override void Overwrite<T>(String key, T entry) {
            HttpContext.Current.Items[BuildCacheKey(key)] = BuildCacheValue(entry);
        }

        public override void Expire(String key) {
            HttpContext.Current.Items.Remove(BuildCacheKey(key));
        }
    }
}
