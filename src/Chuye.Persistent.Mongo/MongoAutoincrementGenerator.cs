using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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

        public Int32 GetNewId(String entryName) {
            var collection = _context.DatabaseFactory().GetCollection<NewId>("_NewId");
            var famArgs = new FindAndModifyArgs {
                Query = Query<NewId>.EQ(r => r.Entry, entryName),
                SortBy = SortBy<NewId>.Descending(r => r.Id),
                Update = Update<NewId>.Inc(r => r.Last, 1),
                Upsert = true,
                VersionReturned = FindAndModifyDocumentVersion.Modified,
            };
            var result = collection.FindAndModify(famArgs);
            return (int)result.ModifiedDocument.GetElement("Last").Value;
        }

        public class NewId {
            [BsonId]
            public ObjectId Id { get; set; }
            public String Entry { get; set; }
            public Int32 Last { get; set; }
        }
    }
}
