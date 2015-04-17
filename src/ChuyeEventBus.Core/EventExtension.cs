using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Configuration;
using System.Messaging;

namespace ChuyeEventBus.Core {
    public static class EventExtension {
        private static readonly Type _basEventHandlerType = typeof(IEventHandler);
        
        public static Type GetEventType(this IEventHandler eventHandler) {
            var genericEventHandlerType = eventHandler.GetType().GetInterfaces()
                .FirstOrDefault(t => _basEventHandlerType.IsAssignableFrom(t));
            if (genericEventHandlerType == null) {
                throw new ArgumentOutOfRangeException("eventHandler",
                    "eventHandler must instance of IEventHandler<T>");
            }
            return genericEventHandlerType.GenericTypeArguments[0];
        }

        public static EventBehaviourAttribute BuildEventBehaviour(Type eventType) {
            var eventBehaviour = eventType.GetCustomAttribute<EventBehaviourAttribute>();
            if (eventBehaviour == null || eventBehaviour.Formatter == null) {
                eventBehaviour = new EventBehaviourAttribute {
                    ConcurrentQuantity = 1,
                    DequeueQuantity = 1,
                };
            }

            if (String.IsNullOrWhiteSpace(eventBehaviour.Label)) {
                eventBehaviour.Label = eventType.FullName.ToString().Replace('.', '_').ToLower();
            }

            var msmqHostConfig = ConfigurationManager.ConnectionStrings["MsmqHost"];
            var msmqHost = msmqHostConfig != null && !String.IsNullOrWhiteSpace(msmqHostConfig.ConnectionString)
                ? "FormatName:DIRECT=TCP:" + msmqHostConfig.ConnectionString : ".";
            eventBehaviour.Label = String.Format(@"{0}\Private$\{1}", msmqHost, eventBehaviour.Label);

            if (eventBehaviour.Formatter == null) {
                eventBehaviour.Formatter = typeof(BinaryMessageFormatter);
            }
            return eventBehaviour;
        }
    }
}
