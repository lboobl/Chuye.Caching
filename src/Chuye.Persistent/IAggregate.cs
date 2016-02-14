using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent {

    public interface IAggregate<TKey> : IEntry<TKey> {
    }

    public interface IAggregate : IAggregate<Int32> {
    }
}
