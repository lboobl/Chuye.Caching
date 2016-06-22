using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Caching {
    public class CacheConfig {
        public static CacheConfig Empty = new CacheConfig();

        public String Pattern { get; private set; }
        public Boolean Readonly { get; private set; }
        public TimeSpan? MaxExpiration { get; private set; }
        public Boolean FormatNullRegion { get; private set; }

        public String BuildCacheKey(String region, String key) {
            if (String.IsNullOrWhiteSpace(region) && !FormatNullRegion) {
                return key;
            }
            else {
                return String.Format(Pattern, region, key);
            }
        }

        public CacheConfig(String pattern = "{region}-{key}", Boolean readOnly = false, Boolean leaveExraConnector = false, TimeSpan? maxExpiration = null) {
            if (pattern.IndexOf("{region}") == -1 || pattern.IndexOf("{key}") == -1) {
                throw new ArgumentOutOfRangeException("pattern");
            }
            Pattern = pattern.Replace("{region}", "{0}").Replace("{key}", "{1}");
            Readonly = readOnly;
            MaxExpiration = maxExpiration;
            FormatNullRegion = leaveExraConnector;
        }
    }
}
