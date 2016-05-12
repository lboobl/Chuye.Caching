using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Chuye.Caching.Redis {
    public class StackExchangeRedis :  IDisposable {
        private static StackExchangeRedis _default;
        private readonly ConnectionMultiplexer _connection;

        public static IConnectionMultiplexer Default {
            get {
                if (_default != null && _default._connection != null) {
                    if (_default._connection.IsConnected) {
                        return _default._connection;
                    }
                    else {
                        //重复 Dispose 并无副作用, 但并发时 null 赋值可能将变量置空在前
                        using (_default) { }
                        _default = null;
                    }
                }

                var connectionString = ConfigurationManager.AppSettings.Get("cache:redis");
                if (String.IsNullOrWhiteSpace(connectionString)) {
                    throw new ArgumentOutOfRangeException("cache:redis", "Configuration \"cache:redis\" missing");
                }
                var instance = new StackExchangeRedis(connectionString);
                if (Interlocked.CompareExchange(ref _default, instance, null) != null) {
                    instance.Dispose();
                }
                return _default._connection;
            }
        }

        public IConnectionMultiplexer Connection {
            get {
                return _connection;
            }
        }

        public StackExchangeRedis(String connectionString) {
            if (String.IsNullOrWhiteSpace(connectionString)) {
                throw new ArgumentOutOfRangeException("connectionString");
            }
            _connection = ConnectionMultiplexer.Connect(connectionString);
        }

        public void Dispose() {
            _connection.Dispose();
        }
    }
}