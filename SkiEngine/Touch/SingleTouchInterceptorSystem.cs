using System.Collections.Generic;
using System.Linq;
using SkiEngine.NCS;
using SkiEngine.NCS.Component.Base;
using SkiEngine.NCS.System;
using SkiEngine.Util;

namespace SkiEngine.Touch
{
    public class SingleTouchInterceptorSystem : ISystem
    {
        private readonly LayeredSets<float, IComponent> _interceptors 
            = new LayeredSets<float, IComponent>(c => c.Node.WorldZ);

        public IEnumerable<ISingleTouchInterceptor> TouchInterceptors => _interceptors.ReversedItems.Cast<ISingleTouchInterceptor>();

        public void OnNodeZChanged(Node node, float previousZ)
        {
            foreach (var interceptor in node.Components.OfType<ISingleTouchInterceptor>())
            {
                _interceptors.Update((IComponent)interceptor, previousZ);
            }
        }

        public void OnComponentCreated(IComponent component)
        {
            if (component is ISingleTouchInterceptor)
            {
                _interceptors.Add(component);
            }
        }

        public void OnComponentDestroyed(IComponent component)
        {
            if (component is ISingleTouchInterceptor)
            {
                _interceptors.Remove(component);
            }
        }
    }
}
