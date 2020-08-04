using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SkiaSharp;
using SkiEngine.Input;
using SkiEngine.Util.Extensions.SkiaSharp;

namespace SkiEngine.UI.Layouts
{
    public class SkiScrollView : SkiView
    {
        private SkiAnimation _flingAnimation;

        private SkiView _content;
        public SkiView Content
        {
            get => _content;
            set
            {
                if (_content != null)
                {
                    _content.Node.Destroy();
                    _content.SizeChanged -= OnContentSizeChanged;
                }
                
                UpdateChildNode(value);
                _content = value;
                _content.SizeChanged += OnContentSizeChanged;
            }
        }

        private void OnContentSizeChanged(SKSize oldSize, SKSize newSize)
        {
            AdjustScrollIfOutOfBounds();
        }

        public override IEnumerable<SkiView> Children
        {
            get { yield return Content; }
        }

        public override bool ListensForPressedTouches => true;
        public override bool IsMultiTouchEnabled => true;

        public void Scroll(float yDelta)
        {
            Content.Node.RelativePoint = new SKPoint(Content.Node.RelativePoint.X, Content.Node.RelativePoint.Y + yDelta);
            AdjustScrollIfOutOfBounds();
        }

        public void ScrollTo(float y)
        {
            Content.Node.RelativePoint = new SKPoint(Content.Node.RelativePoint.X, y);
            AdjustScrollIfOutOfBounds();
        }

        private void AdjustScrollIfOutOfBounds()
        {
            var point = Content.Node.RelativePoint;
            if (point.Y > 0 || Content.Size.Height <= Size.Height)
            {
                Content.Node.RelativePoint = new SKPoint(point.X, 0);
            }
            else if (point.Y < -Content.Size.Height + Size.Height)
            {
                Content.Node.RelativePoint = new SKPoint(point.X, -Content.Size.Height + Size.Height);
            }
        }

        protected override void OnNodeChanged()
        {
            UpdateChildNode(Content);
        }

        public override void Layout(float maxWidth, float maxHeight)
        {
            Size = new SKSize(maxWidth, maxHeight);
            Content.Layout(maxWidth, float.MaxValue);
            AdjustScrollIfOutOfBounds();
        }

        protected override void DrawInternal(SKCanvas canvas)
        {
            canvas.Save();
            var skRect = new SKRect(0, 0, Size.Width, Size.Height);
            canvas.ClipRect(skRect);
            Content.Draw(canvas);
            canvas.Restore();
        }


        private readonly Dictionary<long, ScrollTouchTracker> _touchTrackers = new Dictionary<long, ScrollTouchTracker>();
        protected override ViewTouchResult OnPressedInternal(SkiTouch touch)
        {
            if (_flingAnimation != null)
            {
                UiComponent.AbortAnimation(_flingAnimation);
                _flingAnimation = null;
            }
            _touchTrackers[touch.Id] = ScrollTouchTracker.Get(touch);

            return ViewTouchResult.CancelLowerListeners;
        }

        protected override ViewTouchResult OnMovedInternal(SkiTouch touch)
        {
            var previousPointPixels = _touchTrackers[touch.Id].GetLastPointPixels();

            var delta = PixelToLocalMatrix.MapVector(touch.PointPixels - previousPointPixels);
            Scroll(delta.Y);

            _touchTrackers[touch.Id].Add(touch);

            InvalidateSurface();

            return ViewTouchResult.CancelLowerListeners;
        }

        protected override ViewTouchResult OnReleasedInternal(SkiTouch touch)
        {
            var touchTracker = _touchTrackers[touch.Id];

            var flingPixelsPerSecond = touchTracker.FlingPixelsPerSecond;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (NumPressedTouches == 0 && flingPixelsPerSecond.Y != 0)
            {
                var flingDpPerSecond = UiComponent.Camera.PixelToDpMatrix.MapVector(flingPixelsPerSecond);

                var animationSeconds = Math.Abs(flingDpPerSecond.Y) / 600;

                var flingLocalPerSecond = PixelToLocalMatrix.MapVector(flingPixelsPerSecond);

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                _flingAnimation = new SkiAnimation(
                    velocity =>
                    {
                        var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                        stopwatch.Restart();
                        Scroll((float) (velocity * elapsedSeconds));
                        InvalidateSurface();
                    },
                    flingLocalPerSecond.Y,
                    0,
                    TimeSpan.FromSeconds(animationSeconds)
                );
                UiComponent.StartAnimation(_flingAnimation);
            }

            touchTracker.Recycle();
            _touchTrackers.Remove(touch.Id);

            return ViewTouchResult.CancelLowerListeners;
        }

        protected override void OnCancelledInternal(SkiTouch touch)
        {
            _touchTrackers[touch.Id].Recycle();
            _touchTrackers.Remove(touch.Id);
        }

        class ScrollTouchTracker
        {
            private static readonly ConcurrentBag<ScrollTouchTracker> Cached = new ConcurrentBag<ScrollTouchTracker>();
            internal static ScrollTouchTracker Get(SkiTouch pressedTouch)
            {
                if (Cached.TryTake(out var tracker))
                {
                    tracker._stopwatch.Restart();
                    tracker._touches.Clear();
                    tracker._touches.Add(new ScrollTouch(TimeSpan.Zero, pressedTouch.PointPixels));
                    return tracker;
                }

                return new ScrollTouchTracker(pressedTouch);
            }

            private readonly Stopwatch _stopwatch = new Stopwatch();
            private readonly List<ScrollTouch> _touches = new List<ScrollTouch>();

            private ScrollTouchTracker(SkiTouch pressedTouch)
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
}
