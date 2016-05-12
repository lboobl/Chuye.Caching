using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace Chuye.Caching {
    public static class HttpRuntimeCacheProviderExtensions {
        public static void ExpireAll(this HttpRuntimeCacheProvider httpRuntimeCacheProvider) {
            var entries = HttpRuntime.Cache.OfType<DictionaryEntry>()
                .Where(httpRuntimeCacheProvider.Hit);
            foreach (var entry in entries) {
                HttpRuntime.Cache.Remove((String)entry.Key);
            }
        }

        public static Int32 Count(this HttpRuntimeCacheProvider httpRuntimeCacheProvider) {
            return Count(httpRuntimeCacheProvider, x => true);
        }

        public static Int32 Count(this HttpRuntimeCacheProvider httpRuntimeCacheProvider, Func<String, Boolean> predicate) {
            var entries = HttpRuntime.Cache.OfType<DictionaryEntry>().Where(httpRuntimeCacheProvider.Hit);
            if (predicate != null) {
                entries.Where(r => predicate(httpRuntimeCacheProvider.RemovePrefix((String)r.Key)));
            };
            return entries.Count();
        }
    }
}
