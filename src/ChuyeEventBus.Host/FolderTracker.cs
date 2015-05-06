using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    public class FolderTracker {
        private Boolean _initialed = false;
        private const Int32 WaintSpan = 100;
        private Boolean _hasChanged; //只关注 Changed，不关心细节时用 Boolean 值即可
        private readonly Timer _timer;
        private readonly FileSystemWatcher _watcher;

        public event Action FolderChanged;

        public FolderTracker(String folder, String filter) {
            _hasChanged = false;
            _watcher = new FileSystemWatcher(folder, filter);
            _timer = new Timer(callback, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void WatchAsync() {
            if (!_initialed) {
                _watcher.Changed += watcher_Changed;
                _watcher.Created += watcher_Changed;
                _watcher.Deleted += watcher_Changed;
                _watcher.Renamed += watcher_Changed;
                _watcher.EnableRaisingEvents = true;
                _initialed = true;
            }
        }

        private void watcher_Changed(object sender, FileSystemEventArgs e) {
            //节流，保证回调未结束前，不再进行通知
            if (!_hasChanged) {
                _hasChanged = true;
                _timer.Change(100, Timeout.Infinite);
            }
        }

        private void callback(Object state) {
            Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} Watcher: 文件变更", DateTime.Now));
            if (FolderChanged != null) {
                FolderChanged();
            }
            _hasChanged = false;
        }
    }
}
