using System;
using SkiaSharp.Views.Forms;
using SkiEngine.Camera;
using SkiEngine.UI;
using Xamarin.Forms;

namespace SkiEngine.Xamarin
{
    public class SkiXamarinUiComponent : SkiUiComponent
    {
        private readonly SKCanvasView _canvasView;

        public SkiXamarinUiComponent(SKCanvasView canvasView, Node node, CameraComponent camera, Action invalidateSurface) : base(node, camera, invalidateSurface)
        {
            _canvasView = canvasView;
        }

        public override void StartAnimation(SkiAnimation skiAnimation)
        {
            new Animation(
                skiAnimation.Callback,
                skiAnimation.Start,
                skiAnimation.End,
                Easing.CubicOut,
                skiAnimation.Finished
            ).Commit(
                _canvasView,
                skiAnimation.Id,
                length: (uint) skiAnimation.Length.TotalMilliseconds
            );
        }

        public override void AbortAnimation(SkiAnimation skiAnimation)
        {
            _canvasView.AbortAnimation(skiAnimation.Id);
        }
    }
}
