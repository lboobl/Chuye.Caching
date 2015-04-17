using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using System.Linq.Expressions;

namespace Chuye.Persistent.NH {
    public class NHibernateRepository<TEntry> : Repository<TEntry> where TEntry : class, IAggregate {
        private readonly NHibernateRepositoryContext _context = null;

        public NHibernateRepositoryContext NHContext {
            get { return _context; }
        }

        public NHibernateRepository(IRepositoryContext context)
            : base(context) {
            _context = context as NHibernateRepositoryContext;
            if (_context == null) {
                throw new ArgumentOutOfRangeException("context",
                    "Expect NHibernateRepositoryContext but provided " + context.GetType().FullName);
            }
        }

        public override IQueryable<TEntry> All {
            get {
                return _context.Of<TEntry>();
            }
        }

        public override TEntry Retrive(Int32 id) {
            return _context.EnsureSession().Get<TEntry>(id);
            //return (TEntry)NHContext.EnsureSession().Get(typeof(TEntry), id);
        }

        public override IEnumerable<TEntry> Retrive<TKey>(String field, IList<TKey> keys) {
            var session = NHContext.EnsureSession();
            ICriteria criteria = session.CreateCriteria<TEntry>()
                .Add(Restrictions.In(field, keys.ToArray()));
            return criteria.List<TEntry>();
        }

        public override void Create(TEntry entry) {
            _context.EnsureSession().Save(entry);
        }

        public override void Update(TEntry entry) {
            _context.EnsureSession().Update(entry);
        }

        public override void Update(IEnumerable<TEntry> entries) {
            var session = _context.EnsureSession();
            foreach (var entry in entries) {
                session.Update(entry);
            }
            session.Flush();
        }

        public override void Save(TEntry entry) {
            _context.EnsureSession().SaveOrUpdate(entry);
        }

        public override void Delete(TEntry entry) {
            var session = _context.EnsureSession();
            session.Delete(entry);
            session.Flush();
        }

        public override void Delete(IEnumerable<TEntry> entries) {
            var session = _context.EnsureSession();
            foreach (var entry in entries) {
                session.Delete(entry);
            }
            session.Flush();
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
