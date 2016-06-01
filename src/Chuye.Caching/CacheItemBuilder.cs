using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Caching {
    public class CacheItemBuilder {
        private readonly CacheItemConfigurationSection _section;
        private readonly Type _cacheProviderType;
        private readonly String _region;

        static CacheItemConfigurationSection ReadDefaultSection() {
            return new ConfigurationResolver().Read<CacheItemConfigurationSection>("regionPattern");
        }

        public CacheItemBuilder(Type cacheProviderType, String region)
            : this(cacheProviderType, region, ReadDefaultSection()) {
        }

        public CacheItemBuilder(Type cacheProviderType, String region, CacheItemConfigurationSection section) {
            _cacheProviderType = cacheProviderType;
            _region = region;
            _section = section;
        }

        public String BuildCacheKey(String key) {
            var pattern = "{0}-{1}";
            var leaveDashForEmtpyRegion = true;
            if (_section != null) {
                pattern = ResolvePattern(out leaveDashForEmtpyRegion);
            }
            if (String.IsNullOrWhiteSpace(_region) && !leaveDashForEmtpyRegion) {
                return key;
            }
            else {
                return String.Format(pattern, _region, key);
            }
        }

        public Boolean IsReadonly() {
            var detail = _section.Details.Get(_cacheProviderType.FullName);
            if (detail != null) {
                if (String.IsNullOrWhiteSpace(detail.Region)) {
                    return detail.Readonly;
                }
                else {
                    return detail.Readonly
                        && detail.Region.Equals(_region, StringComparison.Ordinal);
                }
            }
            return _section.Readonly;
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
