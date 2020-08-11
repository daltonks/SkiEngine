using System;
using SkiaSharp;
using SkiEngine.Input;
using SkiEngine.UI.Views.Base;

namespace SkiEngine.UI.Gestures
{
    public class TapGestureRecognizer : SkiGestureRecognizer
    {
        private readonly Action<SKPoint> _onTapped;

        public TapGestureRecognizer(SkiView view, Action<SKPoint> onTapped) : base(view)
        {
            _onTapped = onTapped;
        }

        private SKPoint _pressedPixels;
        protected override GestureTouchResult OnPressedInternal(SkiTouch touch)
        {
            _pressedPixels = touch.PointPixels;
            return GestureTouchResult.Passthrough;
        }

        protected override GestureTouchResult OnMovedInternal(SkiTouch touch)
        {
            return GestureTouchResult.Passthrough;
        }

        protected override GestureTouchResult OnReleasedInternal(SkiTouch touch)
        {
            var deltaDp = UiComponent.Camera.PixelToDpMatrix.MapVector(touch.PointPixels - _pressedPixels);
            if (deltaDp.Length <= 10)
            {
                _onTapped(touch.PointWorld);
                return GestureTouchResult.CancelOtherListeners;
            }

            return GestureTouchResult.Passthrough;
        }

        protected override void OnCancelledInternal(SkiTouch touch)
        {
            
        }

        
    }
}
