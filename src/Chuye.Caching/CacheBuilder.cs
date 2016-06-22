using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Caching {
    public class CacheBuilder {
        private readonly CacheConfigurationSection _section;
        private readonly Type _providerType;
        private readonly String _region;
        private CacheItemDetailElement _config;

        static CacheConfigurationSection ReadDefaultSection() {
            return new ConfigurationResolver().Read<CacheConfigurationSection>("cacheBuilder");
        }

        public CacheBuilder(Type providerType, String region)
            : this(providerType, region, ReadDefaultSection()) {
        }

        public CacheBuilder(Type providerType, String region, CacheConfigurationSection section) {
            _providerType = providerType;
            _region       = region;
            _section      = section;
            RefreshConfig();
        }

        public CacheBuilder(CacheBuilder cacheBuidler, String region) {
            _providerType = cacheBuidler._providerType;
            _section      = cacheBuidler._section;
            _region       = region;
            RefreshConfig();
        }

        private void RefreshConfig() {
            _config = null;
            if (_section != null) {
                _config = _section.SelectEffectiveDetail(_providerType.FullName, _region);
                if (_config.Pattern.IndexOf("{region}") == -1 || _config.Pattern.IndexOf("{key}") == -1) {
                    throw new ArgumentOutOfRangeException("pattern");
                }
                _config.Pattern = _config.Pattern
                    .Replace("{region}", "{0}")
                    .Replace("{key}", "{1}");
            }
            else {
                _config = new CacheItemDetailElement {
                    Pattern = "{0}-{1}"
                };
            }
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
            if (_config.MaxExpiration > 0) {
                return TimeSpan.FromDays(_config.MaxExpiration);
            }
            return null;
        }
    }
}
