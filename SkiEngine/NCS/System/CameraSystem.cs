using System.Collections.Generic;
using SkiaSharp;
using SkiEngine.Interfaces;
using SkiEngine.NCS.Component;
using SkiEngine.NCS.Component.Base;
using SkiEngine.Util;

namespace SkiEngine.NCS.System
{
    public class CameraSystem : ISystem, IDrawable
    {
        private readonly LayeredSets<int, CameraComponent> _layeredCameraComponents = 
            new LayeredSets<int, CameraComponent>(component => component.DrawOrder);
        
        private readonly Dictionary<Node, HashSet<IDrawableComponent>> _nodeToDrawableComponentsMap = 
            new Dictionary<Node, HashSet<IDrawableComponent>>(ReferenceEqualityComparer<Node>.Default);

        public void OnNodeCreated(Node node)
        {
            
        }

        public void OnNodeZChanged(Node node, int previousZ)
        {
            if (_nodeToDrawableComponentsMap.TryGetValue(node, out var drawableComponents))
            {
                foreach (var cameraComponent in _layeredCameraComponents)
                foreach (var drawableComponent in drawableComponents)
                {
                    cameraComponent.OnZChanged(drawableComponent, previousZ);
                }
            }
        }

        public void OnNodeDestroyed(Node node)
        {
            
        }

        public void OnComponentCreated(IComponent component)
        {
            switch (component)
            {
                case CameraComponent cameraComponent:
                    _layeredCameraComponents.Add(cameraComponent);
                    cameraComponent.DrawOrderChanged += OnCameraDrawOrderChanged;
                    break;
                case IDrawableComponent drawableComponent:
                    var node = drawableComponent.Node;

                    if(!_nodeToDrawableComponentsMap.TryGetValue(node, out var nodeDrawableComponents))
                    {
                        nodeDrawableComponents = _nodeToDrawableComponentsMap[node] = new HashSet<IDrawableComponent>();
                    }

                    nodeDrawableComponents.Add(drawableComponent);
                    break;
            }
        }

        public void OnComponentDestroyed(IComponent component)
        {
            switch (component)
            {
                case CameraComponent cameraComponent:
                    _layeredCameraComponents.Remove(cameraComponent);
                    cameraComponent.DrawOrderChanged -= OnCameraDrawOrderChanged;
                    break;
                case IDrawableComponent drawableComponent:
                    var nodeDrawableComponents = _nodeToDrawableComponentsMap[drawableComponent.Node];
                    nodeDrawableComponents.Remove(drawableComponent);
                    if (nodeDrawableComponents.Count == 0)
                    {
                        _nodeToDrawableComponentsMap.Remove(drawableComponent.Node);
                    }
                    break;
            }
        }

        private void OnCameraDrawOrderChanged(CameraComponent cameraComponent, int previousDrawOrder)
        {
            _layeredCameraComponents.Update(cameraComponent, previousDrawOrder);
        }

        public void Draw(SKCanvas canvas, int viewTarget)
        {
            foreach (var cameraComponent in _layeredCameraComponents)
            {
                if (cameraComponent.ViewTarget == viewTarget)
                {
                    cameraComponent.Draw(canvas);
                }
            }
        }
    }
}
