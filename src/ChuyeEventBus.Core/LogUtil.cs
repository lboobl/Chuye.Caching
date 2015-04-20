using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Core {

    public delegate void LogTrace(string message, params object[] args);
    public delegate void LogDebug(string message, params object[] args);
    public delegate void LogWarn(string message, params object[] args);

    public static class LogUtil {
        public static LogDebug Trace = (string message, object[] args) => System.Diagnostics.Debug.WriteLine(message, args);
        public static LogDebug Debug = (string message, object[] args) => System.Diagnostics.Debug.WriteLine(message, args);
        public static LogDebug Warn = (string message, object[] args) => System.Diagnostics.Debug.WriteLine(message, args);
    }
}
