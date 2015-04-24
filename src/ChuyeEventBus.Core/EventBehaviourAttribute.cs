using System;
using System.Messaging;

namespace ChuyeEventBus.Core {
    public class EventBehaviourAttribute : Attribute {
        private Int32 _dequeueQuantity = 1;
        private Int32 _concurrentQuantity = 1;
        private Type _formatterType = null;
        private String _label;

        public const Int32 MAX_DEQUEUE_QUANTITY = 1024;
        public const Int32 MAX_CONCURRENT_QUANTITY = 10;

        public String Label {
            get { return _label; }
            set {
                if (String.IsNullOrWhiteSpace(value) /*|| !Regex.IsMatch(value, "^[a-zA-Z0-9_]+$")*/) {
                    throw new ArgumentOutOfRangeException("Label");
                }
                _label = value;
            }
        }

        public Type FormatterType {
            get { return _formatterType; }
            set {
                if (value == null || !typeof(IMessageFormatter).IsAssignableFrom(value)) {
                    throw new ArgumentOutOfRangeException("Formatter");
                }
                _formatterType = value;
            }
        }

        public Int32 DequeueQuantity {
            get {
                return _dequeueQuantity;
            }
            set {
                if (value <= 0 || value > MAX_DEQUEUE_QUANTITY) {
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
                if (value <= 0 || value > MAX_CONCURRENT_QUANTITY) {
                    throw new ArgumentOutOfRangeException("ConcurrentQuantity");
                }
                _concurrentQuantity = value;
            }
        }
    }
}
