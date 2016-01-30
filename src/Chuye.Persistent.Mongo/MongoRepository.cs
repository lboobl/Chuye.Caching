using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public override TReutrn Fetch<TReutrn>(Func<IQueryable<TEntry>, TReutrn> query) {
            var docs = _context.Database.GetCollection<TEntry>();
            return query(docs.AsQueryable());
        }

        public override TEntry Retrive(int id) {
            var docs = _context.Database.GetCollection<TEntry>();
            return docs.Find(new FilterDefinitionBuilder<TEntry>().Eq(e => e.Id, id),new FindOptions()).Limit(1).FirstOrDefault();
        }

        public override IEnumerable<TEntry> Retrive(params Int32[] keys) {
            return Retrive<Int32>("_id", keys);
        }

        public override IEnumerable<TEntry> Retrive<TKey>(String field, params TKey[] keys) {
            var docs = _context.Database.GetCollection<TEntry>();
            //return docs.Find(Query<TEntry>.In(r => r.Id, keys));
            return docs.Find(new FilterDefinitionBuilder<TEntry>().In(field, keys.Select(k => BsonValue.Create(k)))).ToList();
        }

        public override IEnumerable<TEntry> Retrive<TKey>(Expression<Func<TEntry, TKey>> selector, params TKey[] keys) {
            var docs = _context.Database.GetCollection<TEntry>();
            return docs.Find(new FilterDefinitionBuilder<TEntry>().In(selector, keys)).ToList();
        }

        public override void Create(TEntry entry) {
            var docs = _context.Database.GetCollection<TEntry>();
            var colName = _context.Database.CollectionName<TEntry>();

            entry.Id = _autoincrementGenerator.GetNewId(colName);
            docs.InsertOne(entry);
        }

        public override void Update(TEntry entry) {
            var docs = _context.Database.GetCollection<TEntry>();
            docs.FindOneAndReplace(new FilterDefinitionBuilder<TEntry>().Eq(r => r.Id, entry.Id),  entry);
        }

        public override void Update(IEnumerable<TEntry> entries) {
            foreach (var entry in entries) {
                Update(entry);
            }
        }

        public override void Save(TEntry entry) {
            var docs = _context.Database.GetCollection<TEntry>();
            if (entry.Id == 0) {
                var colName = _context.Database.CollectionName<TEntry>();
                entry.Id = _autoincrementGenerator.GetNewId(colName);
            }            

            docs.FindOneAndReplace(new FilterDefinitionBuilder<TEntry>().Eq(r => r.Id, entry.Id), 
                entry, 
                new FindOneAndReplaceOptions<TEntry, TEntry>(){IsUpsert = true});
        }

        public override void Save(IEnumerable<TEntry> entries) {
            foreach (var entry in entries) {
                Save(entry);
            }
        }

        public void Save<TMember>(Int32 id, Expression<Func<TEntry, TMember>> memberExpression, TMember value)
        {
            var docs = _context.Database.GetCollection<TEntry>();
            docs.FindOneAndUpdate(new FilterDefinitionBuilder<TEntry>().Eq(r => r.Id, id),
                new UpdateDefinitionBuilder<TEntry>().Set(memberExpression, value),
                new FindOneAndUpdateOptions<TEntry, TEntry>() {IsUpsert = true});
        }

        public override void Delete(TEntry entry) {
            var docs = _context.Database.GetCollection<TEntry>();
            docs.DeleteOne(new FilterDefinitionBuilder<TEntry>().Eq(r => r.Id, entry.Id));
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
}
