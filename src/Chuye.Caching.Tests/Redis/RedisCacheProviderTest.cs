﻿using System;
using System.Threading;
using Chuye.Caching.Redis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chuye.Caching.Tests.Redis {
    [TestClass]
    public class RedisCacheProviderTest {
        [TestMethod]
        public void GetOrCreateTest() {
            var key = Guid.NewGuid().ToString("n");
            var val = Guid.NewGuid();
            
            IHttpRuntimeCacheProvider cacheProvider = new RedisCacheProvider(StackExchangeRedis.Default);
            var result = cacheProvider.GetOrCreate<Guid>(key, _ => val);
            Assert.AreEqual(result, val);

            {
                var exist = cacheProvider.TryGet<Guid>(key, out val);
                Assert.IsTrue(exist);
                Assert.AreEqual(result, val);
            }

            {
                var result2 = cacheProvider.GetOrCreate<Guid>(key, _ => {
                    Assert.Fail();
                    return Guid.NewGuid();
                });
                Assert.AreEqual(result2, val);
            }
        }

        [TestMethod]
        public void OverwriteTest() {
            var key = Guid.NewGuid().ToString("n");
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new RedisCacheProvider(StackExchangeRedis.Default);
            var result = cacheProvider.GetOrCreate<Guid>(key, _ => val);
            Assert.AreEqual(result, val);

            var val2 = Guid.NewGuid();
            cacheProvider.Overwrite<Guid>(key, val2);

            Guid val3;
            var exist = cacheProvider.TryGet<Guid>(key, out val3);
            Assert.IsTrue(exist);
            Assert.AreEqual(val3, val2);
        }

        [TestMethod]
        public void OverwriteWithslidingExpirationTest() {
            var key = Guid.NewGuid().ToString("n");
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new RedisCacheProvider(StackExchangeRedis.Default);

            //DateTime.Now
            Guid result;
            cacheProvider.Overwrite(key, val, TimeSpan.FromSeconds(8D));
            {
                Thread.Sleep(TimeSpan.FromSeconds(5D));
                var exist = cacheProvider.TryGet<Guid>(key, out result);
                Assert.IsTrue(exist);
                Assert.AreEqual(result, val);
            }
            {
                Thread.Sleep(TimeSpan.FromSeconds(5D));
                var exist = cacheProvider.TryGet<Guid>(key, out result);
                Assert.IsFalse(exist);
            }
        }

        [TestMethod]
        public void OverwriteWithAbsoluteExpirationTest() {
            var key = Guid.NewGuid().ToString("n");
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new RedisCacheProvider(StackExchangeRedis.Default);

            //DateTime.Now
            Guid result;
            cacheProvider.Overwrite(key, val, DateTime.Now.AddSeconds(8D));
            {
                Thread.Sleep(TimeSpan.FromSeconds(5D));
                var exist = cacheProvider.TryGet<Guid>(key, out result);
                Assert.IsTrue(exist);
                Assert.AreEqual(result, val);
            }
            {
                Thread.Sleep(TimeSpan.FromSeconds(5D));
                var exist = cacheProvider.TryGet<Guid>(key, out result);
                Assert.IsFalse(exist);
            }

            //DateTime.UtcNow
            cacheProvider.Overwrite(key, val, DateTime.UtcNow.AddSeconds(8D));
            {
                Thread.Sleep(TimeSpan.FromSeconds(5D));
                var exist = cacheProvider.TryGet<Guid>(key, out result);
                Assert.IsTrue(exist);
                Assert.AreEqual(result, val);
            }
            {
                Thread.Sleep(TimeSpan.FromSeconds(5D));
                var exist = cacheProvider.TryGet<Guid>(key, out result);
                Assert.IsFalse(exist);
            }
        }

        [TestMethod]
        public void ExpireTest() {
            var key = Guid.NewGuid().ToString("n");
            var val = Guid.NewGuid();
            var redis = StackExchangeRedis.Default;

            IHttpRuntimeCacheProvider cacheProvider = new RedisCacheProvider(redis);

            {
                var result = cacheProvider.GetOrCreate<Guid>(key, _ => val);
                Assert.AreEqual(result, val);

                var exist = cacheProvider.TryGet<Guid>(key, out val);
                Assert.IsTrue(exist);

                cacheProvider.Expire(key);
                Guid val2;
                exist = cacheProvider.TryGet<Guid>(key, out val2);
                Assert.IsFalse(exist);
                Assert.AreEqual(val2, Guid.Empty);
            }


            {
                var result = cacheProvider.GetOrCreate<Guid>(key, _ => val);
                Assert.AreEqual(result, val);

                var exist = cacheProvider.TryGet<Guid>(key, out val);
                Assert.IsTrue(exist);

                cacheProvider.Expire(key);
                Guid val2;
                exist = cacheProvider.TryGet<Guid>(key, out val2);
                Assert.IsFalse(exist);
                Assert.AreEqual(val2, Guid.Empty);
            }
        }
    }
}
