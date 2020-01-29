using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using SkiEngine.NCS.Component.Base;
using SkiEngine.Util;

namespace SkiEngine.NCS.Component
{
    public class CanvasComponent : Base.Component
    {
        private readonly HashSet<CameraComponent> _cameraComponents = new HashSet<CameraComponent>();

        private readonly LayeredSets<int, CameraComponent> _enabledCameraComponents = 
            new LayeredSets<int, CameraComponent>(component => component.DrawOrder);

        public CanvasComponent()
        {
            Destroyed += OnDestroyed;
        }

        private SKRectI _pixelViewport;
        public ref SKRectI PixelViewport => ref _pixelViewport;

        private SKMatrix _xamarinToPixelMatrix;
        public ref SKMatrix XamarinToPixelMatrix => ref _xamarinToPixelMatrix;

        private SKMatrix _pixelToXamarinMatrix;
        public ref SKMatrix PixelToXamarinMatrix => ref _pixelToXamarinMatrix;

        private SKMatrix _halfPixelViewportTranslationMatrix;
        public ref SKMatrix HalfPixelViewportTranslationMatrix => ref _halfPixelViewportTranslationMatrix;

        public void OnNodeZChanged(Node node, int previousZ)
        {
            foreach (var drawableComponent in node.Components.OfType<IDrawableComponent>())
            foreach (var cameraComponent in _cameraComponents)
            {
                cameraComponent.OnZChanged(drawableComponent, previousZ);
            }
        }

        public void AddCamera(CameraComponent cameraComponent)
        {
            cameraComponent.CanvasComponent?.RemoveCamera(cameraComponent);

            if (_cameraComponents.Add(cameraComponent))
            {
                cameraComponent.CanvasComponent = this;
                cameraComponent.EnabledChanged += OnCameraEnabledChanged;
                cameraComponent.DrawOrderChanged += OnCameraDrawOrderChanged;
                cameraComponent.Destroyed += OnCameraComponentDestroyed;

                if (cameraComponent.Enabled)
                {
                    _enabledCameraComponents.Add(cameraComponent);
                }
            }
        }

        private void RemoveCamera(CameraComponent cameraComponent)
        {
            if (_cameraComponents.Remove(cameraComponent))
            {
                cameraComponent.CanvasComponent = null;
                cameraComponent.EnabledChanged -= OnCameraEnabledChanged;
                cameraComponent.DrawOrderChanged -= OnCameraDrawOrderChanged;
                cameraComponent.Destroyed -= OnCameraComponentDestroyed;

                if (cameraComponent.Enabled)
                {
                    _enabledCameraComponents.Remove(cameraComponent);
                }
            }
        }

        private void OnCameraEnabledChanged(CameraComponent component)
        {
            if (component.Enabled)
            {
                _enabledCameraComponents.Add(component);
            }
            else
            {
                _enabledCameraComponents.Remove(component);
            }
        }

        private void OnCameraComponentDestroyed(IComponent component)
        {
            RemoveCamera((CameraComponent) component);
        }

        private void OnCameraDrawOrderChanged(CameraComponent cameraComponent, int previousDrawOrder)
        {
            if (cameraComponent.Enabled)
            {
                _enabledCameraComponents.Update(cameraComponent, previousDrawOrder);
            }
        }

        private SKRectI _previousPixelViewport;
        public void Draw(SKCanvas canvas, double widthXamarinUnits, double heightXamarinUnits)
        {
            PixelViewport = canvas.DeviceClipBounds;
            if (PixelViewport != _previousPixelViewport)
            {
                _xamarinToPixelMatrix = SKMatrix.MakeScale(
                    (float) (PixelViewport.Width / widthXamarinUnits),
                    (float) (PixelViewport.Height / heightXamarinUnits)
                );

                _pixelToXamarinMatrix = SKMatrix.MakeScale(
                    (float) (widthXamarinUnits / PixelViewport.Width),
                    (float) (heightXamarinUnits / PixelViewport.Height)
                );

                _halfPixelViewportTranslationMatrix = SKMatrix.MakeTranslation(PixelViewport.Width / 2f, PixelViewport.Height / 2f);
                _previousPixelViewport = PixelViewport;
            }

            foreach (var cameraComponent in _enabledCameraComponents)
            {
                cameraComponent.Draw(canvas);
            }
        }

        private void OnDestroyed(IComponent component)
        {
            foreach (var camera in _cameraComponents.ToList())
            {
                RemoveCamera(camera);
            }
        }
    }
}
