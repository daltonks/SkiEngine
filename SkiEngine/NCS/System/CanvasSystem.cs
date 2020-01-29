using System.Collections.Generic;
using SkiEngine.NCS.Component;
using SkiEngine.NCS.Component.Base;

namespace SkiEngine.NCS.System
{
    public class CanvasSystem : ISystem
    {
        private readonly HashSet<CanvasComponent> _canvasComponents = new HashSet<CanvasComponent>();

        public void OnNodeZChanged(Node node, int previousZ)
        {
            foreach (var canvasComponent in _canvasComponents)
            {
                canvasComponent.OnNodeZChanged(node, previousZ);
            }
        }

        public void OnComponentCreated(IComponent component)
        {
            if (component is CanvasComponent canvasComponent)
            {
                _canvasComponents.Add(canvasComponent);
            }
        }

        public void OnComponentDestroyed(IComponent component)
        {
            if (component is CanvasComponent canvasComponent)
            {
                _canvasComponents.Remove(canvasComponent);
            }
        }
    }
}
