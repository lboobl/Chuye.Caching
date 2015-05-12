using ChuyeEventBus.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Demo {
    class Program {
        static void Main(string[] args) {
            PluginDemo();
            //EventMocker.MockClientAsync();

            Console.WriteLine("Press <Enter> to exit");
            Console.ReadLine();
        }

        static void PluginDemo() {
            var pluginCatalogProxy = new PluginCatalogProxy();
            pluginCatalogProxy.PluginCatalogType = typeof(MyPluginCatalog);
            var pluginFolder = AppDomain.CurrentDomain.BaseDirectory;
            var pluginCatalog = (MyPluginCatalog)pluginCatalogProxy.Construct(pluginFolder);

            pluginCatalog.StartAll();

            pluginCatalogProxy.Release(pluginFolder);

        }
    }

    public interface IFeature : IPlugin {
        void Start();
    }

    public class MyFeature : IFeature {

        public void Start() {
            Console.WriteLine("MyFeature.Start()");
        }
    }

    public class MyPluginCatalog : PluginCatalog {
        protected override IEnumerable<Object> OnInitilized() {
            //return base.OnBuild(pluginFolder);
            var resolver = new ReflectionPluginResolver();
            return resolver.FindAll<IFeature>(PluginFolder);
        }

        public void StartAll() {
            Console.WriteLine("MyPluginCatalog.Start()");
            foreach (IFeature plugin in Plugins) {
                plugin.Start();
            }
        }
    }
}
