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
    public static class MemoryCacheProviderExtensions {
        public static void ExpireAll(this MemoryCacheProvider cache) {
            var entries = HttpRuntime.Cache.OfType<DictionaryEntry>()
                .Where(cache.Hit);
            foreach (var entry in entries) {
                HttpRuntime.Cache.Remove((String)entry.Key);
            }
        }

        public static void Flush(this MemoryCacheProvider cache, String file, Func<String, Boolean> predicate) {
            using (var stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            using (var writer = new StreamWriter(stream)) {
                stream.SetLength(0L);
                var entries = HttpRuntime.Cache.OfType<DictionaryEntry>().Where(cache.Hit);
                if (predicate != null) {
                    entries.Where(r => predicate(cache.RemovePrefix((String)r.Key)));
                }
                var json = new JavaScriptSerializer();
                foreach (var entry in entries) {
                    writer.WriteLine(json.Serialize(entry));
                }
                writer.Flush();
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
