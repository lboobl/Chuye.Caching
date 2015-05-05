using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    internal static class ProcessSingleton {
        private static Mutex mutex = null;

        public static bool CreateMutex() {
            return CreateMutex(Assembly.GetEntryAssembly().FullName);
        }

        public static bool CreateMutex(string name) {
            bool result = false;
            mutex = new Mutex(true, name, out result);
            return result;
        }
        public static void ReleaseMutex() {
            if (mutex != null) {
                mutex.Close();
            }
        }
    }
}
