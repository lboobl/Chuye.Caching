using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChuyeEventBus.Core {
    public class EventBehaviourAttribute : Attribute {
        private Int32 _dequeueQuantity = 1;
        private Int32 _concurrentQuantity = 1;
        private Type _formatter = null;
        private String _label;

        public String Label {
            get { return _label; }
            set {
                if (String.IsNullOrWhiteSpace(value) /*|| !Regex.IsMatch(value, "^[a-zA-Z0-9_]+$")*/) {
                    throw new ArgumentOutOfRangeException("Label");
                }
                _label = value;
            }
        }

        public Type Formatter {
            get { return _formatter; }
            set {
                if (value == null || !typeof(IMessageFormatter).IsAssignableFrom(value)) {
                    throw new ArgumentOutOfRangeException("Formatter");
                }
                _formatter = value;
            }
        }

        public Int32 DequeueQuantity {
            get {
                return _dequeueQuantity;
            }
            set {
                if (value <= 0 || value > 512) {
                    throw new ArgumentOutOfRangeException("DequeueQuantity");
                }
                _dequeueQuantity = value;
            }
        }

        public Int32 ConcurrentQuantity {
            get {
                return _concurrentQuantity;
            }
            set {
                if (value <= 0 || value > 10) {
                    throw new ArgumentOutOfRangeException("DequeueQuantity");
                }
                _concurrentQuantity = value;
            }
        }
    }
}
