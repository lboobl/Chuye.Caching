using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chuye.Persistent {
    public interface IEntry<TKey> {
        TKey Id { get; set; }
    }
}
