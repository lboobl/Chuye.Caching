using ServiceStack.Redis;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Caching.Redis {
    public class ServiceStackRedis : IRedis {
        private readonly IRedisNativeClient _client;

        public ServiceStackRedis(IRedisNativeClient client) {
            _client = client;
        }

        public ServiceStackRedis() {
            var connectionString = ConfigurationManager.AppSettings.Get("cache:redis");
            if (String.IsNullOrWhiteSpace(connectionString)) {
                throw new Exception("AppSettings \"redis\" missing");
            }

            var redisManager = new PooledRedisClientManager(connectionString);
            redisManager.ConnectTimeout = 100;
            _client = (IRedisNativeClient)redisManager.GetClient();
        }

        public Boolean KeyExists(RedisKey key) {
            return _client.Exists(key) > 0;
        }

        public Int64 KeyDelete(RedisKey key) {
            return _client.Del(key);
        }

        public Boolean KeyExpire(RedisKey key, TimeSpan expiry) {
            return _client.Expire(key, (Int32)expiry.TotalSeconds);
        }

        public Boolean KeyExpire(RedisKey key, DateTime expiry) {
            return _client.ExpireAt(key, expiry.ToUnixTime());
        }

        public RedisKey StringGet(RedisKey key) {
            return _client.Get(key);
        }

        public void StringSet(RedisKey key, RedisKey value) {
            _client.Set(key, value);
        }

        public RedisKey HashGet(RedisKey key, RedisKey hashField) {
            return _client.HGet(key, hashField);
        }

        public Int64 HashSet(RedisKey key, RedisKey hashField, RedisKey value) {
            return _client.HSet(key, hashField, value);
        }

        public KeyValuePair<RedisKey, RedisKey>[] HashGetAll(RedisKey key) {
            var hash = _client.HGetAll(key);
            var list = new KeyValuePair<RedisKey, RedisKey>[hash.Length / 2];
            for (int i = 0; i < hash.Length; i += 2) {
                list[i] = new KeyValuePair<RedisKey, RedisKey>(hash[i], hash[i + 1]);
            }
            return list;
        }

        public Int64 HashDelete(RedisKey key, RedisKey hashField) {
            return _client.HDel(key, hashField);
        }

        public Int64 ListLeftPush(RedisKey key, RedisKey value) {
            return _client.LPush(key, value);
        }

        public RedisKey ListLeftPop(RedisKey key) {
            return _client.LPop(key);
        }

        public Int64 ListRightPush(RedisKey key, RedisKey value) {
            return _client.RPush(key, value);
        }

        public RedisKey ListRightPop(RedisKey key) {
            return _client.RPop(key);
        }

        public Int64 ListLength(RedisKey key) {
            return _client.LLen(key);
        }
    }
}
