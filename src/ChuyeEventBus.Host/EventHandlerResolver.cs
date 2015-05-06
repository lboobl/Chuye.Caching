using ChuyeEventBus.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    internal class EventHandlerResolver : IEventHandlerResolver {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public IEnumerable<IEventHandler> FindAll(String folder) {
            try {
                AggregateCatalog catalog = new AggregateCatalog();
                catalog.Catalogs.Add(new DirectoryCatalog(folder));
                CompositionContainer container = new CompositionContainer(catalog);
                return container.GetExportedValues<IEventHandler>();
            }
            catch (ReflectionTypeLoadException ex) {
                foreach (var ex2 in ex.LoaderExceptions) {
                    _logger.Error(ex2);
                }
                throw;
            }
        }
    }
}
