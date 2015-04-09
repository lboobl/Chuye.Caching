using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chuye.Persistent {
    public interface IUnitOfWork {
        Boolean DistributedTransactionSupported { get; }
        void Begin();
        void Rollback();
        void Commit();
    }
}
