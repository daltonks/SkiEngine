using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using SkiEngine.NCS.Component.Base;
using SkiEngine.NCS.Component.Camera;
using SkiEngine.Util;

namespace SkiEngine.NCS.Component
{
    public class CanvasComponent : Base.Component
    {
        private readonly HashSet<CameraGroup> _cameraGroups = new HashSet<CameraGroup>();

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

        public CameraGroup CreateCameraGroup()
        {
            var cameraGroup = new CameraGroup(this);
            _cameraGroups.Add(cameraGroup);
            return cameraGroup;
        }

        public void OnNodeZChanged(Node node, int previousZ)
        {
            foreach (var drawableComponent in node.Components.OfType<IDrawableComponent>())
            foreach (var group in _cameraGroups)
            {
                group.OnDrawableZChanged(drawableComponent, previousZ);
            }
        }

        private SKRectI _previousPixelViewport;
        public void StartDraw(SKCanvas canvas, double widthXamarinUnits, double heightXamarinUnits)
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
        }

        private void OnDestroyed(IComponent component)
        {
            foreach (var group in _cameraGroups)
            {
                group.OnDestroyed();
            }
        }
    }
}
