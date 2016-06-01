using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chuye.Caching.Tests {
    [TestClass]
    public class RegionConfigurationSectionTest {
        [TestMethod]
        public void Set_configrationSection_then_read() {
            var resolver = new ConfigurationResolver();
            resolver.ExeConfigFilename = "test.dll";
            var sectionWrite = new CacheItemConfigurationSection {
                Pattern = "{region}-{key}",
                LeaveDashForEmtpyRegion = true,
                Details = new CacheItemElementCollection()
            };
            sectionWrite.MaxExpiration = 30D;
            sectionWrite.Details.Add(new CacheItemDetailElement {
                Pattern = "{region}-{key}",
                Provider = typeof(HttpRuntimeCacheProvider).FullName,
                LeaveDashForEmtpyRegion = false,
            });
            resolver.Save(sectionWrite, "regionPattern");

            var sectionRead = resolver.Read<CacheItemConfigurationSection>("regionPattern");
            Assert.AreEqual(sectionRead.Pattern, sectionWrite.Pattern);
            Assert.AreEqual(sectionRead.LeaveDashForEmtpyRegion, sectionWrite.LeaveDashForEmtpyRegion);
            Assert.AreEqual(sectionRead.MaxExpiration, sectionWrite.MaxExpiration);
            Assert.IsNotNull(sectionRead.Details);
            Assert.AreEqual(sectionRead.Details.Count, sectionWrite.Details.Count);
            Assert.AreEqual(
                sectionRead.Details.Get(typeof(HttpRuntimeCacheProvider).FullName),
                sectionWrite.Details.Get(typeof(HttpRuntimeCacheProvider).FullName)
            );
        }

        [TestMethod]
        public void Set_configrationSection_then_build_cache_key() {
            var section = new CacheItemConfigurationSection {
                Pattern = "{region}-{key}",
                LeaveDashForEmtpyRegion = false,
                Details = new CacheItemElementCollection()
            };

            var provider = new CacheItemBuilder(section, typeof(HttpRuntimeCacheProvider));
            Assert.AreEqual("key", provider.BuildCacheKey(null, "key"));
            Assert.AreEqual("region-key", provider.BuildCacheKey("region", "key"));

            section.LeaveDashForEmtpyRegion = true;
            Assert.AreEqual("-key", provider.BuildCacheKey(null, "key"));

            section.Details.Add(new CacheItemDetailElement {
                Pattern = "{key}+{region}",
                Provider = typeof(HttpRuntimeCacheProvider).FullName,
                LeaveDashForEmtpyRegion = false,
            });

            Assert.AreEqual("key", provider.BuildCacheKey(null, "key"));
            Assert.AreEqual("key+region", provider.BuildCacheKey("region", "key"));
        }
    }
}
