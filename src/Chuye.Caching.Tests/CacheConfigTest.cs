using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chuye.Caching.Tests {
    [TestClass]
    public class CacheConfigTest {

        [TestMethod]
        public void Default_config_build_key_then_valid() {
            var config = new CacheConfig();
            var key1 = config.BuildCacheKey("r", "k");
            Assert.AreEqual(key1, "r-k");

            var key2 = config.BuildCacheKey(null, "k");
            Assert.AreEqual(key2, "k");

            var key3 = config.BuildCacheKey(String.Empty, "k");
            Assert.AreEqual(key3, "k");
        }

        [TestMethod]
        public void Custom_config_build_key_then_valid() {
            var config = new CacheConfig(pattern: "{key}:{region}");
            var key = config.BuildCacheKey("r", "k");
            Assert.AreEqual(key, "k:r");
        }

        [TestMethod]
        public void Custom_config_with_invalid_argument() {
            try {
                var config = new CacheConfig(pattern: "{key}");
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException) {
            }

            try {
                var config = new CacheConfig(pattern: "{0}-{1}");
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException) {
            }
        }
    }
}
