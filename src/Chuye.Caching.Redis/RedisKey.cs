using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Caching.Redis {
    public class RedisKey {
        private String key1;
        private byte[] key2;

        private RedisKey(String key) {
            key1 = key;
        }

        private RedisKey(byte[] key) {
            key2 = key;
        }

        public Boolean HasValue {
            get {
                return key1 != null || key2 != null;
            }
        }

        public static implicit operator RedisKey(String key) {
            return new RedisKey(key);
        }

        public static implicit operator RedisKey(Byte[] key) {
            return new RedisKey(key);
        }

        public static implicit operator String(RedisKey key) {
            if (key.key1 != null) {
                return key.key1;
            }
            if (key.key2 != null) {
                key.key1 = Encoding.UTF8.GetString(key.key2);
                return key.key1;
            }
            return null;
        }

        public static implicit operator byte[] (RedisKey key) {
            if (key.key2 != null) {
                return key.key2;
            }
            if (key.key1 != null) {
                key.key2 = Encoding.UTF8.GetBytes(key.key1);
                return key.key2;
            }
            return null;
        }

        public override String ToString() {
            return (String)this;
        }
    }
}
