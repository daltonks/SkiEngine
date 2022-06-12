using System;
using SkiaSharp;
using SkiEngine.Input;
using SkiEngine.UI.Views.Base;

namespace SkiEngine.UI.Gestures
{
    public class TapGestureRecognizer : SkiGestureRecognizer
    {
        public event Action ConsideredNotATap;

        private readonly Action<SKPoint> _onTapped;

        public TapGestureRecognizer(SkiView view, Action<SKPoint> onTapped) : base(view)
        {
            _onTapped = onTapped;
        }
        
        private SKPoint _previousPixels;
        private double _totalMovedDp;
        private bool _hasMovedTooFarToBeATap;
        protected override PressedGestureTouchResult OnPressedInternal(SkiTouch touch)
        {
            _previousPixels = touch.PointPixels;
            _totalMovedDp = 0;
            _hasMovedTooFarToBeATap = false;
            return PressedGestureTouchResult.Passthrough;
        }

        protected override GestureTouchResult OnMovedInternal(SkiTouch touch)
        {
            if (!_hasMovedTooFarToBeATap)
            {
                _totalMovedDp += UiComponent.Camera.PixelToDpMatrix
                    .MapVector(touch.PointPixels - _previousPixels)
                    .Length;

                if (_totalMovedDp >= 15)
                {
                    ConsideredNotATap?.Invoke();
                    _hasMovedTooFarToBeATap = true;
                }
            }

            _previousPixels = touch.PointPixels;

            return GestureTouchResult.Passthrough;
        }

        protected override GestureTouchResult OnReleasedInternal(SkiTouch touch)
        {
            if (_hasMovedTooFarToBeATap)
            {
                return GestureTouchResult.Passthrough;
            }

            _onTapped(touch.PointWorld);
            return GestureTouchResult.CancelOtherListeners;
        }

        protected override void OnCancelledInternal(SkiTouch touch)
        {
            
        }
    }
}
