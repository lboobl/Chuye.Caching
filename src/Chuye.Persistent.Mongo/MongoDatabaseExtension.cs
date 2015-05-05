using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent.Mongo {
    public static class MongoDatabaseExtension {
        public static MongoCollection<TEntry> GetCollection<TEntry>(this MongoDatabase mongoDatabase) {
            var docs = MongoEntryMapperFactory.Mapper.Map<TEntry>();
            return mongoDatabase.GetCollection<TEntry>(docs);
        }

        public static void DropCollection<TEntry>(this MongoDatabase mongoDatabase) {
            var docs = MongoEntryMapperFactory.Mapper.Map<TEntry>();
            mongoDatabase.DropCollection(docs);
        }
    }
}
