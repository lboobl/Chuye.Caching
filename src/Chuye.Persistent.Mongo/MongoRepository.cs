using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Builders;
using System.Linq.Expressions;

namespace Chuye.Persistent.Mongo {
    public class MongoRepository<TEntry> : Repository<TEntry> where TEntry : class, IAggregate {
        private readonly MongoRepositoryContext _context = null;
        private readonly MongoAutoincrementGenerator _autoincrementGenerator;

        public MongoRepositoryContext MGContext {
            get { return _context; }
        }

        public MongoRepository(IRepositoryContext context)
            : base(context) {
            _context = context as MongoRepositoryContext;
            if (_context == null) {
                throw new ArgumentOutOfRangeException("context",
                    "Expect MongoRepositoryContext but provided " + context.GetType().FullName);
            }
            _autoincrementGenerator = new MongoAutoincrementGenerator(_context);
        }


        public override IQueryable<TEntry> All {
            get {
                var docs = _context.Database.GetCollection<TEntry>();
                return docs.AsQueryable();
            }
        }

        public override TEntry Retrive(int id) {
            var docs = _context.Database.GetCollection<TEntry>();
            return docs.FindOneById(id);
        }

        public override IEnumerable<TEntry> Retrive<TKey>(String field, IList<TKey> keys) {
            var docs = _context.Database.GetCollection<TEntry>();
            //return docs.Find(Query<TEntry>.In(r => r.Id, keys));
            return docs.Find(Query.In(field, keys.Select(k => BsonValue.Create(k)))).AsEnumerable();
        }

        public override void Create(TEntry entry) {
            var docs = _context.Database.GetCollection<TEntry>();
            entry.Id = _autoincrementGenerator.GetNewId(docs.Name);
            docs.Insert(entry);
        }

        public override void Update(TEntry entry) {
            var docs = _context.Database.GetCollection<TEntry>();
            docs.Update(Query<TEntry>.EQ(r => r.Id, entry.Id),
                Update<TEntry>.Replace(entry),
                UpdateFlags.None);
        }

        public override void Update(IEnumerable<TEntry> entries) {
            foreach (var entry in entries) {
                Update(entry);
            }
        }

        public override void Save(TEntry entry) {
            var docs = _context.Database.GetCollection<TEntry>();
            docs.Update(Query<TEntry>.EQ(r => r.Id, entry.Id),
                Update<TEntry>.Replace(entry),
                UpdateFlags.Upsert);
        }

        public void Save<TMember>(Int32 id, Expression<Func<TEntry, TMember>> memberExpression, TMember value) {
            var docs = _context.Database.GetCollection<TEntry>();
            docs.Update(Query<TEntry>.EQ(r => r.Id, id),
                Update<TEntry>.Set(memberExpression, value),
                UpdateFlags.Upsert);
        }

        public override void Delete(TEntry entry) {
            var docs = _context.Database.GetCollection<TEntry>();
            docs.Remove(Query<TEntry>.EQ(r => r.Id, entry.Id), RemoveFlags.Single);
        }

        public override void Delete(IEnumerable<TEntry> entries) {
            foreach (var entry in entries) {
                Delete(entry);
            }
        }

        public override bool Any(params Expression<Func<TEntry, bool>>[] predicates) {
            IQueryable<TEntry> query = All;
            foreach (var predicate in predicates) {
                query = query.Where(predicate);
            }
            return query.Select(r => r.Id).Any();
        }
    }

    public static class MongoDatabaseExtension {

        public static MongoCollection<TEntry> GetCollection<TEntry>(this MongoDatabase mongoDatabase) {
            return mongoDatabase.GetCollection<TEntry>(MongoEntryMapperFactory.Mapger.Map<TEntry>());
        }
    }
}
