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
        private readonly ConcurrentRenderer _concurrentRenderer;
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

            _concurrentRenderer = new ConcurrentRenderer(
                queueDrawAction: action => SkiUi.RunAsync(action, true),
                drawAction: Draw,
                drawCompleteAction: InvalidateSurface
            );

            _touchInterceptor = new DiscardMultipleTouchInterceptor(
                new SingleTouchInterceptorSystemTouchHandler(null, _scene.SingleTouchInterceptorSystem)
            );
        }

        public SKColor BackgroundColor { get; set; }

        public void InvalidateSurface()
        {
            _invalidateSurface();
        }

        private void Draw(
            SKSurface surface, 
            ConcurrentRenderer.SnapshotHandler snapshotHandler, 
            double widthDp, 
            double heightDp, 
            bool canvasSizeChanged
        )
        {
            _scene.Update();
            
            var canvas = surface.Canvas;
            canvas.Clear(BackgroundColor);
            _canvasComponent.StartDraw(canvas, widthDp, heightDp);

            _cameraGroup.Draw(canvas);

            snapshotHandler.Snapshot(0);
        }

        public void OnPaintSurface(
            SKCanvas canvas,
            int widthPixels, 
            int heightPixels, 
            double widthDp, 
            double heightDp
        )
        {
            _concurrentRenderer.OnPaintSurface(
                widthPixels, 
                heightPixels, 
                widthDp, 
                heightDp, 
                snapshots =>
                {
                    canvas.Clear(BackgroundColor);
                    if(snapshots.TryGetValue(0, out var snapshot))
                    {
                        canvas.DrawImage(snapshot.SkImage, 0, 0);
                    }
                }
            );
        }

        public void OnTouch(SkiTouch touch)
        {
            SkiUi.RunAsync(() => {
                _touchInterceptor.OnTouch(touch);
                _concurrentRenderer.TryQueueDraw();
            });
        }
    }
}
