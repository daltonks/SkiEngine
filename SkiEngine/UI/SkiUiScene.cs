using System;
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

        public void OnPaintSurface(SKCanvas canvas, double widthDp, double heightDp)
        {
            _scene.Update();

            var widthPixels = canvas.DeviceClipBounds.Width;
            var heightPixels = canvas.DeviceClipBounds.Height;
            UiComponent.Size = new SKSizeI(widthPixels, heightPixels);
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
