using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Caching {
    public class CacheBuilder {
        private readonly CacheItemDetailElement _config;
        private readonly Type _cacheProviderType;
        private readonly String _region;

        static CacheConfigurationSection ReadDefaultSection() {
            return new ConfigurationResolver().Read<CacheConfigurationSection>("cacheBuilder");
        }

        public CacheBuilder(Type cacheProviderType, String region)
            : this(cacheProviderType, region, ReadDefaultSection()) {
        }

        public CacheBuilder(Type cacheProviderType, String region, CacheConfigurationSection section) {
            _cacheProviderType = cacheProviderType;
            _region = region;
            _config = section.SelectEffectiveDetail(_cacheProviderType.FullName, _region);
            if (_config.Pattern.IndexOf("{region}") == -1 || _config.Pattern.IndexOf("{key}") == -1) {
                throw new ArgumentOutOfRangeException("pattern");
            }
            _config.Pattern = _config.Pattern
                .Replace("{region}", "{0}")
                .Replace("{key}", "{1}");
        }

        public String BuildCacheKey(String key) {
            if (String.IsNullOrWhiteSpace(_region) && !_config.LeaveDashForEmtpyRegion) {
                return key;
            }
            else {
                return String.Format(_config.Pattern, _region, key);
            }
        }

        public Boolean IsReadonly() {
            return _config != null && _config.Readonly;

        }

        public TimeSpan? GetMaxExpiration() {
            if (_config != null && _config.MaxExpiration > 0) {
                return TimeSpan.FromDays(_config.MaxExpiration);
            }

            if (_config.MaxExpiration > 0) {
                return TimeSpan.FromDays(_config.MaxExpiration);
            }
            return null;
        }
    }
}
