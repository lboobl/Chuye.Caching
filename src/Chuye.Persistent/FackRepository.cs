using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chuye.Persistent {
    public class FackRepository<TEntry> : Repository<TEntry> where TEntry : class, IAggregate {
        private Int32 _id = 0;
        private readonly List<TEntry> _all = new List<TEntry>();

        public FackRepository()
            : base(null) {
        }

        public override void Create(TEntry entry) {
            entry.Id = Interlocked.Increment(ref _id);
            _all.Add(entry);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void Delete(TEntry entry) {
            _all.Remove(entry);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void Delete(IEnumerable<TEntry> entries) {
            foreach (var entry in entries) {
                _all.Remove(entry);
            }
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void Update(TEntry entry) {
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void Update(IEnumerable<TEntry> entries) {
        }

        public override void Save(TEntry entry) {
            if (entry.Id == 0) {
                Create(entry);
            }
        }

        public override TEntry Retrive(int key) {
            return _all.FirstOrDefault(r => r.Id == key);
        }

        public override IEnumerable<TEntry> Retrive<TKey>(String field, IList<TKey> keys) {
            throw new NotImplementedException();
        }

        public override IQueryable<TEntry> All {
            get { return _all.AsQueryable(); }
        }

        public override bool Any(params Expression<Func<TEntry, bool>>[] predicates) {
            IQueryable<TEntry> left = All;
            foreach (var predicate in predicates) {
                left = left.Where(predicate);
            }
            return left.Any();
        }
    }
}
