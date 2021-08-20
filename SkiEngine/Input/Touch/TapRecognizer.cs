using System;
using SkiaSharp;
using SkiEngine.Util.Extensions.SkiaSharp;

namespace SkiEngine.Input.Touch
{
    public class TapRecognizer
    {
        private readonly Func<float, float> _convertPixelsToDp;

        public TapRecognizer(Func<float, float> convertPixelsToDp)
        {
            _convertPixelsToDp = convertPixelsToDp;
        }

        private SKPoint _lastTouchPixels;
        private float _movementDistancePixels;
        public void OnTouchPressed(SkiTouch touch)
        {
            _lastTouchPixels = touch.PointPixels;
            _movementDistancePixels = 0;
        }

        public void OnTouchMoved(SkiTouch touch)
        {
            var pointPixels = touch.PointPixels;
            _movementDistancePixels += (float) _lastTouchPixels.Distance(pointPixels);
            _lastTouchPixels = pointPixels;
        }

        public bool OnTouchReleased(SkiTouch touch)
        {
            var vectorDp = _convertPixelsToDp.Invoke(_movementDistancePixels);
            return vectorDp <= 25;
        }
    }
}
