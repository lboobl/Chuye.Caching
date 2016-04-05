# Chuye.Caching
.Net cache solution implemented for memcached using EnyimMemcached, for redis using StackExchange.Redis

## Architecture

![Alt Architecture](https://raw.githubusercontent.com/jusfr/Chuye.Caching/release/doc/architecture.png "Architecture")

## Usage

### HttpRuntimeCacheProvider

```c
    IHttpRuntimeCacheProvider cacheProvider 
        = new HttpRuntimeCacheProvider();
        // MemcachedCacheProvider.Default;                     //using memcached, load from configuration
        // new RedisCacheProvider(StackExchangeRedis.Default)  //using redis, load from configuration
    var exist = cacheProvider.TryGet<Object>(key, out val);
    Assert.IsFalse(exist);
    Assert.AreEqual(val, null);

    cacheProvider.Overwrite(key, val);
    exist = cacheProvider.TryGet<Object>(key, out val);
    Assert.IsNull(val);
```


More detail in [HttpRuntimeCacheProviderTest](src/Chuye.Caching.Tests/HttpRuntimeCache/HttpRuntimeCacheProviderTest.cs)

### DistributedLock

```c
    IDistributedLock cache 
        = MemcachedCacheProvider.Default; 
        //= StackExchangeRedis.Default;   //or using redis
    var key = "DistributedLock";

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
```

More detail in [MemcachedCacheProviderTest](src/Chuye.Caching.Tests/Memcached/MemcachedCacheProviderTest.cs)
  or [RedisTest](src/Chuye.Caching.Tests/Redis/RedisTest.cs)