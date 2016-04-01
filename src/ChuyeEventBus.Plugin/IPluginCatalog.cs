using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Plugin {

    public interface IPluginCatalog {
        String PluginFolder { get; set; }
    }
}
