using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Chuye.Persistent.Mongo {
    public class MongoRepository<TEntry, TKey> : Repository<TEntry, TKey> where TEntry : class, IAggregate<TKey> {
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

        public override TReutrn Fetch<TReutrn>(Func<IQueryable<TEntry>, TReutrn> query) {
            var docs = _context.Database.GetCollection<TEntry>();
            return query(docs.AsQueryable());
        }

        public override TEntry Retrive(TKey id) {
            var docs = _context.Database.GetCollection<TEntry>();
            //Builders<TEntry>.Filter.Eq(e => e.Id, id)
            return docs.Find(new FilterDefinitionBuilder<TEntry>().Eq(e => e.Id, id), new FindOptions()).Limit(1).FirstOrDefault();
        }

        public override IEnumerable<TEntry> Retrive(params TKey[] keys) {
            return Retrive("_id", keys);
        }

        public override IEnumerable<TEntry> Retrive<TMember>(String field, params TMember[] keys) {
            var docs = _context.Database.GetCollection<TEntry>();
            return docs.Find(new FilterDefinitionBuilder<TEntry>().In(field,
                keys.Select(k => BsonValue.Create(k)))).ToList();
        }

        public override IEnumerable<TEntry> Retrive<TMember>(Expression<Func<TEntry, TMember>> selector, params TMember[] keys) {
            var docs = _context.Database.GetCollection<TEntry>();
            return docs.Find(new FilterDefinitionBuilder<TEntry>().In(selector, keys)).ToList();
        }

        public override void Create(TEntry entry) {
            var docs = _context.Database.GetCollection<TEntry>();
            if (entry.Id.Equals(default(TKey))) {
                entry.Id = GenerateNewId();
            }
            docs.InsertOne(entry);
        }

        protected virtual TKey GenerateNewId() {
            Object newId = null;
            if (typeof(TKey) == typeof(ObjectId)) {
                newId = ObjectId.GenerateNewId();
            }
            else if (typeof(TKey) == typeof(Int32)) {
                var colName = _context.Database.CollectionName<TEntry>();
                checked {
                    newId = _autoincrementGenerator.GetNewId(colName);
                }
            }
            else if (typeof(TKey) == typeof(Int64)) {
                var colName = _context.Database.CollectionName<TEntry>();
                newId = _autoincrementGenerator.GetNewId(colName);
            }
            else {
                throw new ArgumentOutOfRangeException();
            }
            return (TKey)Convert.ChangeType(newId, typeof(TKey));
        }

        public override void Update(TEntry entry) {
            var docs = _context.Database.GetCollection<TEntry>();
            docs.FindOneAndReplace(new FilterDefinitionBuilder<TEntry>().Eq(r => r.Id, entry.Id), entry);
        }

        public override void Update(IEnumerable<TEntry> entries) {
            foreach (var entry in entries) {
                Update(entry);
            }
        }

        public override void Save(TEntry entry) {
            var docs = _context.Database.GetCollection<TEntry>();
            if (entry.Id.Equals(default(TKey))) {
                entry.Id = GenerateNewId();
            }
            docs.FindOneAndReplace(new FilterDefinitionBuilder<TEntry>().Eq(r => r.Id, entry.Id),
                entry, new FindOneAndReplaceOptions<TEntry, TEntry>() { IsUpsert = true });
        }

        public override void Save(IEnumerable<TEntry> entries) {
            foreach (var entry in entries) {
                Save(entry);
            }
        }

        public void Save<TMember>(TKey id, Expression<Func<TEntry, TMember>> memberExpression, TMember value) {
            var docs = _context.Database.GetCollection<TEntry>();
            docs.FindOneAndUpdate(new FilterDefinitionBuilder<TEntry>().Eq(r => r.Id, id),
                new UpdateDefinitionBuilder<TEntry>().Set(memberExpression, value),
                new FindOneAndUpdateOptions<TEntry, TEntry>() { IsUpsert = true });
        }

        public override void Delete(TEntry entry) {
            var docs = _context.Database.GetCollection<TEntry>();
            docs.DeleteOne(new FilterDefinitionBuilder<TEntry>().Eq(r => r.Id, entry.Id));
        }

        public override void Delete(IEnumerable<TEntry> entries) {
            var docs = _context.Database.GetCollection<TEntry>();
            docs.DeleteMany(Builders<TEntry>.Filter.In(e => e.Id, entries.Select(e => e.Id)));
        }

        public override bool Any(params Expression<Func<TEntry, bool>>[] predicates) {
            IQueryable<TEntry> query = All;
            foreach (var predicate in predicates) {
                query = query.Where(predicate);
            }
            return query.Select(r => r.Id).Any();
        }
    }
}
