using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent {
   
    public interface IRepository<in TEntry, out TResult, TKey> where TEntry : IAggregate<TKey> {
        void Create(TEntry entry);
        void Update(TEntry entry);
        void Update(IEnumerable<TEntry> entries);
        void Save(TEntry entry);
        void Save(IEnumerable<TEntry> entries);
        void Delete(TEntry entry);
        void Delete(IEnumerable<TEntry> entries);

        IQueryable<TResult> All { get; }
        TReutrn Fetch<TReutrn>(Func<IQueryable<TResult>, TReutrn> query);
        TResult Retrive(TKey id);
        IEnumerable<TResult> Retrive(params TKey[] keys);
        IEnumerable<TResult> Retrive(String field, params TKey[] keys);
    }
}
