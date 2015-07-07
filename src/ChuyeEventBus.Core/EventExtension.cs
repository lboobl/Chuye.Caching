using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ChuyeEventBus.Core {
    public static class EventExtension {
        private static readonly Type _baseEventHandlerType = typeof(IEventHandler);

        public static Type GetEventType(this IEventHandler eventHandler) {
            var genericEventHandlerType = eventHandler.GetType().GetInterfaces()
                .FirstOrDefault(t => _baseEventHandlerType.IsAssignableFrom(t));
            if (genericEventHandlerType == null) {
                throw new ArgumentOutOfRangeException("eventHandler",
                    "EventHandler must instance of IEventHandler<T>");
            }
            return genericEventHandlerType.GenericTypeArguments[0];
        }

        public static IEventBehaviour GetEventBehaviour(Type eventType) {
            var attr = eventType.GetCustomAttribute<CustomEventBehaviourAttribute>();
            if (attr != null) {
                return (IEventBehaviour)Activator.CreateInstance(attr.CustomEventBehaviourType);
            }
            else {
                return new DefaultEventBehaviour() { EventType = eventType };
            }
        }
    }
}
