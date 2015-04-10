using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace ChuyeEventBus.Core {
    public static class EventHandlerExtension {
        public static Boolean SupportMultiple(this IEventHandler eventHandler, out Int32 quantity) {
            quantity = 1;
            var eventHandlerType = eventHandler.GetType();
            var eventHandlerAttr = eventHandlerType.GetCustomAttribute<EventHandlerAttribute>();
            if (eventHandlerAttr != null) {
                quantity = eventHandlerAttr.Quantity;
                return eventHandlerAttr.SupportMultiple;
            }
            return false;
        }

        public static Boolean SupportConcurrent(this IEventHandler eventHandler) {
            var eventHandlerType = eventHandler.GetType();
            var eventHandlerAttr = eventHandlerType.GetCustomAttribute<EventHandlerAttribute>();
            if (eventHandlerAttr != null) {
                return eventHandlerAttr.SupportConcurrent;
            }
            return false;
        }
    }
}
