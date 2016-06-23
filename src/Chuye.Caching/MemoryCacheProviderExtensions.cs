using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Chuye.Caching {
    public static class MemoryCacheProviderExtensions {
        public static void ExpireAll(this MemoryCacheProvider cacheProvider) {
            var entries = HttpRuntime.Cache.OfType<DictionaryEntry>()
                .Where(cacheProvider.Hit);
            foreach (var entry in entries) {
                HttpRuntime.Cache.Remove((String)entry.Key);
            }
        }

        public static Int32 Count(this MemoryCacheProvider cacheProvider) {
            return Count(cacheProvider, x => true);
        }

        public static Int32 Count(this MemoryCacheProvider cacheProvider, Func<String, Boolean> predicate) {
            var entries = HttpRuntime.Cache.OfType<DictionaryEntry>().Where(cacheProvider.Hit);
            if (predicate != null) {
                entries.Where(r => predicate(cacheProvider.RemovePrefix((String)r.Key)));
            };
            return entries.Count();
        }
    }
}
