using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChuyeEventBus.Host {
    public class FileTracker {
        private Boolean _initialed = false;
        private const Int32 WaintSpan = 100;
        private readonly Queue<String> _changes; //只关注 Changed，不关心细节时用 Boolean 值即可
        private readonly Timer _timer;
        private readonly FileSystemWatcher _watcher;

        public event Action<String> FileChanged;

        public FileTracker(String folder, String filter) {
            _changes = new Queue<String>();
            _watcher = new FileSystemWatcher(folder, filter);
            _timer = new Timer(callback, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void WatchAsync() {
            if (!_initialed) {
                _watcher.Changed += watcher_Changed;
                _watcher.Created += watcher_Changed;
                _watcher.Deleted += watcher_Changed;
                _watcher.Renamed += watcher_Changed;

                var subFolders = Directory.EnumerateDirectories(_watcher.Path, "*", SearchOption.TopDirectoryOnly);
                foreach (var subFolder in subFolders) {
                    var watcher = new FileSystemWatcher(subFolder, _watcher.Filter);
                    watcher.Changed += watcher_Changed;
                    watcher.Created += watcher_Changed;
                    watcher.Deleted += watcher_Changed;
                    watcher.Renamed += watcher_Changed;
                    watcher.EnableRaisingEvents = true;
                }

                _watcher.EnableRaisingEvents = true;
                _initialed = true;
            }
        }

        private void watcher_Changed(object sender, FileSystemEventArgs e) {
            //节流，保证回调未结束前，不再进行通知
            if (_changes.Count == 0) {
                _changes.Enqueue(e.FullPath);
                _timer.Change(100, Timeout.Infinite);
            }
        }

        private void callback(Object state) {
            Debug.WriteLine(String.Format("{0:HH:mm:ss.ffff} Watcher: 文件变更", DateTime.Now));
            var changes = new List<String>();
            while (_changes.Count > 0) {
                changes.Add(_changes.Dequeue());
            }
            foreach (var change in changes.Distinct(StringComparer.OrdinalIgnoreCase)) {
                if (FileChanged != null) {
                    FileChanged(change);
                }
            }
        }
    }
}
