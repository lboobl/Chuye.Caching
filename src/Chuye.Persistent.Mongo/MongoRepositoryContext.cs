using MongoDB.Driver;
using System;
using System.Text.RegularExpressions;

namespace Chuye.Persistent.Mongo {
    public class MongoRepositoryContext : DisposableObject, IRepositoryContext {
        private readonly Guid _id = Guid.NewGuid();
        private readonly MongoClient _client;
        private const String ConnectionStringPattern = "mongodb://[^/]+/(?<db>.+)";

        public IMongoDatabase Database { get; private set; }

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
            //mongodb://�û���:����@ip:�˿�/���ӵ�Ĭ�����ݿ�
            var match = Regex.Match(connectionString, ConnectionStringPattern);
            if (!match.Success) {
                throw new ArgumentOutOfRangeException("connectionString");
            }
            _client = new MongoClient(connectionString);
            if (databaseName == null) {
                databaseName = match.Groups["db"].Value;
            }

            Database = _client.GetDatabase(databaseName);
        }
    }
}
