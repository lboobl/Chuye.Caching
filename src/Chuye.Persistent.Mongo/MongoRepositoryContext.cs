using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent.Mongo {
    public class MongoRepositoryContext : DisposableObject, IRepositoryContext {
        private readonly Guid _id = Guid.NewGuid();
        private readonly MongoClient _client;
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

        public MongoRepositoryContext(String connectionString, String databaseName) {
            _client = new MongoClient(connectionString);
            var server = _client.GetServer();
            if (!server.DatabaseExists(databaseName)) {
                throw new Exception(String.Format("Database \"{0}\" not exists", databaseName));
            }

            Database = server.GetDatabase(databaseName);
        }
    }
}
