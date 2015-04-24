using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuyeEventBus.Core {
    public class CustomEventBehaviourAttribute : Attribute {
        private Type _customEventBehaviourType;
        public Type CustomEventBehaviourType {
            get { return _customEventBehaviourType; }
            set {
                if (value == null || !typeof(IEventBehaviour).IsAssignableFrom(value)) {
                    throw new ArgumentOutOfRangeException("CustomEventBehaviourType");
                }
                _customEventBehaviourType = value;
            }
        }

        public CustomEventBehaviourAttribute(Type customEventBehaviourType) {
            CustomEventBehaviourType = customEventBehaviourType;
        }
    }
}
