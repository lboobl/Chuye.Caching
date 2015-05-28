using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChuyeEventBus.Plugin {
    public static class AppDoaminHelper {
        public static AppDomain CreateAppDomain(string friendlyName) {
            return CreateAppDomain(friendlyName, AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation);
        }

        public static AppDomain CreateAppDomain(string friendlyName, AppDomainSetup setup) {
            return CreateAppDomain(friendlyName, AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation);
        }

        public static AppDomain CreateAppDomain(string friendlyName, Evidence securityInfo, AppDomainSetup setup) {
            AppDomain domain = AppDomain.CreateDomain(friendlyName, securityInfo, setup);
            //domain.SetData("CurrentQueryAssemblyPath", ...);
            //domain.AssemblyResolve += new ResolveEventHandler(...);
            return domain;
        }

        public static void UnloadAppDomain(AppDomain appDomain, int attempts = 3) {
            var repeat = 0;
            var unloaded = false;
            while (repeat <= attempts && !unloaded) {
                Thread.Sleep((int)(100 * repeat));
                unloaded = UnloadAppDomain(appDomain, repeat == attempts);
                repeat++;
            }
        }

        private static bool UnloadAppDomain(AppDomain appDomain, Boolean throwIfFailed) {
            try {
                AppDomain.Unload(appDomain);
                return true;
            }
            catch (AppDomainUnloadedException) {
                if (throwIfFailed) {
                    throw;
                }
            }
            catch (CannotUnloadAppDomainException) {
                if (throwIfFailed) {
                    throw;
                }
            }
            return false;
        }
    }
}
