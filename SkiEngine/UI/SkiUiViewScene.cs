using System;
using SkiaSharp;
using SkiEngine.Input;
using SkiEngine.NCS;
using SkiEngine.NCS.Component.Camera;
using SkiEngine.Touch;

namespace SkiEngine.UI
{
    public class SkiUiViewScene
    {
        private readonly Scene _scene = new Scene();
        private readonly CanvasComponent _canvasComponent;
        private readonly CameraGroup _cameraGroup;
        private readonly CameraComponent _camera;
        private readonly Action _invalidateSurface;
        private readonly DiscardMultipleTouchInterceptor _touchInterceptor;
        
        public SkiUiViewScene(Action invalidateSurface)
        {
            _invalidateSurface = invalidateSurface;

            _canvasComponent = new CanvasComponent();
            _cameraGroup = _canvasComponent.CreateCameraGroup();
            _camera = _scene.RootNode
                .CreateChild()
                .AddComponent(new CameraComponent(_cameraGroup, 0));
            _scene.RootNode.AddComponent(_canvasComponent);

            _scene.Start();

            _touchInterceptor = new DiscardMultipleTouchInterceptor(
                new SingleTouchInterceptorSystemTouchHandler(null, _scene.SingleTouchInterceptorSystem)
            );
        }

        public SKColor BackgroundColor { get; set; }

        public void InvalidateSurface()
        {
            _invalidateSurface();
        }

        public void OnPaintSurface(SKCanvas canvas, double widthDp, double heightDp)
        {
            _scene.Update();

            canvas.Clear(BackgroundColor);
            _canvasComponent.StartDraw(canvas, widthDp, heightDp);

            _cameraGroup.Draw(canvas);
        }

        public void OnTouch(SkiTouch touch)
        {
            _touchInterceptor.OnTouch(touch);
            InvalidateSurface();
        }
    }
}
