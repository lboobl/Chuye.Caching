using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Caching {
    public interface IBasicCacheProvider {
        Boolean TryGet<T>(String key, out T value);
        T GetOrCreate<T>(String key, Func<String, T> func);
        void Overwrite<T>(String key, T value);
        void Expire(String key);
    }
}
