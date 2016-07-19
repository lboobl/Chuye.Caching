# Chuye.Caching
.Net cache solution implemented with memcached using EnyimMemcached, redis using StackExchange.Redis

## Architecture

![Alt Architecture](doc/architecture.png)

## Usage

### ICacheProvider

```c
ICacheProvider cacheProvider
	//= new MemoryCacheProvider();
	//= MemcachedCacheProvider.Default;      //using memcached, load from configuration
	= new RedisCacheProvider("ubuntu-16");   //using redis, load from configuration

var exist = cacheProvider.TryGet<Object>(key, out val);
Assert.IsFalse(exist);
Assert.AreEqual(val, null);

cacheProvider.Overwrite(key, val);
exist = cacheProvider.TryGet<Object>(key, out val);
Assert.IsNull(val);
```


More detail in [MemcachedCacheProviderTest](src/Chuye.Caching.Tests/Memcached/MemcachedCacheProviderTest.cs)
  or [RedisCacheProviderTest](src/Chuye.Caching.Tests/Redis/RedisCacheProviderTest.cs)
  

### Dependency injection

```c
// prepare
{
	var builder = new ContainerBuilder();
	//use memcached from config
	builder.RegisterInstance(MemcachedCacheProvider.Default)
		.As<IRegionCacheProvider>();

	//use redis with connectionString
	builder.RegisterInstance(new RedisCacheProvider("ubuntu-16"))
		.As<IRegionCacheProvider>();
}
```

### Region swith

```c
{
    private readonly IRegionCacheProvider _userCache;
    private readonly IRegionCacheProvider _loginCache;

    public SomeController(IRegionCacheProvider cacheProvider) {
        _userCache = cacheProvider.Switch("user");
        _loginCache = cacheProvider.Switch("login");
    }
}

```

### DistributedLock

```c
    IDistributedLock cache 
        = MemcachedCacheProvider.Default; 
        //= new RedisCacheProvider("ubuntu-16");   //You could using redis directly though
    
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
  
## Release log

### 2.5 合并与简化接口

* 移除基于 HttpContext 的缓存实现;
* 重构设计配置生效行为;

### 2.4 添加基于配置的扩展能力

* 加入分布式缓存对所有或特定 Region 实施只读策略的能力;
* 缓存路径即 region+key 的拼接方式可以通过配置修改, 以方便分区与迁移；
* 可以通过配置中的默认过期时间限制未显示提供生命周期的缓存时长;
* 可以通过切换 region 生成新的实例以满足依赖注入的需求
  