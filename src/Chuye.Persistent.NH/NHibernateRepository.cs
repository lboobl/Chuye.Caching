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

    public class NHibernateRepository<TEntry> : NHibernateRepository<TEntry, Int32>
        where TEntry : class, IAggregate {

        public NHibernateRepository(IRepositoryContext context)
           : base(context) {
        }
    }
}
