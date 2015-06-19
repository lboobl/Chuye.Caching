using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Chuye.Persistent.Mongo {
    public class MongoRepositoryContext : DisposableObject, IRepositoryContext {
        private readonly Guid _id = Guid.NewGuid();
        private readonly MongoClient _client;
        private const String ConnectionStringPattern = "mongodb://[^/]+/(?<db>.+)";

        public MongoDatabase Database { get; private set; }

        public Guid ID {
            get { return _id; }
        }

        public Boolean DistributedTransactionSupported {
            get { return false; }
        }

        public void Begin() {
            throw new NotImplementedException();
        }

        public void Rollback() {
            throw new NotImplementedException();
        }

        public void Commit() {
            throw new NotImplementedException();
        }

        public MongoRepositoryContext(String connectionString)
            : this(connectionString, null) {
        }

        public MongoRepositoryContext(String connectionString, String databaseName) {
            //mongodb://用户名:密码@ip:端口/连接的默认数据库
            var match = Regex.Match(connectionString, ConnectionStringPattern);
            if (!match.Success) {
                throw new ArgumentOutOfRangeException("connectionString");
            }
            _client = new MongoClient(connectionString);
            var server = _client.GetServer();
            if (databaseName == null) {
                databaseName = match.Groups["db"].Value;
            }            
			if (!server.DatabaseExists(databaseName)) {
				throw new Exception(String.Format("Database \"{0}\" not exists", databaseName));
			}
            Database = server.GetDatabase(databaseName);
        }
    }
}
