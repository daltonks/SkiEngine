using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SkiaSharp;
using SkiEngine.Input;
using SkiEngine.UI.Views.Base;
using SkiEngine.Util.Extensions.SkiaSharp;
using WeakEvent;

namespace SkiEngine.UI.Gestures
{
    public class FlingGestureRecognizer : SkiGestureRecognizer
    {
        private readonly WeakEventSource<EventArgs> _firstTouchPressedEventSource = new WeakEventSource<EventArgs>();
        public event EventHandler<EventArgs> FirstTouchPressed
        {
            add => _firstTouchPressedEventSource.Subscribe(value);
            remove => _firstTouchPressedEventSource.Unsubscribe(value);
        }

        private readonly WeakEventSource<EventArgs> _touchMovedEventSource = new WeakEventSource<EventArgs>();
        public event EventHandler<EventArgs> TouchMoved
        {
            add => _touchMovedEventSource.Subscribe(value);
            remove => _touchMovedEventSource.Unsubscribe(value);
        }

        private readonly WeakEventSource<EventArgs> _allTouchesReleasedEventSource = new WeakEventSource<EventArgs>();
        public event EventHandler<EventArgs> AllTouchesReleased
        {
            add => _allTouchesReleasedEventSource.Subscribe(value);
            remove => _allTouchesReleasedEventSource.Unsubscribe(value);
        }

        private SkiAnimation _animation;
        private readonly Func<bool> _canFlingHorizontally;
        private readonly Func<bool> _canFlingVertically;
        private readonly Func<SKPoint, bool> _onMove;
        
        public FlingGestureRecognizer(
            SkiView view,
            Func<bool> canFlingHorizontally, 
            Func<bool> canFlingVertically,
            Func<SKPoint, bool> onMove
        ) : base(view)
        {
            _canFlingHorizontally = canFlingHorizontally;
            _canFlingVertically = canFlingVertically;
            _onMove = onMove;
        }

        public override bool IsMultiTouchEnabled => true;

        public bool AllowMouseFling { get; set; } = false;
        public bool FlingOnRelease { get; set; } = true;

        public void AbortAnimation()
        {
            if (_animation != null)
            {
                UiComponent.AbortAnimation(_animation);
                _animation = null;
            }
        }

        private readonly Dictionary<long, FlingTouchTracker> _touchTrackers = new Dictionary<long, FlingTouchTracker>();
        protected override PressedGestureTouchResult OnPressedInternal(SkiTouch touch)
        {
            if (!AllowMouseFling && touch.DeviceType == SKTouchDeviceType.Mouse)
            {
                return PressedGestureTouchResult.Ignore;
            }

            if (NumPressedTouches == 0)
            {
                _firstTouchPressedEventSource.Raise(this, new EventArgs());
            }

            AbortAnimation();
            _touchTrackers[touch.Id] = FlingTouchTracker.Get(touch);

            return PressedGestureTouchResult.CancelLowerListeners;
        }

        protected override GestureTouchResult OnMovedInternal(SkiTouch touch)
        {
            var previousPointPixels = _touchTrackers[touch.Id].GetLastPointPixels();

            _onMove(touch.PointPixels - previousPointPixels);
            _touchMovedEventSource.Raise(this, new EventArgs());

            _touchTrackers[touch.Id].Add(touch);

            return GestureTouchResult.CancelLowerListeners;
        }

        protected override GestureTouchResult OnReleasedInternal(SkiTouch touch)
        {
            var touchTracker = _touchTrackers[touch.Id];

            var flingPixelsPerSecond = touchTracker.FlingPixelsPerSecond;
            if (!_canFlingHorizontally())
            {
                flingPixelsPerSecond.X = 0;
            }
            if (!_canFlingVertically())
            {
                flingPixelsPerSecond.Y = 0;
            }

            var shouldAnimate = 
                FlingOnRelease
                && NumPressedTouches == 0
                && (Math.Abs(flingPixelsPerSecond.X) > 5 || Math.Abs(flingPixelsPerSecond.Y) > 5);
            if (shouldAnimate)
            {
                var flingDpPerSecond = UiComponent.Camera.PixelToDpMatrix.MapVector(flingPixelsPerSecond);
                var animationSeconds = flingDpPerSecond.Length / 600;

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                _animation = new SkiAnimation(
                    multiplier =>
                    {
                        var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                        stopwatch.Restart();

                        var continueAnimating = _onMove(flingPixelsPerSecond.Multiply(elapsedSeconds * multiplier));
                        if (!continueAnimating)
                        {
                            AbortAnimation();
                        }
                    },
                    1,
                    0,
                    TimeSpan.FromSeconds(animationSeconds)
                );
                UiComponent.StartAnimation(_animation);
            }

            if (NumPressedTouches == 0)
            {
                _allTouchesReleasedEventSource.Raise(this, new EventArgs());
            }

            touchTracker.Recycle();
            _touchTrackers.Remove(touch.Id);

            return GestureTouchResult.CancelLowerListeners;
        }

        protected override void OnCancelledInternal(SkiTouch touch)
        {
            if (NumPressedTouches == 0)
            {
                _allTouchesReleasedEventSource.Raise(this, new EventArgs());
            }

            _touchTrackers[touch.Id].Recycle();
            _touchTrackers.Remove(touch.Id);
        }
    }

    internal class FlingTouchTracker
    {
        private static readonly ConcurrentBag<FlingTouchTracker> Cached = new ConcurrentBag<FlingTouchTracker>();
        internal static FlingTouchTracker Get(SkiTouch pressedTouch)
        {
            if (Cached.TryTake(out var tracker))
            {
                tracker._stopwatch.Restart();
                tracker._touches.Clear();
                tracker._touches.Add(new ScrollTouch(TimeSpan.Zero, pressedTouch.PointPixels));
                return tracker;
            }

            return new FlingTouchTracker(pressedTouch);
        }

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly List<ScrollTouch> _touches = new List<ScrollTouch>();

        private FlingTouchTracker(SkiTouch pressedTouch)
        {
            _stopwatch.Start();
            _touches.Add(new ScrollTouch(TimeSpan.Zero, pressedTouch.PointPixels));
        }

        public SKPoint FlingPixelsPerSecond
        {
            get
            {
                if (_touches.Count < 2)
                {
                    return new SKPoint();
                }

                var lastTouch = _touches.Last();
                var firstConsideredTouch = _touches.FirstOrDefault(
                    t => t != lastTouch 
                         && t.TimeSpan >= lastTouch.TimeSpan - TimeSpan.FromSeconds(.25)
                );

                if (firstConsideredTouch == null)
                {
                    return new SKPoint();
                }

                return lastTouch.PointPixels
                    .Subtract(firstConsideredTouch.PointPixels)
                    .Divide((lastTouch.TimeSpan - firstConsideredTouch.TimeSpan).TotalSeconds);
            }
        }

        public void Add(SkiTouch touch)
        {
            _touches.Add(new ScrollTouch(_stopwatch.Elapsed, touch.PointPixels));
        }

        public SKPoint GetLastPointPixels()
        {
            return _touches.Last().PointPixels;
        }

        public void Recycle()
        {
            Cached.Add(this);
        }

        private class ScrollTouch
        {
            public ScrollTouch(TimeSpan timeSpan, SKPoint pointPixels)
            {
                TimeSpan = timeSpan;
                PointPixels = pointPixels;
            }

            public TimeSpan TimeSpan { get; }
            public SKPoint PointPixels { get; }
        }
    }
}
