using System;
using System.IO;
using System.Threading;
using System.Web.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chuye.Caching.Tests.Memory {
    [TestClass]
    public class MemoryCacheProviderRegionTest {
        [TestMethod]
        public void Save_ValueType_then_get() {
            var key = "key-guid";
            IBasicCacheProvider cache = new MemoryCacheProvider("region1");
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
            IBasicCacheProvider cache = new MemoryCacheProvider("region2");
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
            IBasicCacheProvider cache = new MemoryCacheProvider("region3");

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

            ICacheProvider cache = new MemoryCacheProvider("region4");
            cache.Overwrite(key, value, TimeSpan.FromSeconds(3D));

            {
                Guid value2;
                Thread.Sleep(2000);
                var exist = cache.TryGet<Guid>(key, out value2);
                Assert.IsTrue(exist);
                Assert.AreEqual(value2, value);
            }
            {
                Guid value2;
                Thread.Sleep(2000);
                var exist = cache.TryGet(key, out value2);
                Assert.IsTrue(exist);
                Assert.AreEqual(value2, value);
            }
            {
                Guid value2;
                Thread.Sleep(4000);
                var exist = cache.TryGet(key, out value2);
                Assert.IsFalse(exist);
            }
        }

        [TestMethod]
        public void Set_with_absoluteExpiration_then_get() {
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid();

            ICacheProvider cache = new MemoryCacheProvider("region5");
            cache.Overwrite(key, value, DateTime.Now.AddSeconds(3D));

            {
                Guid value2;
                Thread.Sleep(2000);
                var exist = cache.TryGet<Guid>(key, out value2);
                Assert.IsTrue(exist);
                Assert.AreEqual(value2, value);
            }
            {
                Guid value2;
                Thread.Sleep(2000);
                var exist = cache.TryGet(key, out value2);
                Assert.IsFalse(exist);
            }
        }

        [TestMethod]
        public void Set_then_expire() {
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid();

            var cache = new MemoryCacheProvider("region6");
            cache.Overwrite(key, value);

            cache.Expire(key);
            Guid value2;
            var exist = cache.TryGet(key, out value2);
            Assert.IsFalse(exist);
            Assert.AreEqual(value2, Guid.Empty);

            cache.ExpireAll();
            Assert.AreEqual(cache.Count(), 0);
        }
        
        [TestMethod]
        public void Set_then_flush() {
            var cache = new MemoryCacheProvider("region7");
            cache.Overwrite("id", 21685);
            cache.Overwrite("begin", DateTime.Now);
            var file1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache1.db");
            cache.Flush(file1, _ => true);

            cache = new MemoryCacheProvider("User");
            cache.Overwrite("13", new User { Id = 13, Name = "Rattz", Age = 20, Address = new[] { "Beijing", "Wuhan" } });
            cache.Overwrite("14", new User { Id = 14, Name = "Kate", Age = 18, Address = new[] { "Tokyo", "Los Angeles" } });
            var file2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache2.db");
            cache.Flush(file2, _ => true);

            cache = new MemoryCacheProvider("Job");
            cache.Overwrite("52", new { Id = 52, Title = "Software Engineer", Salary = 10000 });
            cache.Overwrite("100", new { Id = 100, Title = "Gwhilsttroenterologist", Salary = 12000 });
            var file3 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache3.db");
            cache.Flush(file3, _ => true);
        }

        class User {
            public Int32 Id { get; set; }
            public String Name { get; set; }
            public Int32 Age { get; set; }
            public String[] Address { get; set; }
        }
    }
}
