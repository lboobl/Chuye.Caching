using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Caching {
    public static class CacheConfigBuilder {
        public static CacheConfig Build(Type providerType, String region) {
            var section = new ConfigurationResolver().Read<CacheConfigurationSection>("cacheBuilder");
            return Build(providerType, region, section);
        }

        public static CacheConfig Build(Type providerType, String region, CacheConfigurationSection section) {
            if (section == null) {
                return CacheConfig.Empty;
            }

            var provider = providerType.FullName;
            CacheItemDetailElement detail = null;

            foreach (var item in section.Details.OfType<CacheItemDetailElement>()) {
                if (item.Provider == provider && item.Region == region) {
                    detail = item;
                    break;
                }
            }

            TimeSpan? maxExpiration = null;
            if (detail == null) {
                if (section.MaxExpirationHour > 0) {
                    maxExpiration = TimeSpan.FromHours(section.MaxExpirationHour);
                }
                return new CacheConfig(section.Pattern, section.Readonly, section.FormatNullRegion, maxExpiration);
            }

            var pattern = detail.Pattern;
            if (String.IsNullOrWhiteSpace(pattern)) {
                pattern = section.Pattern;
            }
            if (detail.MaxExpirationHour > 0) {
                maxExpiration = TimeSpan.FromHours(section.MaxExpirationHour);
            }
            return new CacheConfig(pattern, detail.Readonly, detail.FormatNullRegion, maxExpiration);
        }
    }
}
