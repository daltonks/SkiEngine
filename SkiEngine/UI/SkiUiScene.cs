using System;
using System.Diagnostics.CodeAnalysis;
using SkiaSharp;
using SkiEngine.Camera;
using SkiEngine.Canvas;
using SkiEngine.Input;

namespace SkiEngine.UI
{
    public class SkiUiScene
    {
        private readonly Scene _scene = new Scene();
        private readonly CanvasComponent _canvasComponent;
        private readonly CameraGroup _cameraGroup;
        private readonly CameraComponent _camera;
        private readonly Action _invalidateSurface;
        
        public SkiUiScene(Action invalidateSurface)
        {
            _invalidateSurface = invalidateSurface;

            _canvasComponent = new CanvasComponent();
            _cameraGroup = _canvasComponent.CreateCameraGroup();
            _camera = _scene.RootNode
                .CreateChild()
                .AddComponent(new CameraComponent(_cameraGroup, 0));
            _scene.RootNode.AddComponent(_canvasComponent);

            UiComponent = _scene.RootNode
                .CreateChild()
                .AddComponent(new SkiUiComponent(_scene.RootNode, _camera, InvalidateSurface))
                .AddToCamera(_camera);

            _scene.Start();
            InvalidateSurface();
        }

        public SkiUiComponent UiComponent { get; }
        public SKColor BackgroundColor { get; set; }

        public void InvalidateSurface()
        {
            _invalidateSurface();
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public void OnPaintSurface(SKCanvas canvas, double widthDp, double heightDp)
        {
            _scene.Update();

            var widthPixels = canvas.DeviceClipBounds.Width;
            var heightPixels = canvas.DeviceClipBounds.Height;

            if (widthDp == 0 || widthDp == 0)
            {
                return;
            }

            var dpScale = (float) (widthPixels / widthDp);
            var scale = new SKPoint(dpScale, dpScale);
            if (UiComponent.Node.RelativeScale != scale)
            {
                UiComponent.Node.RelativeScale = scale;
            }

            UiComponent.Size = new SKSize((float) widthDp, (float) heightDp);
            _camera.Node.RelativePoint = new SKPoint(widthPixels / 2f, heightPixels / 2f);

            canvas.Clear(BackgroundColor);
            _canvasComponent.StartDraw(canvas, widthDp, heightDp);

            _cameraGroup.Draw(canvas);
        }

        public void OnTouch(SkiTouch touch)
        {
            _scene.OnTouch(touch);
        }
    }
}
