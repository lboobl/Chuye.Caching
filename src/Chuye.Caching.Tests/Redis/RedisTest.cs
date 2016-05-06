using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Chuye.Caching.Redis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StackExchange.Redis;

namespace Chuye.Caching.Tests.Redis {
    [TestClass]
    public class RedisTest {

        [TestMethod]
        public void RedisKey_Equal_Test() {
            //StackExchange.Redis.IDatabase d;
            //d.SortedSetRangeByRankWithScores;
            //d.SortedSetRangeByScoreWithScores;

            RedisKey f1 = new RedisKey();
            RedisKey f2 = new RedisKey();
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
        public void RedisValue_Equal_Test() {
            var e1 = new RedisValue();
            var e2 = new RedisValue();
            Assert.IsTrue(e1 == e2);
            Assert.IsTrue(e1.Equals(e2));

            Object e4 = e2;
            Assert.IsTrue(e1.Equals(e4));
            Assert.IsTrue(e4.Equals(e1));

            RedisValue e5 = Guid.NewGuid().ToString();
            Assert.IsTrue(e1 != e5);
            Assert.IsTrue(!e1.Equals(e5));
        }

        [TestMethod]
        public void KeyRename() {
            var redis = StackExchangeRedis.Default;
            var db = redis.GetDatabase();
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid().ToString();

            db.StringSet(key, value);
            Assert.IsTrue(db.KeyExists(key));

            var key2= Guid.NewGuid().ToString();
            var renamed1 = db.KeyRename(key, key2);
            Assert.IsTrue(renamed1);

            Assert.IsFalse(db.KeyExists(key));
            Assert.IsTrue(db.KeyExists(key2));

            db.KeyDelete(key2);
            try {
                var renamed12 = db.KeyRename(key, key2);
                Assert.Fail();

            }
            catch(Exception ex) {
                Assert.IsTrue(ex is RedisException);
            }
        }

        [TestMethod]
        public void StringTest() {
            var cacheKey = Guid.NewGuid().ToString();
            var redis = StackExchangeRedis.Default;
            var db = redis.GetDatabase();

            //StringGet
            var cacheField = db.StringGet(cacheKey);
            Assert.IsFalse(cacheField.HasValue);
            Assert.AreEqual((String)cacheField, null);

            //StringSet
            var cacheValue = Guid.NewGuid().ToString();
            db.StringSet(cacheKey, cacheValue);

            //StringGet again
            cacheField = db.StringGet(cacheKey);
            Assert.IsTrue(cacheField.HasValue);
            Assert.AreEqual((String)cacheField, cacheValue);

            //KeyDelete
            db.KeyDelete(cacheKey);
        }

        [TestMethod]
        public void ListTest() {
            var cacheKey = Guid.NewGuid().ToString();
            var redis = StackExchangeRedis.Default;
            var db = redis.GetDatabase();
            var linkList = new LinkedList<String>();
            const Int32 listLength = 4;

            Action init = () => {
                db.KeyDelete(cacheKey);
                linkList.Clear();

                for (int i = 0; i < listLength; i++) {
                    var cacheValue = Guid.NewGuid().ToString();

                    if ((Guid.NewGuid().GetHashCode() & 1) == 0) {
                        linkList.AddFirst(cacheValue);
                        //ListLeftPush
                        db.ListLeftPush(cacheKey, linkList.First.Value);
                    }
                    else {
                        linkList.AddLast(cacheValue);
                        //ListLeftPush
                        db.ListRightPush(cacheKey, linkList.Last.Value);
                    }
                }
            };

            init();
            Assert.AreEqual(linkList.Count, db.ListLength(cacheKey));


            for (int i = 0; i < listLength; i++) {
                RedisValue cacheItem;
                if ((Guid.NewGuid().GetHashCode() & 1) == 0) {
                    cacheItem = db.ListLeftPop(cacheKey);
                    Assert.AreEqual(linkList.First.Value, (String)cacheItem);
                    linkList.RemoveFirst();
                }
                else {
                    cacheItem = db.ListRightPop(cacheKey);
                    Assert.AreEqual(linkList.Last.Value, (String)cacheItem);
                    linkList.RemoveLast();
                }

                Assert.AreEqual(linkList.Count, db.ListLength(cacheKey));
            }

            var cacheEists = db.KeyExists(cacheKey);
            Assert.IsFalse(cacheEists);
        }

        [TestMethod]
        public void ListMultiTest() {
            var cacheKey = Guid.NewGuid().ToString();
            var redis = StackExchangeRedis.Default;
            var db = redis.GetDatabase();
            var linkList = new List<String>();
            var listLength = Math.Abs(Guid.NewGuid().GetHashCode() % 5) + 5;

            db.KeyDelete(cacheKey);
            linkList = Enumerable.Repeat(0, listLength)
                .Select(x => Guid.NewGuid().ToString())
                .ToList();

            {
                db.ListRightPush(cacheKey, linkList.Select(x => (RedisValue)x).ToArray());
                Assert.AreEqual(linkList.Count, db.ListLength(cacheKey));

                for (int i = 0; i < listLength; i++) {
                    var cacheItem = db.ListLeftPop(cacheKey);
                    Assert.AreEqual(linkList[i], (String)cacheItem);
                }

                var cacheEists = db.KeyExists(cacheKey);
                Assert.IsFalse(cacheEists);
            }

            {
                db.ListLeftPush(cacheKey, linkList.Select(x => (RedisValue)x).ToArray());
                Assert.AreEqual(linkList.Count, db.ListLength(cacheKey));

                for (int i = 0; i < listLength; i++) {
                    var cacheItem = db.ListRightPop(cacheKey);
                    Assert.AreEqual(linkList[i], (String)cacheItem);
                }

                var cacheEists = db.KeyExists(cacheKey);
                Assert.IsFalse(cacheEists);
            }
        }

        [TestMethod]
        public void HashTest() {
            var cacheKey = Guid.NewGuid().ToString();
            var redis = StackExchangeRedis.Default;
            var db = redis.GetDatabase();

            var count = 10;
            var names = new String[count].ToList();
            var values = new String[count];

            for (int i = 0; i < count; i++) {
                names[i] = Guid.NewGuid().ToString();
                values[i] = Guid.NewGuid().ToString();
            }
            var list = Enumerable.Range(0, count)
                .Select(i => new HashEntry(names[i], values[i]))
                .ToArray();

            db.HashSet(cacheKey, list);
            Assert.AreEqual(db.HashLength(cacheKey), count);

            var array = db.HashGet(cacheKey, names.Select(x => (RedisValue)x).ToArray());
            for (int i = 0; i < count; i++) {
                Assert.IsTrue(array[i] == values[i]);
            }

            var hash = db.HashGetAll(cacheKey);
            Assert.AreEqual(hash.Length, count);
            for (int i = 0; i < count; i++) {
                Assert.IsTrue(hash[i].Name == names[i]);
                Assert.IsTrue(hash[i].Value == values[i]);
            }

            for (int i = 0; i < count; i++) {
                var cacheItem = db.HashGet(cacheKey, names[i]);
                Assert.IsTrue((String)cacheItem == values[i]);
            }

            for (int i = 0; i < count; i++) {
                var deleted = db.HashDelete(cacheKey, names[i]);
                Assert.IsTrue(deleted);
            }

            var exist = db.KeyExists(cacheKey);
            Assert.IsFalse(exist);
        }

        [TestMethod]
        public void SortedSetRange() {
            var cacheKey = Guid.NewGuid().ToString();
            var redis = StackExchangeRedis.Default;
            var db = redis.GetDatabase();

            var random = new Random();
            var list = Enumerable.Repeat(0, 20).Select(r => random.Next(100)).Distinct().ToList();

            for (int i = 0; i < list.Count; i++) {
                db.SortedSetAdd(cacheKey, list[i].ToString(), i);
                var len = db.SortedSetLength(cacheKey);
                Assert.AreEqual(len, i + 1);
            }

            var values = db.SortedSetRangeByRank(cacheKey, 0, -1);
            Assert.AreEqual(values.Length, list.Count);
            for (int i = 0; i < list.Count; i++) {
                Assert.AreEqual((String)values[i], list[i].ToString());
            }

            for (int i = 0; i < 3; i++) {
                var index1 = random.Next(list.Count);
                var index2 = db.SortedSetRank(cacheKey, list[index1].ToString());
                Assert.AreEqual(index1, index2);
            }

            for (int i = 0; i < 3; i++) {
                var index = random.Next(list.Count);
                var value = list[index];
                list.RemoveAt(index);
                var removed = db.SortedSetRemove(cacheKey, value.ToString());
                Assert.IsTrue(removed);
                var len = db.SortedSetLength(cacheKey);
                Assert.AreEqual(len, list.Count);
            }

            Assert.IsTrue(db.SortedSetLength(cacheKey) > 3);
            var removedByRank = db.SortedSetRemoveRangeByRank(cacheKey, 0, 2);
            Assert.AreEqual(removedByRank, 3);
        }

        [TestMethod]
        public void SortedSet_Ordered() {
            var cacheKey = Guid.NewGuid().ToString();
            var redis = StackExchangeRedis.Default;
            var db = redis.GetDatabase();

            var random = new Random();
            var list = Enumerable.Repeat(0, 4).Select(r => random.Next(100)).ToList();
            list.ForEach(i => db.SortedSetAdd(cacheKey, i.ToString(), (double)i));

            var list1 = db.SortedSetRangeByRank(cacheKey, order: Order.Ascending);
            Assert.AreEqual(list1.Length, list.Count);

            var array1 = list.ToArray();
            Array.Sort(array1);
            for (int i = 0; i < list1.Length; i++) {
                Assert.AreEqual((Int32)list1[i], array1[i]);
            }

            var list2 = db.SortedSetRangeByRank(cacheKey, order: Order.Descending);
            Assert.AreEqual(list2.Length, list.Count);

            var array2 = array1.Reverse().ToArray();
            for (int i = 0; i < list2.Length; i++) {
                Assert.AreEqual((Int32)list2[i], array2[i]);
            }

        }


        [TestMethod]
        public void StringIncrement() {
            //StackExchange.Redis.IDatabase d;
            var key = "RedisParallelTest";
            var redis = StackExchangeRedis.Default;
            var db = redis.GetDatabase();
            db.KeyDelete(key);

            Action action = () => Console.WriteLine(db.StringIncrement(key));
            Parallel.Invoke(Enumerable.Repeat(1, 100).Select(i => action).ToArray());
        }        

        [TestMethod]
        public void HashIncrement() {
            var redis = StackExchangeRedis.Default;
            var db = redis.GetDatabase();
            var key = "RedisParallelTest5";
            var field = "HashIncrement";
            var repeat = 10000;
            db.KeyDelete(key);
            Int64 last = 0;
            for (int i = 0; i < repeat; i++) {
                last = db.HashIncrement(key, field, 1);
            }
            Assert.AreEqual(last, repeat);

            last = Int64.Parse((String)db.HashGet(key, field));
            Assert.AreEqual(last, repeat);

            db.KeyDelete(key);
            Parallel.For(0, repeat, x => last = db.HashIncrement(key, "HashIncrement", 1));
            last = Int64.Parse((String)db.HashGet(key, field));
            Assert.AreEqual(last, repeat);
        }

        [TestMethod]
        public void DistributedLock() {
            var redis = StackExchangeRedis.Default;
            var cache = new RedisCacheProvider(redis);
            var key = "DistributedLock1";
            {

                var list = new List<int>();
                var except = new Random().Next(1000, 2000);
                var stopwatch = Stopwatch.StartNew();

                Parallel.For(0, except, i => {
                    using (cache.ReleasableLock(key)) {
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
                    cache.Lock(key, DistributedLockTime.IntervalMillisecond);
                    list.Add(i);
                    cache.UnLock(key);
                });

                stopwatch.Stop();
                Console.WriteLine("Handle {0} times cost {1}, {2:f2} per sec.",
                    except, stopwatch.Elapsed.TotalSeconds, except / stopwatch.Elapsed.TotalSeconds);

                Assert.AreEqual(list.Count, except);
            }
        }
    }
}
