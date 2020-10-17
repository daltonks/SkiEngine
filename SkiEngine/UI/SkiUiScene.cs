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

        public SkiUiScene(Action invalidateSurface, Func<Node, CameraComponent, Action, SkiUiComponent> createUiComponent)
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
                .AddComponent(createUiComponent(_scene.RootNode, _camera, InvalidateSurface))
                .AddToCamera(_camera);

            _scene.Start();
            InvalidateSurface();
        }

        public SkiUiComponent UiComponent { get; }
        public SKColor BackgroundColor { get; set; } = SKColors.White;

        private bool _drawPending;
        private DateTime _drawPendingTimeout = DateTime.MinValue;
        public void InvalidateSurface()
        {
            var nowUtc = DateTime.UtcNow;

            // _drawPendingTimeout is a workaround for _drawPending
            // getting stuck to 'true' when rotating devices
            // (mostly seen on iOS)
            if (_drawPending && nowUtc < _drawPendingTimeout)
            {
                return;
            }

            _drawPending = true;
            _drawPendingTimeout = nowUtc + TimeSpan.FromSeconds(.25);
            _invalidateSurface();
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public void OnPaintSurface(SKCanvas canvas, double widthDp, double heightDp)
        {
            _scene.Update();
            _drawPending = false;
            _drawPendingTimeout = DateTime.MinValue;

            if (widthDp == 0 || widthDp == 0)
            {
                return;
            }

            var widthPixels = canvas.DeviceClipBounds.Width;
            var heightPixels = canvas.DeviceClipBounds.Height;

            var dpScale = (float) (widthPixels / widthDp);
            var scale = new SKPoint(dpScale, dpScale);
            UiComponent.Node.RelativeScale = scale;

            UiComponent.Size = new SKSize((float) widthDp, (float) heightDp);
            _camera.Node.RelativePoint = new SKPoint(widthPixels / 2f, heightPixels / 2f);

            canvas.Clear(BackgroundColor);
            _canvasComponent.StartDraw(canvas, widthDp, heightDp);

            _cameraGroup.Draw(canvas);
        }

        public void OnTouch(SkiTouch touch)
        {
            if (touch.InContact)
            {
                UiComponent.HideNativeEntry();
            }
            _scene.OnTouch(touch);
        }
    }
}
