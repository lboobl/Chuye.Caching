using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Caching {
    public class CacheItemBuilder {
        private readonly CacheItemConfigurationSection _section;
        private readonly Type _cacheProviderType;

        static CacheItemConfigurationSection Default {
            get {
                return new ConfigurationResolver().Read<CacheItemConfigurationSection>("regionPattern");
            }
        }

        public CacheItemBuilder(Type cacheProviderType)
            : this(Default, cacheProviderType) {
        }

        public CacheItemBuilder(CacheItemConfigurationSection section, Type cacheProviderType) {
            _cacheProviderType = cacheProviderType;
            _section = section;
        }

        public String BuildCacheKey(String region, String key) {
            var pattern = "{0}-{1}";
            var leaveDashForEmtpyRegion = true;
            if (_section != null) {
                pattern = ResolvePattern(out leaveDashForEmtpyRegion);
            }
            if (String.IsNullOrWhiteSpace(region) && !leaveDashForEmtpyRegion) {
                return key;
            }
            else {
                return String.Format(pattern, region, key);
            }
        }

        public TimeSpan? GetMaxExpiration() {
            var detail = _section.Details.Get(_cacheProviderType.FullName);
            if (detail != null && detail.MaxExpiration > 0) {
                return TimeSpan.FromDays(detail.MaxExpiration);
            }

            if (_section.MaxExpiration > 0) {
                return TimeSpan.FromDays(_section.MaxExpiration);
            }
            return null;
        }

        private String ResolvePattern(out bool leaveDashForEmtpyRegion) {
            var pattern = _section.Pattern;
            leaveDashForEmtpyRegion = _section.LeaveDashForEmtpyRegion;
            var detail = _section.Details.Get(_cacheProviderType.FullName);
            if (detail != null) {
                pattern = detail.Pattern;
                leaveDashForEmtpyRegion = detail.LeaveDashForEmtpyRegion;
            }
            //Pattern = "{region}-{key}",
            if (pattern.IndexOf("{region}") == -1 || pattern.IndexOf("{key}") == -1) {
                throw new ArgumentOutOfRangeException("pattern");
            }
            pattern = pattern.Replace("{region}", "{0}");
            pattern = pattern.Replace("{key}", "{1}");
            return pattern;
        }
    }
}
