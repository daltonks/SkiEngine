﻿using SkiaSharp;
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
        
        public CameraSystem()
        {

        }

        public void OnNodeCreated(Node node)
        {
            
        }

        public void OnNodeDestroyed(Node node)
        {
            
        }

        public void OnComponentCreated(IComponent component)
        {
            if (component is CameraComponent cameraComponent)
            {
                _layeredCameraComponents.Add(cameraComponent);
                cameraComponent.DrawOrderChanged += OnCameraDrawOrderChanged;
            }
        }

        public void OnComponentDestroyed(IComponent component)
        {
            if (component is CameraComponent cameraComponent)
            {
                _layeredCameraComponents.Remove(cameraComponent);
                cameraComponent.DrawOrderChanged -= OnCameraDrawOrderChanged;
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
