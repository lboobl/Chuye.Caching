using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chuye.Caching.Redis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chuye.Caching.Tests {
    [TestClass]
    public class CacheConfigBuilderTest {
        [TestMethod]
        public void Set_configrationSection_then_read() {
            var section1 = new CacheConfigurationSection {
                Pattern = "{region}/{key}",
                FormatNullRegion = true,
                MaxExpirationHour = 1D,
                Details = new CacheItemElementCollection()
            };
            section1.Details.Add(new CacheItemDetailElement {
                Region = "someRegion",
                Pattern = "{region}//{key}",
                Provider = typeof(RedisCacheProvider).FullName,
                FormatNullRegion = false,
            });


            var resolver = new ConfigurationResolver();
            resolver.ExeConfigFilename = "test.config";
            resolver.Save(section1, "cacheBuilder");

            var section2 = resolver.Read<CacheConfigurationSection>("cacheBuilder");
            Assert.AreEqual(section2.Pattern, section1.Pattern);
            Assert.AreEqual(section2.FormatNullRegion, section1.FormatNullRegion);
            Assert.AreEqual(section2.MaxExpirationHour, section1.MaxExpirationHour);
            Assert.IsNotNull(section2.Details);
            Assert.AreEqual(section2.Details.Count, section1.Details.Count);
            Assert.AreEqual(
                section2.Details.Get(typeof(RedisCacheProvider).FullName),
                section1.Details.Get(typeof(RedisCacheProvider).FullName)
            );
        }

        [TestMethod]
        public void Set_section_no_detail_then_read() {
            var section = new CacheConfigurationSection {
                Pattern = "{region}/{key}",
                FormatNullRegion = true,
                MaxExpirationHour = 1D,
            };

            //with region
            {
                var config = CacheConfigBuilder.Build(typeof(MemoryCacheProvider), "anyRegion", section);
                Assert.AreNotEqual(config, CacheConfig.Empty);

                Assert.AreEqual(config.Pattern, "{0}/{1}");
                Assert.AreEqual(config.FormatNullRegion, section.FormatNullRegion);
                Assert.AreEqual(config.MaxExpiration.Value.TotalHours, section.MaxExpirationHour);
            }

            //without region
            {
                var config = CacheConfigBuilder.Build(typeof(RedisCacheProvider), null, section);
                Assert.AreNotEqual(config, CacheConfig.Empty);

                Assert.AreEqual(config.Pattern, "{0}/{1}");
                Assert.AreEqual(config.FormatNullRegion, section.FormatNullRegion);
                Assert.AreEqual(config.MaxExpiration.Value.TotalHours, section.MaxExpirationHour);
            }
        }


        [TestMethod]
        public void Set_section_with_detail_then_read() {
            var section = new CacheConfigurationSection {
                Pattern = "{region}/{key}",
                FormatNullRegion = true,
                MaxExpirationHour = 1D,
                Details = new CacheItemElementCollection()
            };
            var detail = new CacheItemDetailElement {
                Region = "someRegion",
                Pattern = "{region}//{key}",
                Provider = typeof(RedisCacheProvider).FullName,
                MaxExpirationHour = 2D,
                FormatNullRegion = false,
            };
            section.Details.Add(detail);

            //with region
            {
                var config = CacheConfigBuilder.Build(typeof(MemoryCacheProvider), "otherRegion", section);
                Assert.AreNotEqual(config, CacheConfig.Empty);

                Assert.AreEqual(config.Pattern, "{0}/{1}");
                Assert.AreEqual(config.FormatNullRegion, section.FormatNullRegion);
                Assert.AreEqual(config.MaxExpiration.Value.TotalHours, section.MaxExpirationHour);
            }

            //without region
            {
                var config = CacheConfigBuilder.Build(typeof(RedisCacheProvider), null, section);
                Assert.AreNotEqual(config, CacheConfig.Empty);

                Assert.AreEqual(config.Pattern, "{0}/{1}");
                Assert.AreEqual(config.FormatNullRegion, section.FormatNullRegion);
                Assert.AreEqual(config.MaxExpiration.Value.TotalHours, section.MaxExpirationHour);
            }

            //with other region
            {
                var config = CacheConfigBuilder.Build(typeof(RedisCacheProvider), "otherRegion", section);
                Assert.AreNotEqual(config, CacheConfig.Empty);

                Assert.AreEqual(config.Pattern, "{0}/{1}");
                Assert.AreEqual(config.FormatNullRegion, section.FormatNullRegion);
                Assert.AreEqual(config.MaxExpiration.Value.TotalHours, section.MaxExpirationHour);
            }

            //with correct region
            {
                var config = CacheConfigBuilder.Build(typeof(RedisCacheProvider), "someRegion", section);
                Assert.AreNotEqual(config, CacheConfig.Empty);

                Assert.AreEqual(config.Pattern, "{0}//{1}");
                Assert.AreEqual(config.FormatNullRegion, detail.FormatNullRegion);
                Assert.AreEqual(config.MaxExpiration.Value.TotalHours, detail.MaxExpirationHour);
            }
        }
    }
}
