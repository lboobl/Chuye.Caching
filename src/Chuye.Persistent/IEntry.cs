using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent {

    public interface IEntry {
    }

    public interface IEntry<TKey> : IEntry {
        TKey Id { get; set; }
    }
}
