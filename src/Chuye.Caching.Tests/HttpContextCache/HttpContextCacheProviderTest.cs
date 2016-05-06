using System;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chuye.Caching.Tests.HttpContextCache {
    [TestClass]
    public class HttpContextCacheProviderTest {
        [TestMethod]
        public void Save_null_with_HttpContext() {
            HttpContext.Current = new HttpContext(new HttpRequest(null, "http://localhost", null), new HttpResponse(null));
            var key = "key-null";
            HttpContext.Current.Items.Add(key, null);
            Assert.IsTrue(HttpContext.Current.Items.Contains(key));
            Assert.IsNull(HttpContext.Current.Items[key]);
        }

        [TestMethod]
        public void Save_ValueType_then_get() {
            var key = "key-guid";
            ICacheProvider cache = new HttpContextCacheProvider();
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
            ICacheProvider cache = new HttpContextCacheProvider();
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
            ICacheProvider cache = new HttpContextCacheProvider();
            Object id1 = null;
            var id2 = cache.GetOrCreate(key, _ => id1);
            Assert.IsNull(id2);

            Object id3;
            var exists = cache.TryGet(key, out id3);
            Assert.IsFalse(exists);
        }
    }
}
