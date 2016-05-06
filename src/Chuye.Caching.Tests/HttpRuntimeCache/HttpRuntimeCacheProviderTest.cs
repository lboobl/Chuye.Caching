using System;
using System.IO;
using System.Threading;
using System.Web.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chuye.Caching.Tests.HttpRuntimeCache {
    [TestClass]
    public class HttpRuntimeCacheProviderTest {
        [TestMethod]
        public void Save_ValueType_then_get() {
            var key = "key-guid";
            ICacheProvider cache = new HttpRuntimeCacheProvider();
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
            ICacheProvider cache = new HttpRuntimeCacheProvider();
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
            ICacheProvider cache = new HttpRuntimeCacheProvider();
            Object id1 = null;
            var id2 = cache.GetOrCreate(key, _ => id1);
            Assert.IsNull(id2);

            Object id3;
            var exists = cache.TryGet(key, out id3);
            Assert.IsFalse(exists);
        }
        
        [TestMethod]
        public void GetOrCreateWithslidingExpiration() {
            var key = Guid.NewGuid().ToString();
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new HttpRuntimeCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, _ => val, TimeSpan.FromSeconds(1.5D));
            Assert.AreEqual(result, val);
            {
                Thread.Sleep(1000);
                var exist = cacheProvider.TryGet<Guid>(key, out val);
                Assert.IsTrue(exist);
                Assert.AreEqual(result, val);
            }
            {
                Thread.Sleep(1000);
                var exist = cacheProvider.TryGet<Guid>(key, out val);
                Assert.IsTrue(exist);
                Assert.AreEqual(result, val);
            }
            {
                Thread.Sleep(2000);
                var exist = cacheProvider.TryGet<Guid>(key, out val);
                Assert.IsFalse(exist);
                Assert.AreEqual(val, Guid.Empty);
            }
        }

        [TestMethod]
        public void GetOrCreateWithAbsoluteExpiration() {
            var key = Guid.NewGuid().ToString();
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new HttpRuntimeCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, _ => val, DateTime.UtcNow.AddSeconds(2D));
            Assert.AreEqual(result, val);

            var exist = cacheProvider.TryGet<Guid>(key, out val);
            Assert.IsTrue(exist);
            Assert.AreEqual(result, val);

            Thread.Sleep(2000);
            exist = cacheProvider.TryGet<Guid>(key, out val);
            Assert.IsFalse(exist);
            Assert.AreEqual(val, Guid.Empty);
        }

        [TestMethod]
        public void OverwriteWithslidingExpiration() {
            var key = Guid.NewGuid().ToString();
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new HttpRuntimeCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, _ => val);
            Assert.AreEqual(result, val);

            var val2 = Guid.NewGuid();
            cacheProvider.Overwrite<Guid>(key, val2, TimeSpan.FromSeconds(1D));

            Thread.Sleep(2000);
            Guid val3;
            var exist = cacheProvider.TryGet<Guid>(key, out val3);
            Assert.IsFalse(exist);
            Assert.AreEqual(val3, Guid.Empty);

        }

        [TestMethod]
        public void OverwriteWithAbsoluteExpiration() {
            var key = Guid.NewGuid().ToString();
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new HttpRuntimeCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, _ => val);
            Assert.AreEqual(result, val);

            var val2 = Guid.NewGuid();
            cacheProvider.Overwrite<Guid>(key, val2, DateTime.UtcNow.AddSeconds(1D));

            Thread.Sleep(2000);
            Guid val3;
            var exist = cacheProvider.TryGet<Guid>(key, out val3);
            Assert.IsFalse(exist);
            Assert.AreEqual(val3, Guid.Empty);

        }

        [TestMethod]
        public void Expire() {
            var key = Guid.NewGuid().ToString();
            var val = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new HttpRuntimeCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, _ => val);
            Assert.AreEqual(result, val);

            cacheProvider.Expire(key);
            Guid val2;
            var exist = cacheProvider.TryGet<Guid>(key, out val2);
            Assert.IsFalse(exist);
            Assert.AreEqual(val2, Guid.Empty);
        }

        [TestMethod]
        public void ExpireAll() {
            var key = Guid.NewGuid().ToString();
            var val = Guid.NewGuid();

            HttpRuntimeCacheProvider cacheProvider = new HttpRuntimeCacheProvider();
            var result = cacheProvider.GetOrCreate<Guid>(key, _ => val);
            Assert.AreEqual(result, val);
            Assert.IsTrue(cacheProvider.Count() > 0);


            cacheProvider.ExpireAll();
            Guid val2;
            var exist = cacheProvider.TryGet<Guid>(key, out val2);
            Assert.IsFalse(exist);
            Assert.AreEqual(val2, Guid.Empty);

            Assert.IsTrue(cacheProvider.Count() == 0);
        }


        [TestMethod]
        public void Callback() {
            var key = Guid.NewGuid().ToString();
            var val = Guid.NewGuid();

            HttpRuntimeCacheProvider cacheProvider = new HttpRuntimeCacheProvider();
            var expireCallback = new CacheItemUpdateCallback(Callback);
            cacheProvider.Overwrite(key, val, DateTime.Now.AddSeconds(4D), expireCallback);
            Thread.Sleep(5000);
        }

        private void Callback(string key, CacheItemUpdateReason reason, out object expensiveObject, out CacheDependency dependency, out DateTime absoluteExpiration, out TimeSpan slidingExpiration) {
            expensiveObject = null;
            dependency = null;
            absoluteExpiration = Cache.NoAbsoluteExpiration;
            slidingExpiration = Cache.NoSlidingExpiration;
            Console.WriteLine("{0} key expired", key);
        }

        [TestMethod]
        public void Flush() {
            var cacheProvider = new HttpRuntimeCacheProvider();
            cacheProvider.Overwrite("id", 21685);
            cacheProvider.Overwrite("begin", DateTime.Now);
            var file1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache1.db");
            cacheProvider.Flush(file1, _ => true);

            cacheProvider = new HttpRuntimeCacheProvider("User");
            cacheProvider.Overwrite("13", new User { Id = 13, Name = "Rattz", Age = 20, Address = new[] { "Beijing", "Wuhan" } });
            cacheProvider.Overwrite("14", new User { Id = 14, Name = "Kate", Age = 18, Address = new[] { "Tokyo", "Los Angeles" } });
            var file2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache2.db");
            cacheProvider.Flush(file2, _ => true);

            cacheProvider = new HttpRuntimeCacheProvider("Job");
            cacheProvider.Overwrite("52", new { Id = 52, Title = "Software Engineer", Salary = 10000 });
            cacheProvider.Overwrite("100", new { Id = 100, Title = "Gwhilsttroenterologist", Salary = 12000 });
            var file3 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache3.db");
            cacheProvider.Flush(file3, _ => true);
        }

        class User {
            public Int32 Id { get; set; }
            public String Name { get; set; }
            public Int32 Age { get; set; }
            public String[] Address { get; set; }
        }
    }
}
