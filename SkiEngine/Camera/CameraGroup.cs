using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using SkiEngine.Canvas;
using SkiEngine.Drawable;
using SkiEngine.Util;

namespace SkiEngine.Camera
{
    public class CameraGroup
    {
        private readonly HashSet<CameraComponent> _cameras = new HashSet<CameraComponent>();
        private readonly LayeredSets<int, CameraComponent> _enabledCameras = new LayeredSets<int, CameraComponent>(camera => camera.DrawOrder);

        internal CameraGroup(CanvasComponent canvasComponent)
        {
            CanvasComponent = canvasComponent;
        }

        public IEnumerable<CameraComponent> Cameras => _cameras;

        public CanvasComponent CanvasComponent { get; }

        public void Add(CameraComponent camera)
        {
            camera.Group?.Remove(camera);

            if (_cameras.Add(camera))
            {
                camera.Group = this;
                camera.EnabledChanged += OnCameraEnabledChanged;
                camera.DrawOrderChanged += OnCameraDrawOrderChanged;
                camera.Destroyed += OnCameraComponentDestroyed;

                if (camera.Enabled)
                {
                    _enabledCameras.Add(camera);
                }
            }
        }

        private void Remove(CameraComponent camera)
        {
            if (_cameras.Remove(camera))
            {
                camera.EnabledChanged -= OnCameraEnabledChanged;
                camera.DrawOrderChanged -= OnCameraDrawOrderChanged;
                camera.Destroyed -= OnCameraComponentDestroyed;

                if (camera.Enabled)
                {
                    _enabledCameras.Remove(camera);
                }
            }
        }

        private void OnCameraEnabledChanged(CameraComponent component)
        {
            if (component.Enabled)
            {
                _enabledCameras.Add(component);
            }
            else
            {
                _enabledCameras.Remove(component);
            }
        }

        private void OnCameraDrawOrderChanged(CameraComponent component, int previousDrawOrder)
        {
            if (component.Enabled)
            {
                _enabledCameras.Update(component, previousDrawOrder);
            }
        }

        private void OnCameraComponentDestroyed(IComponent component)
        {
            Remove((CameraComponent) component);
        }

        public void Draw(SKCanvas canvas, DrawOptions options)
        {
            foreach (var cameraComponent in _enabledCameras)
            {
                cameraComponent.Draw(canvas, options);
            }
        }

        public void OnDrawableZChanged(IDrawableComponent drawableComponent, float previousZ)
        {
            foreach (var camera in _cameras)
            {
                camera.OnZChanged(drawableComponent, previousZ);
            }
        }

        public void OnDestroyed()
        {
            foreach (var camera in _cameras.ToList())
            {
                Remove(camera);
            }
        }
    }
}
