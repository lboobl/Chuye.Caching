using NHibernate;
using NHibernate.SqlCommand;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent.NH {
    public class NHibernateInterceptor : EmptyInterceptor, IInterceptor {
        public override SqlString OnPrepareStatement(SqlString sql) {
#if DEBUG
            Debug.WriteLine(sql);
#endif
            return base.OnPrepareStatement(sql);
        }
    }
}
