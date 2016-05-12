using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Caching {
    public interface IHttpRuntimeCacheProvider : ICacheProvider {
        T GetOrCreate<T>(String key, Func<String, T> func, TimeSpan slidingExpiration);
        T GetOrCreate<T>(String key, Func<String, T> func, DateTime absoluteExpiration);
        void Overwrite<T>(String key, T value, TimeSpan slidingExpiration);
        void Overwrite<T>(String key, T value, DateTime absoluteExpiration);
    }
}
