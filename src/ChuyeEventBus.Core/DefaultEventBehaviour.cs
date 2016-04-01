using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Configuration;
using System.Messaging;

namespace ChuyeEventBus.Core {
    public class DefaultEventBehaviour : IEventBehaviour {
        private static readonly Type _baseEventType = typeof(IEvent);
        private static readonly IMessageFormatter _defaultMsgFormatter = new BinaryMessageFormatter();

        private Boolean _initialized = false;
        private Int32 _dequeueQuantity;
        private String _msgPath;
        private IMessageFormatter _msgFormatter;

        public Type EventType { get; set; }

        private void Initialize() {
            if (!_initialized) {
                if (!_baseEventType.IsAssignableFrom(EventType)) {
                    throw new ArgumentOutOfRangeException("Only suport type assigned from IEvent");
                }

                var attr = EventType.GetCustomAttribute<EventBehaviourAttribute>();
                if (attr == null) {
                    attr = new EventBehaviourAttribute() { DequeueQuantity = 1, ConcurrentQuantity = 1 };
                }
                _dequeueQuantity = attr.DequeueQuantity;
                var msgLabel = String.IsNullOrWhiteSpace(attr.Label)
                    ? EventType.FullName.ToString().Replace('.', '_').ToLower()
                    : attr.Label;

                var msmqHost = ".";
                var msmqHostStr = ConfigurationManager.AppSettings.Get("app:MsmqHost");
                if (!String.IsNullOrWhiteSpace(msmqHostStr)) {
                    //todo：If msmqHostStr is not a ip address, throw errors
                    msmqHost = "FormatName:DIRECT=TCP:" + msmqHostStr;
                }

                _msgPath = String.Format(@"{0}\Private$\{1}", msmqHost, msgLabel);

                if (attr.FormatterType == null) {
                    _msgFormatter = _defaultMsgFormatter;
                }
                else {
                    _msgFormatter = (IMessageFormatter)Activator.CreateInstance(attr.FormatterType);
                }
                _initialized = true;
            }
        }

        public virtual Int32 GetDequeueQuantity() {
            Initialize();
            return _dequeueQuantity;
        }


        public virtual String GetMessagePath() {
            Initialize();
            return _msgPath;
        }


        public virtual IMessageFormatter GetMessageFormatter() {
            Initialize();
            return _msgFormatter;
        }
    }
}
