using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent {

    public abstract class Repository<TEntry, TKey> : IRepository<TEntry, TEntry, TKey> where TEntry : class, IAggregate<TKey> {
        private IRepositoryContext _context;
        public IRepositoryContext Context {
            get { return _context; }
        }

        public Repository(IRepositoryContext context) {
            _context = context;
        }

        public abstract IQueryable<TEntry> All { get; }
        public abstract TReutrn Fetch<TReutrn>(Func<IQueryable<TEntry>, TReutrn> query);
        public abstract Boolean Any(params Expression<Func<TEntry, Boolean>>[] predicates);
        public abstract TEntry Retrive(TKey id);
        public abstract IEnumerable<TEntry> Retrive(params TKey[] keys);
        public abstract IEnumerable<TEntry> Retrive<TMember>(String field, params TMember[] keys);
        public abstract IEnumerable<TEntry> Retrive<TMember>(Expression<Func<TEntry, TMember>> selector, params TMember[] keys);

        public abstract void Create(TEntry entry);
        public abstract void Update(TEntry entry);
        public abstract void Update(IEnumerable<TEntry> entries);
        public abstract void Save(TEntry entry);
        public abstract void Save(IEnumerable<TEntry> entries);
        public abstract void Delete(TEntry entry);
        public abstract void Delete(IEnumerable<TEntry> entries);
    }
}
