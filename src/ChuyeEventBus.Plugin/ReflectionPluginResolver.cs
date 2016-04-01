using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Plugin {
    public class ReflectionPluginResolver : IPluginResolver {
        public IEnumerable<T> FindAll<T>(String pluginFolder) {
            var basePluginType = typeof(T);
            var pluginTypes = Directory.EnumerateFiles(pluginFolder, "*.dll", SearchOption.TopDirectoryOnly)
                .Concat(Directory.EnumerateFiles(pluginFolder, "*.exe", SearchOption.TopDirectoryOnly))
                .SelectMany(f => Assembly.LoadFrom(f).ExportedTypes)
                .Where(t => basePluginType.IsAssignableFrom(t) && t != basePluginType 
                    && !t.IsInterface && !t.IsAbstract);
            foreach (var pluginType in pluginTypes) {
                yield return (T)Activator.CreateInstance(pluginType);
            }
        }
    }
}
