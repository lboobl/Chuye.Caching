using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chuye.Persistent {
    public class DisposableObject : IDisposable {
        private bool disposed = false;

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(Boolean finalizing) {
            if (!disposed) {
                if (finalizing) {
                    // Release managed resources                    
                    DisposeManaged();
                }
                DisposeUnmanaged();
                // Release unmanaged resources
                disposed = true;
            }
        }

        protected virtual void DisposeManaged() {
        }

        protected virtual void DisposeUnmanaged() {
        }

        ~DisposableObject() {
            Dispose(false);
        }

        public static void TryDispose(Object obj) {
            if (obj != null) {
                var dispose = obj as IDisposable;
                if (dispose != null) {
                    dispose.Dispose();
                }
            }
        }
    }
}
