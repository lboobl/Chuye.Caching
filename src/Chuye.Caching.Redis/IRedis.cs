using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Caching.Redis {
    public interface IRedis {
        Boolean KeyExists(RedisKey key);
        Int64 KeyDelete(RedisKey key);
        Boolean KeyExpire(RedisKey key, TimeSpan expiry);
        Boolean KeyExpire(RedisKey key, DateTime expiry);
        RedisKey StringGet(RedisKey key);
        void StringSet(RedisKey key, RedisKey value);
        RedisKey HashGet(RedisKey key, RedisKey hashField);
        Int64 HashSet(RedisKey key, RedisKey hashField, RedisKey value);
        KeyValuePair<RedisKey, RedisKey>[] HashGetAll(RedisKey key);
        Int64 HashDelete(RedisKey key, RedisKey hashField);
        Int64 ListLeftPush(RedisKey key, RedisKey value);
        RedisKey ListLeftPop(RedisKey key);
        Int64 ListRightPush(RedisKey key, RedisKey value);
        RedisKey ListRightPop(RedisKey key);
        Int64 ListLength(RedisKey key);
    }
}
