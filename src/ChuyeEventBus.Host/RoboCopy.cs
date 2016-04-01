using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    public static class RoboCopy {
        public static void Mir(String sourceFolder, String targetFolder) {
            var startInfo = new ProcessStartInfo("ROBOCOPY",
            String.Format("\"{0}\" \"{1}\" /mir /NFL /NDL /NJS", sourceFolder, targetFolder));
            startInfo.CreateNoWindow = false;
            startInfo.RedirectStandardOutput = false;
            startInfo.UseShellExecute = false;
            Process.Start(startInfo).WaitForExit();
        }
    }
}
