using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using SkiEngine.Camera;
using SkiEngine.Drawable;

namespace SkiEngine.Canvas
{
    public class CanvasComponent : Component
    {
        private readonly HashSet<CameraGroup> _cameraGroups = new HashSet<CameraGroup>();

        public CanvasComponent()
        {
            Destroyed += OnDestroyed;
        }

        private SKRect _dpViewport;
        public ref SKRect DpViewport => ref _dpViewport;

        private SKRectI _pixelViewport;
        public ref SKRectI PixelViewport => ref _pixelViewport;

        private SKMatrix _dpToPixelMatrix;
        public ref SKMatrix DpToPixelMatrix => ref _dpToPixelMatrix;

        private SKMatrix _pixelToDpMatrix;
        public ref SKMatrix PixelToDpMatrix => ref _pixelToDpMatrix;

        private SKMatrix _halfPixelViewportTranslationMatrix;
        public ref SKMatrix HalfPixelViewportTranslationMatrix => ref _halfPixelViewportTranslationMatrix;

        public CameraGroup CreateCameraGroup()
        {
            var cameraGroup = new CameraGroup(this);
            _cameraGroups.Add(cameraGroup);
            return cameraGroup;
        }

        public void OnNodeZChanged(Node node, float previousZ)
        {
            foreach (var drawableComponent in node.Components.OfType<IDrawableComponent>())
            foreach (var group in _cameraGroups)
            {
                group.OnDrawableZChanged(drawableComponent, previousZ);
            }
        }

        private SKRectI _previousPixelViewport;
        public void StartDraw(SKCanvas canvas, double widthDp, double heightDp)
        {
            if (widthDp == 0 || heightDp == 0)
            {
                return;
            }

            PixelViewport = canvas.DeviceClipBounds;
            if (PixelViewport != _previousPixelViewport)
            {
                DpViewport = new SKRect(0, 0, (float) widthDp, (float) heightDp);

                _dpToPixelMatrix = SKMatrix.CreateScale(
                    (float) (PixelViewport.Width / widthDp),
                    (float) (PixelViewport.Height / heightDp)
                );

                _pixelToDpMatrix = SKMatrix.CreateScale(
                    (float) (widthDp / PixelViewport.Width),
                    (float) (heightDp / PixelViewport.Height)
                );

                _halfPixelViewportTranslationMatrix = SKMatrix.CreateTranslation(PixelViewport.Width / 2f, PixelViewport.Height / 2f);
                _previousPixelViewport = PixelViewport;

                foreach (var cameraGroup in _cameraGroups)
                {
                    foreach (var camera in cameraGroup.Cameras)
                    {
                        camera.RecalculatePixelMatrices();
                    }
                }
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
