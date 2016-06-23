using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Caching {
    public interface IRegionCacheProvider : ICacheProvider {
        String Region { get; }
        IRegionCacheProvider Switch(String region);
    }
}
