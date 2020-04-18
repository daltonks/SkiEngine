using System.Collections.Generic;
using SkiEngine.NCS.Component;
using SkiEngine.NCS.Component.Base;
using SkiEngine.NCS.Component.Camera;

namespace SkiEngine.NCS.System
{
    public class CanvasSystem : ISystem
    {
        private readonly HashSet<CanvasComponent> _canvasComponents = new HashSet<CanvasComponent>();

        public void OnNodeZChanged(Node node, float previousZ)
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
