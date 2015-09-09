using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Chuye.Caching.Redis;

namespace Chuye.Caching.Tests {
    [TestClass]
    public class RedisTest {

        [TestMethod]
        public void RedisField_Equal_Test() {
            RedisField f1 = new RedisField();
            RedisField f2 = new RedisField();
            Assert.IsTrue(f1 == f2);
            Assert.IsTrue(f1.Equals(f2));

            String str = Guid.NewGuid().ToString();
            f1 = str;
            f2 = str;
            Assert.IsTrue(f1 == f2);
            Assert.IsTrue(f1.Equals(f2));

            Object f3 = f1;
            Assert.IsTrue(f3.Equals(f2));
            Assert.IsTrue(f2.Equals(f3));

            f2 = Guid.NewGuid().ToString();
            Assert.IsTrue(f1 != f2);
            Assert.IsTrue(!f1.Equals(f2));

            Assert.IsTrue(!f3.Equals(f2));
            Assert.IsTrue(!f2.Equals(f3));
        }

        [TestMethod]
        public void RedisEntry_Equal_Test() {
            var e1 = new RedisEntry();
            var e2 = new RedisEntry();
            Assert.IsTrue(e1 == e2);
            Assert.IsTrue(e1.Equals(e2));

            KeyValuePair<RedisField, RedisField> e3 = e2;
            Assert.IsTrue(e1 == e2);
            Assert.IsTrue(e1.Equals(e2));

            Object e4 = e2;
            Assert.IsTrue(e1.Equals(e4));
            Assert.IsTrue(e4.Equals(e1));

            var e5 = new RedisEntry(Guid.NewGuid().ToString(), e2.Value);
            Assert.IsTrue(e1 != e5);
            Assert.IsTrue(!e1.Equals(e5));
        }

        [TestMethod]
        public void StringTest() {
            IRedis redis = new ServiceStackRedis();
            var cacheKey = Guid.NewGuid().ToString();
            var cacheField = redis.StringGet(cacheKey);
            Assert.IsFalse(cacheField.HasValue);
            Assert.AreEqual((String)cacheField, null);

            var cacheValue = Guid.NewGuid().ToString();
            redis.StringSet(cacheKey, cacheValue);

            cacheField = redis.StringGet(cacheKey);
            Assert.IsTrue(cacheField.HasValue);
            Assert.AreEqual((String)cacheField, cacheValue);
        }
    }
}
