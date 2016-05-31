﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Chuye.Caching.Redis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chuye.Caching.Tests.Redis {
    [TestClass]
    public class RedisCacheProviderRegionTest {
        [TestMethod]
        public void Save_ValueType_then_get() {
            var key = "key-guid";
            ICacheProvider cache = new RedisCacheProvider(StackExchangeRedis.Default, "region1");
            var id1 = Guid.NewGuid();
            var id2 = cache.GetOrCreate(key, _ => id1);
            Assert.AreEqual(id1, id2);

            cache.Expire(key);
            Guid id3;
            var exists = cache.TryGet(key, out id3);
            Assert.IsFalse(exists);
            Assert.AreNotEqual(id1, id3);
            Assert.AreEqual(id3, Guid.Empty);
        }

        [TestMethod]
        public void Save_ReferenceType_then_get() {
            var key = "key-object";
            ICacheProvider cache = new RedisCacheProvider(StackExchangeRedis.Default, "region2");
            var id1 = new Object();
            var id2 = cache.GetOrCreate(key, _ => id1);
            Assert.AreEqual(id1, id2);

            cache.Expire(key);
            Object id3;
            var exists = cache.TryGet(key, out id3);
            Assert.IsFalse(exists);
            Assert.AreNotEqual(id1, id3);
            Assert.AreEqual(id3, null);
        }

        [TestMethod]
        public void Save_null_then_get() {
            var key = "key-object-null";
            ICacheProvider cache = new RedisCacheProvider(StackExchangeRedis.Default, "region3");

            cache.Overwrite(key, (Person)null);
            Person id1;
            var exists = cache.TryGet(key, out id1);
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void Set_with_slidingExpiration_then_get() {
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new RedisCacheProvider(StackExchangeRedis.Default, "region4");
            cacheProvider.Overwrite(key, value, TimeSpan.FromSeconds(3D));

            {
                Guid value2;
                Thread.Sleep(2000);
                var exist = cacheProvider.TryGet<Guid>(key, out value2);
                Assert.IsTrue(exist);
                Assert.AreEqual(value2, value);
            }
            {
                Guid value2;
                Thread.Sleep(2000);
                var exist = cacheProvider.TryGet(key, out value2);
                Assert.IsFalse(exist);
            }
        }

        [TestMethod]
        public void Set_with_absoluteExpiration_then_get() {
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new RedisCacheProvider(StackExchangeRedis.Default, "region5");
            cacheProvider.Overwrite(key, value, DateTime.Now.AddSeconds(3D));

            {
                Guid value2;
                Thread.Sleep(2000);
                var exist = cacheProvider.TryGet<Guid>(key, out value2);
                Assert.IsTrue(exist);
                Assert.AreEqual(value2, value);
            }
            {
                Guid value2;
                Thread.Sleep(2000);
                var exist = cacheProvider.TryGet(key, out value2);
                Assert.IsFalse(exist);
            }
        }

        [TestMethod]
        public void Set_then_expire() {
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new RedisCacheProvider(StackExchangeRedis.Default, "region6");
            cacheProvider.Overwrite(key, value);

            cacheProvider.Expire(key);
            Guid value2;
            var exist = cacheProvider.TryGet(key, out value2);
            Assert.IsFalse(exist);
            Assert.AreEqual(value2, Guid.Empty);
        }

        [TestMethod]
        public void Lock_then_modify_list() {
            IDistributedLock memcached = new RedisCacheProvider(StackExchangeRedis.Default, "region7");
            var key = "DistributedLock1";

            {
                var list = new List<int>();
                var except = new Random().Next(1000, 2000);
                var stopwatch = Stopwatch.StartNew();

                Parallel.For(0, except, i => {
                    using (memcached.ReleasableLock(key)) {
                        list.Add(i);
                    }
                });
                stopwatch.Stop();
                Console.WriteLine("Handle {0} times cost {1}, {2:f2} per sec.",
                    except, stopwatch.Elapsed.TotalSeconds, except / stopwatch.Elapsed.TotalSeconds);

                Assert.AreEqual(list.Count, except);
            }

            {
                var list = new List<int>();
                var except = new Random().Next(1000, 2000);
                var stopwatch = Stopwatch.StartNew();

                Parallel.For(0, except, i => {
                    memcached.ReleasableLock(key);
                    list.Add(i);
                    memcached.UnLock(key);
                });

                stopwatch.Stop();
                Console.WriteLine("Handle {0} times cost {1}, {2:f2} per sec.",
                    except, stopwatch.Elapsed.TotalSeconds, except / stopwatch.Elapsed.TotalSeconds);

                Assert.AreEqual(list.Count, except);
            }
        }
    }
}
