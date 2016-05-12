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

            Person id1 = null;
            var id2 = cache.GetOrCreate(key, _ => id1);
            Assert.IsNull(id2);

            Person id3;
            var exists = cache.TryGet(key, out id3);
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void Set_with_slidingExpiration_then_get() {
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new HttpRuntimeCacheProvider();
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
                Assert.IsTrue(exist);
                Assert.AreEqual(value2, value);
            }
            {
                Guid value2;
                Thread.Sleep(4000);
                var exist = cacheProvider.TryGet(key, out value2);
                Assert.IsFalse(exist);
            }
        }

        [TestMethod]
        public void Set_with_absoluteExpiration_then_get() {
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid();

            IHttpRuntimeCacheProvider cacheProvider = new HttpRuntimeCacheProvider();
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

            var cacheProvider = new HttpRuntimeCacheProvider();
            cacheProvider.Overwrite(key, value);

            cacheProvider.Expire(key);
            Guid value2;
            var exist = cacheProvider.TryGet(key, out value2);
            Assert.IsFalse(exist);
            Assert.AreEqual(value2, Guid.Empty);

            cacheProvider.ExpireAll();
            Assert.AreEqual(cacheProvider.Count(), 0);
        }
        
        [TestMethod]
        public void Set_then_flush() {
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
