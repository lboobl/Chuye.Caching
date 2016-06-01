using System;
using Chuye.Caching.Redis;
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
                Provider = typeof(RedisCacheProvider).FullName,
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
                sectionRead.Details.Get(typeof(RedisCacheProvider).FullName),
                sectionWrite.Details.Get(typeof(RedisCacheProvider).FullName)
            );
        }

        [TestMethod]
        public void Set_default_value_then_build() {
            var section = new CacheItemConfigurationSection {
                Pattern = "{region}-{key}",
            };

            var region = Guid.NewGuid().ToString();
            var builder = new CacheItemBuilder(typeof(RedisCacheProvider), region);
            Assert.IsNull(builder.GetMaxExpiration());
            Assert.IsFalse(builder.IsReadonly());

            section.MaxExpiration = Math.Abs(Guid.NewGuid().GetHashCode() % 100);
            Assert.AreEqual(builder.GetMaxExpiration().Value.Days, section.MaxExpiration);

            section.Readonly = true;
            Assert.IsTrue(builder.IsReadonly());

            var key = builder.BuildCacheKey("key");
            Assert.AreEqual(key, region + "-key");
        }

        [TestMethod]
        public void Set_region_then_build() {
            var section = new CacheItemConfigurationSection {
                Pattern = "{region}-{key}",
                LeaveDashForEmtpyRegion = true,
                Details = new CacheItemElementCollection(),
                MaxExpiration = Math.Abs(Guid.NewGuid().GetHashCode() % 100),
                Readonly = true
            };

            var builder = new CacheItemBuilder(typeof(RedisCacheProvider), null);
            Assert.IsTrue(builder.IsReadonly());
            Assert.AreEqual(builder.GetMaxExpiration().Value.Days, section.MaxExpiration);

            var key = builder.BuildCacheKey("key");
            Assert.AreEqual(key, "key");
        }
    }
}
