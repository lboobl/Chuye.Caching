using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Enyim.Caching;
using Enyim.Caching.Memcached;

namespace Chuye.Caching.Tests {
    [TestClass]
    public class MemcachedTest {
        [TestMethod]
        public void Online() {
            using (MemcachedClient client = new MemcachedClient("enyim.com/memcached")) {
                String key = Guid.NewGuid().ToString("n");
                //key = "7864189e85c4478b9b8946db8b26c922";
                //var r = client.Get(key);
                //client.Store(StoreMode.Set, key, new Person {
                //    Id = 2,
                //    Name = "Rattz",
                //    Address = new Address {
                //        Line1 = "Haidin Shuzhoujie",
                //        Line2 = "Beijing China"
                //    }
                //}); 
                //return;

                Object value = client.Get(key);
                Assert.IsNull(value);

                var exist = client.TryGet(key, out value);
                Assert.IsFalse(exist);
                Assert.IsNull(value);

                value = new Person {
                    Id = 2,
                    Name = "Rattz",
                    Address = new Address {
                        Line1 = "Haidin Shuzhoujie",
                        Line2 = "Beijing China"
                    }
                };
                client.Store(StoreMode.Set, key, value);
                exist = client.TryGet(key, out value);
                Assert.IsTrue(exist);
                Assert.IsNotNull(value);
            }
        }

        [TestMethod]
        public void NullCache() {
            using (MemcachedClient client = new MemcachedClient("enyim.com/memcached")) {
                String key = Guid.NewGuid().ToString("n");
                Object value = null;
                client.Store(StoreMode.Set, key, value);
                var exist = client.TryGet(key, out value);
                Assert.IsTrue(exist);
                Assert.IsNull(value);
            }
        }
    }

    [Serializable]
    public class Person {
        public int Id { get; set; }
        public String Name { get; set; }
        public Address Address { get; set; }
    }

    [Serializable]
    public class Address {
        public String Line1 { get; set; }
        public String Line2 { get; set; }
    }
}

