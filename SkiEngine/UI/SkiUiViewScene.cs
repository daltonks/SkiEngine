using System;
using SkiaSharp;
using SkiEngine.Input;
using SkiEngine.NCS;
using SkiEngine.NCS.Component.Camera;

namespace SkiEngine.UI
{
    public class SkiUiViewScene
    {
        private readonly Scene _scene = new Scene();
        private readonly CanvasComponent _canvasComponent;
        private readonly CameraGroup _cameraGroup;
        private readonly CameraComponent _camera;
        private readonly Action _invalidateSurface;
        
        public SkiUiViewScene(SkiView view, Action invalidateSurface)
        {
            _invalidateSurface = invalidateSurface;

            _canvasComponent = new CanvasComponent();
            _cameraGroup = _canvasComponent.CreateCameraGroup();
            _camera = _scene.RootNode
                .CreateChild()
                .AddComponent(new CameraComponent(_cameraGroup, 0));
            _scene.RootNode.AddComponent(_canvasComponent);

            UiComponent = _scene.RootNode
                .AddComponent(new SkiUiComponent(_camera, InvalidateSurface))
                .AddToCamera(_camera);
            UiComponent.View = view;

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

            canvas.Clear(BackgroundColor);
            _canvasComponent.StartDraw(canvas, widthDp, heightDp);

            _cameraGroup.Draw(canvas);
        }

        public void OnTouch(SkiTouch touch)
        {
            UiComponent.OnTouch(touch);
        }
    }
}
