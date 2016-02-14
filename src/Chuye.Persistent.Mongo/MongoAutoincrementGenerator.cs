using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;

namespace Chuye.Persistent.Mongo {
    public class MongoAutoincrementGenerator {
        private readonly MongoRepositoryContext _context = null;

        public MongoAutoincrementGenerator(IRepositoryContext context) {
            _context = context as MongoRepositoryContext;
            if (_context == null) {
                throw new ArgumentOutOfRangeException("context",
                    "Expect MongoRepositoryContext but provided " + context.GetType().FullName);
            }
        }

        public Int64 GetNewId(String entryName) {
            var collection = _context.Database.GetCollection<NewId>("_NewId");
            var famArgs = new FindOneAndUpdateOptions<NewId, NewId> {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After,
                Sort = new SortDefinitionBuilder<NewId>().Descending(r => r.Id)
            };

            var result = collection.FindOneAndUpdate(
                new FilterDefinitionBuilder<NewId>().Eq(r => r.Entry, entryName),
                new UpdateDefinitionBuilder<NewId>().Inc(r => r.Last, 1),
                famArgs);
            return (Int64)result.ToBsonDocument().GetElement("Last").Value;
        }

        public class NewId {
            [BsonId]
            public ObjectId Id { get; set; }
            public String Entry { get; set; }
            public Int64 Last { get; set; }
        }
    }
}
