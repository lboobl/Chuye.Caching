using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Caching {
    public interface IRegionHttpRuntimeCacheProvider : IHttpRuntimeCacheProvider {
        String Region { get; }
        IRegionHttpRuntimeCacheProvider Switch(String region);
    }
}
