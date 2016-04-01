using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent {

    public abstract class Repository<TEntry> : Repository<TEntry, Int32> 
        where TEntry : class, IAggregate<Int32> {

        public Repository(IRepositoryContext context) : base(context) {
        }
    }
}
