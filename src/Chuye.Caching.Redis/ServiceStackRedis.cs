﻿using ServiceStack.Redis;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Caching.Redis {
    public class ServiceStackRedis : IRedis, IDisposable {
        private static IRedisClientsManager _redisFactory;

        static ServiceStackRedis() {
            Initialize();
        }

        private static void Initialize() {
            var connectionString = ConfigurationManager.AppSettings.Get("cache:redis");
            if (!String.IsNullOrWhiteSpace(connectionString)) {
                Initialize(connectionString);
            }
        }

        public static void Initialize(params String[] hosts) {
            if (_redisFactory != null) {
                _redisFactory.Dispose();
            }
            _redisFactory = new PooledRedisClientManager(hosts) { ConnectTimeout = 100 };
        }

        //注意，用完需要dispose
        public IRedisNativeClient GetRedisClient() {
            if (_redisFactory == null) {
                throw new ArgumentException("Configuration \"cache:redis\" miss or method Initialize() not invoke");
            }
            return (IRedisNativeClient)_redisFactory.GetClient();
        }

        public void Dispose() {
            if (_redisFactory != null) {
                _redisFactory.Dispose();
            }
        }

        public Boolean KeyExists(RedisField key) {
            using (var client = GetRedisClient()) {
                return client.Exists(key) == 1L;
            }
        }

        public Boolean KeyDelete(RedisField key) {
            using (var client = GetRedisClient()) {
                return client.Del(key) == 1L;
            }
        }

        public Boolean KeyExpire(RedisField key, TimeSpan expiry) {
            using (var client = GetRedisClient()) {
                return client.Expire(key, (Int32)expiry.TotalSeconds);
            }
        }

        public Boolean KeyExpire(RedisField key, DateTime expiry) {
            using (var client = GetRedisClient()) {
                return client.ExpireAt(key, expiry.ToUnixTime());
            }
        }

        public RedisField StringGet(RedisField key) {
            using (var client = GetRedisClient()) {
                return client.Get(key);
            }
        }

        public void StringSet(RedisField key, RedisField value) {
            using (var client = GetRedisClient()) {
                client.Set(key, value);
            }
        }

        public Int64 StringIncrement(RedisField key, Int32 value = 1) {
            using (var client = GetRedisClient()) {
                return client.IncrBy(key, value);
            }
        }

        public Double StringIncrement(RedisField key, Double value) {
            using (var client = GetRedisClient()) {
                return client.IncrByFloat(key, value);
            }
        }

        public Int64 HashLength(RedisField key) {
            using (var client = GetRedisClient()) {
                return client.HLen(key);
            }
        }

        public Int64 HashIncrement(RedisField key, RedisField hashField, Int32 value = 1) {
            using (var client = GetRedisClient()) {
                return client.HIncrby(key, hashField, value);
            }
        }

        public Double HashIncrement(RedisField key, RedisField hashField, Double value) {
            using (var client = GetRedisClient()) {
                return client.HIncrbyFloat(key, hashField, value);
            }
        }

        public RedisField HashGet(RedisField key, RedisField hashField) {
            using (var client = GetRedisClient()) {
                return client.HGet(key, hashField);
            }
        }

        public Int64 HashSet(RedisField key, RedisField hashField, RedisField value) {
            using (var client = GetRedisClient()) {
                return client.HSet(key, hashField, value);
            }
        }

        public Int64 HashSet(RedisField key, RedisEntry hash) {
            using (var client = GetRedisClient()) {
                return client.HSet(key, hash.Name, hash.Value);
            }
        }

        public void HashSet(RedisField key, IList<RedisEntry> pairs) {
            using (var client = GetRedisClient()) {
                var hashFields = pairs.Select(p => (Byte[])p.Name).ToArray();
                var values = pairs.Select(p => (Byte[])p.Value).ToArray();
                client.HMSet(key, hashFields, values);
            }
        }

        public RedisEntry[] HashGetAll(RedisField key) {
            using (var client = GetRedisClient()) {
                var hash = client.HGetAll(key);
                if (hash == null || hash.Length == 0) {
                    return null;
                }
                var list = new RedisEntry[hash.Length / 2];
                for (int i = 0; i < list.Length; i++) {
                    list[i] = new RedisEntry(hash[2 * i], hash[2 * i + 1]);
                }
                return list;
            }
        }

        public Boolean HashDelete(RedisField key, RedisField hashField) {
            using (var client = GetRedisClient()) {
                return client.HDel(key, hashField) == 1;
            }
        }

        public Int64 ListLength(RedisField key) {
            using (var client = GetRedisClient()) {
                return client.LLen(key);
            }
        }

        public Int64 ListLeftPush(RedisField key, RedisField value) {
            using (var client = GetRedisClient()) {
                return client.LPush(key, value);
            }
        }

        public RedisField ListLeftPop(RedisField key) {
            using (var client = GetRedisClient()) {
                return client.LPop(key);
            }
        }

        public RedisField[] ListRange(RedisField key, Int32 startingFrom, Int32 endingAt) {
            using (var client = GetRedisClient()) {
                return client.LRange(key, startingFrom, endingAt)
                    .Select(r => (RedisField)r)
                    .ToArray();
            }
        }

        public Int64 ListRightPush(RedisField key, RedisField value) {
            using (var client = GetRedisClient()) {
                return client.RPush(key, value);
            }
        }

        public RedisField ListRightPop(RedisField key) {
            using (var client = GetRedisClient()) {
                return client.RPop(key);
            }
        }

        public Int64 SortedSetLength(RedisField key) {
            using (var client = GetRedisClient()) {
                return client.ZCard(key);
            }
        }

        public RedisField[] SortedSetRangeByRank(RedisField key, Int32 startPosition = 0, Int32 stopPosition = -1) {
            using (var client = GetRedisClient()) {
                return client.ZRange(key, startPosition, stopPosition)
                    .Select(r => (RedisField)r)
                    .ToArray();
            }
        }

        public RedisField[] SortedSetRangeByScore(RedisField key, double startScore = double.NegativeInfinity, double stopScore = double.PositiveInfinity, Int32 skip = 0, Int32 take = -1) {
            using (var client = GetRedisClient()) {
                return client.ZRangeByScore(key, startScore, stopScore, skip, take)
                    .Select(r => (RedisField)r)
                    .ToArray();
            }
        }

        public Int64? SortedSetRank(RedisField key, RedisField member) {
            using (var client = GetRedisClient()) {
                var value = client.ZRank(key, member);
                if (value == -1) {
                    return null;
                }
                return value;
            }
        }

        public long SortedSetAdd(RedisField key, RedisField value, double score) {
            using (var client = GetRedisClient()) {
                return client.ZAdd(key, score, value);
            }
        }

        public bool SortedSetRemove(RedisField key, RedisField member) {
            using (var client = GetRedisClient()) {
                return client.ZRem(key, member) == 1;
            }
        }

        public long SortedSetRemoveRangeByRank(RedisField key, Int32 startPosition, Int32 stopPosition) {
            using (var client = GetRedisClient()) {
                return client.ZRemRangeByRank(key, startPosition, stopPosition);
            }
        }

        public long SortedSetRemoveRangeByScore(RedisField key, double startScore, double stopScore) {
            using (var client = GetRedisClient()) {
                return client.ZRemRangeByScore(key, startScore, stopScore);
            }
        }
    }
}
