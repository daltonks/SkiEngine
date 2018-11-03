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

        public void OnNodeCreated(Node node)
        {
            
        }

        public void OnNodeZChanged(Node node, int previousZ)
        {
            var drawableComponents = node.DrawableComponents;

            if (drawableComponents == null)
            {
                return;
            }

            foreach (var cameraComponent in _layeredCameraComponents)
            foreach (var drawableComponent in drawableComponents)
            {
                cameraComponent.OnZChanged(drawableComponent, previousZ);
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
