﻿using System;
using SkiaSharp;
using SkiEngine.Util.Extensions.SkiaSharp;

namespace SkiEngine.Input.Touch
{
    public class TapRecognizer
    {
        private const int TapDistanceDp = 25;

        public event Action<SkiTouch> TouchMovedWhileDragging;

        private readonly Func<float, float> _convertPixelsToDp;

        public TapRecognizer(Func<float, float> convertPixelsToDp)
        {
            _convertPixelsToDp = convertPixelsToDp;
        }

        public bool MovedTooFarToBeATap { get; private set; }

        private SKPoint _lastTouchPixels;
        private float _movementDistanceDp;
        public void OnTouchPressed(SkiTouch touch)
        {
            _lastTouchPixels = touch.PointPixels;
            _movementDistanceDp = 0;
            MovedTooFarToBeATap = false;
        }

        public void OnTouchMoved(SkiTouch touch)
        {
            var pointPixels = touch.PointPixels;
            _movementDistanceDp += _convertPixelsToDp.Invoke((float) _lastTouchPixels.Distance(pointPixels));
            _lastTouchPixels = pointPixels;

            if (_movementDistanceDp > TapDistanceDp)
            {
                MovedTooFarToBeATap = true;
                TouchMovedWhileDragging?.Invoke(touch);
            }
        }

        public bool OnTouchReleased(SkiTouch touch)
        {
            return _movementDistanceDp <= TapDistanceDp;
        }
    }
}
