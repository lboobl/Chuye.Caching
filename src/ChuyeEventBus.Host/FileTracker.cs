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
        private const Int32 WaitTime = 100;
        private readonly Queue<FileSystemEventArgs> _changes; //只关注 Changed，不关心细节时用 Boolean 值即可
        private readonly Timer _timer;
        private readonly FileSystemWatcher _watcher;

        public event Action<List<FileSystemEventArgs>> FileChanged;

        public FileTracker(String folder)
            : this(folder, null, false) {
        }

        public FileTracker(String folder, String filter)
            : this(folder, filter, false) {
        }

        public FileTracker(String folder, String filter, Boolean includeSubdirectories) {
            _changes = new Queue<FileSystemEventArgs>();
            _watcher = String.IsNullOrWhiteSpace(filter)
                ? new FileSystemWatcher(folder)
                : new FileSystemWatcher(folder, filter);
            _watcher.IncludeSubdirectories = includeSubdirectories;
            _timer = new Timer(callback, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void WatchAsync() {
            Init();
        }

        public void Watch() {
            Init();
            _watcher.WaitForChanged(WatcherChangeTypes.All);
        }

        private void Init() {
            if (!_initialed) {
                _watcher.Changed += watcher_Changed;
                _watcher.Created += watcher_Changed;
                _watcher.Deleted += watcher_Changed;
                _watcher.Renamed += watcher_Changed;
                _watcher.EnableRaisingEvents = true;
                _initialed = true;
            }
        }

        private void watcher_Changed(Object sender, FileSystemEventArgs e) {
            //节流，保证回调未结束前，不再进行通知
            _changes.Enqueue(e);
            if (_changes.Count == 1) {
                _timer.Change(100, Timeout.Infinite);
            }
        }

        private void callback(Object state) {
            var changes = new List<FileSystemEventArgs>();
            while (_changes.Count > 0) {
                changes.Add(_changes.Dequeue());
            }
            if (FileChanged != null) {
                FileChanged(changes);
            }
        }
    }
}
