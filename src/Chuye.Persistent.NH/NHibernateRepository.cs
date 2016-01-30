using NHibernate;
using NHibernate.Criterion;
using NHibernate.Exceptions;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent.NH {

    public interface INHibernateRepository<TEntry> {
        void Evict(TEntry entry);
    }

    public class NHibernateRepository<TEntry> : Repository<TEntry>, INHibernateRepository<TEntry> 
        where TEntry : class, IEntry {
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

            //Check IEntry is IEntry<TKey>
        }

        public override IQueryable<TEntry> All {
            get {
                return _context.Of<TEntry>();
            }
        }

        public override TReutrn Fetch<TReutrn>(Func<IQueryable<TEntry>, TReutrn> query) {
            return SafeProceed(() => query(_context.Of<TEntry>()));
        }
        
        private void SafeProceed(Action<ISession> action) {
            try {
                action(_context.EnsureSession());
            }
            catch (GenericADOException ex) {
                throw ex.InnerException;
            }
        }

        private TResult SafeProceed<TResult>(Func<TResult> func) {
            try {
                return func();
            }
            catch (GenericADOException ex) {
                throw ex.InnerException;
            }
        }

        private TResult SafeProceed<TResult>(Func<ISession, TResult> func) {
            try {
                return func(_context.EnsureSession());
            }
            catch (GenericADOException ex) {
                throw ex.InnerException;
            }
        }

        public override TEntry Retrive(Int32 id) {
            return SafeProceed(session => session.Get<TEntry>(id));
            //return (TEntry)NHContext.EnsureSession().Get(typeof(TEntry), id);
        }

        public override IEnumerable<TEntry> Retrive(params Int32[] keys) {
            return SafeProceed(() => Retrive<Int32>("Id", keys));
        }

        public override IEnumerable<TEntry> Retrive<TKey>(String field, params TKey[] keys) {
            return SafeProceed(session => {
                ICriteria criteria = session.CreateCriteria<TEntry>()
                    .Add(Restrictions.In(field, keys.ToArray()));
                return criteria.List<TEntry>();
            });
        }

        public override IEnumerable<TEntry> Retrive<TKey>(Expression<Func<TEntry, TKey>> selector, params TKey[] keys) {
            return SafeProceed(() => {
                var field = ExpressionBuilder.GetPropertyInfo(selector).Name;
                return Retrive(field, keys);
            });
        }

        public override void Create(TEntry entry) {
            SafeProceed(session => session.Save(entry));
        }

        public override void Update(TEntry entry) {
            SafeProceed(session => session.Update(entry));
        }

        public override void Update(IEnumerable<TEntry> entries) {
            SafeProceed(session => {
                foreach (var entry in entries) {
                    session.Update(entry);
                }
                session.Flush();
            });
        }

        public override void Save(TEntry entry) {
            SafeProceed(session => session.SaveOrUpdate(entry));
        }

        public override void Save(IEnumerable<TEntry> entries) {
            SafeProceed(session => {
                foreach (var entry in entries) {
                    session.SaveOrUpdate(entry);
                }
                session.Flush();
            });
        }

        public override void Delete(TEntry entry) {
            SafeProceed(session => {
                session.Delete(entry);
                session.Flush();
            });
        }

        public override void Delete(IEnumerable<TEntry> entries) {
            SafeProceed(session => {
                foreach (var entry in entries) {
                    session.Delete(entry);
                }
                session.Flush();
            });
        }

        public override bool Any(params Expression<Func<TEntry, bool>>[] predicates) {
            return SafeProceed(() => {
                IQueryable<TEntry> query = All;
                foreach (var predicate in predicates) {
                    query = query.Where(predicate);
                }
                return query.Select(r => r).FirstOrDefault() != null;
            });
        }

        public void Evict(TEntry entry) {
            SafeProceed(session => session.Evict(entry));
        }
    }
}
