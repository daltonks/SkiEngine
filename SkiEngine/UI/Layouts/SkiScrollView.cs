using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SkiaSharp;
using SkiEngine.Input;
using SkiEngine.UI.Gestures;
using SkiEngine.Util.Extensions.SkiaSharp;

namespace SkiEngine.UI.Layouts
{
    public class SkiScrollView : SkiView
    {
        private SkiAnimation _flingAnimation;

        public SkiScrollView()
        {
            CanScrollHorizontallyProp = new LinkedProperty<bool>(
                valueChanged: (oldValue, newValue) =>
                {
                    LayoutContent();
                    ScrollMaxProp.UpdateValue();
                }
            );
            CanScrollVerticallyProp = new LinkedProperty<bool>(
                true, 
                valueChanged: (oldValue, newValue) =>
                {
                    LayoutContent();
                    ScrollMaxProp.UpdateValue();
                }
            );
            ScrollMaxProp = new LinkedProperty<SKPoint>(
                updateValue: () => new SKPoint(
                    CanScrollHorizontally ? Math.Max((Content?.Size.Width ?? 0) - Size.Width, 0) : 0, 
                    CanScrollVertically ? Math.Max((Content?.Size.Height ?? 0) - Size.Height, 0) : 0
                ),
                valueChanged: (oldValue, newValue) => AdjustScrollIfOutOfBounds()
            );
            ScrollProp = new LinkedProperty<SKPoint>(
                valueChanging: (oldValue, newValue) => AdjustScrollIfOutOfBounds(newValue), 
                valueChanged: (oldValue, newValue) =>
                {
                    Content.Node.RelativePoint = new SKPoint(-newValue.X, -newValue.Y);
                    InvalidateSurface();
                }
            );
            SizeProp.ValueChanged += (size, skSize) => ScrollMaxProp.UpdateValue();
        }

        private SkiView _content;
        public SkiView Content
        {
            get => _content;
            set
            {
                if (_content != null)
                {
                    _content.Node.Destroy();
                    _content.SizeProp.ValueChanged -= OnContentSizeChanged;
                }
                
                UpdateChildNode(value);
                _content = value;
                _content.SizeProp.ValueChanged += OnContentSizeChanged;
            }
        }

        public LinkedProperty<bool> CanScrollHorizontallyProp { get; }
        public bool CanScrollHorizontally
        {
            get => CanScrollHorizontallyProp.Value;
            set => CanScrollHorizontallyProp.Value = value;
        }

        public LinkedProperty<bool> CanScrollVerticallyProp { get; }
        public bool CanScrollVertically
        {
            get => CanScrollVerticallyProp.Value;
            set => CanScrollVerticallyProp.Value = value;
        }

        public LinkedProperty<SKPoint> ScrollProp { get; }
        public SKPoint Scroll
        {
            get => ScrollProp.Value;
            set => ScrollProp.Value = value;
        }
        
        public LinkedProperty<SKPoint> ScrollMaxProp { get; }
        public SKPoint ScrollMax => ScrollMaxProp.Value;

        public override IEnumerable<SkiView> ChildrenEnumerable
        {
            get { yield return Content; }
        }

        public override bool ListensForPressedTouches => true;

        private void OnContentSizeChanged(SKSize oldSize, SKSize newSize)
        {
            ScrollMaxProp.UpdateValue();
        }

        private void AdjustScrollIfOutOfBounds()
        {
            Scroll = AdjustScrollIfOutOfBounds(Scroll);
        }

        private SKPoint AdjustScrollIfOutOfBounds(SKPoint scroll)
        {
            var scrollMax = ScrollMax;

            if (scroll.X < 0 || (Content?.Size.Width ?? 0) <= Size.Width)
            {
                scroll.X = 0;
            }
            else if (scroll.X > scrollMax.X)
            {
                scroll.X = scrollMax.X;
            }

            if (scroll.Y < 0 || (Content?.Size.Height ?? 0) <= Size.Height)
            {
                scroll.Y = 0;
            }
            else if (scroll.Y > scrollMax.Y)
            {
                scroll.Y = scrollMax.Y;
            }

            return scroll;
        }

        protected override void OnNodeChanged()
        {
            UpdateChildNode(Content);
        }

        public override void Layout(float maxWidth, float maxHeight)
        {
            Size = new SKSize(maxWidth, maxHeight);
            LayoutContent();
        }

        private void LayoutContent()
        {
            var contentWidth = CanScrollHorizontally ? float.MaxValue : Size.Width;
            var contentHeight = CanScrollVertically ? float.MaxValue : Size.Height;
            Content?.Layout(contentWidth, contentHeight);
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
        protected override GestureTouchResult OnPressedInternal(SkiTouch touch)
        {
            if (_flingAnimation != null)
            {
                UiComponent.AbortAnimation(_flingAnimation);
                _flingAnimation = null;
            }
            _touchTrackers[touch.Id] = ScrollTouchTracker.Get(touch);

            return GestureTouchResult.CancelLowerListeners;
        }

        protected override GestureTouchResult OnMovedInternal(SkiTouch touch)
        {
            var previousPointPixels = _touchTrackers[touch.Id].GetLastPointPixels();

            Scroll += PixelToLocalMatrix.MapVector(previousPointPixels - touch.PointPixels);

            _touchTrackers[touch.Id].Add(touch);

            return GestureTouchResult.CancelLowerListeners;
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        protected override GestureTouchResult OnReleasedInternal(SkiTouch touch)
        {
            var touchTracker = _touchTrackers[touch.Id];

            var flingPixelsPerSecond = touchTracker.FlingPixelsPerSecond;
            if (!CanScrollHorizontally)
            {
                flingPixelsPerSecond.X = 0;
            }
            if (!CanScrollVertically)
            {
                flingPixelsPerSecond.Y = 0;
            }
            var shouldAnimate = 
                NumPressedTouches == 0
                && (flingPixelsPerSecond.X > 5 || flingPixelsPerSecond.Y > 5);
            if (shouldAnimate)
            {
                var flingDpPerSecond = UiComponent.Camera.PixelToDpMatrix.MapVector(flingPixelsPerSecond);
                var animationSeconds = flingDpPerSecond.Length / 600;
                var flingLocalPerSecond = PixelToLocalMatrix.MapVector(flingPixelsPerSecond);

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                _flingAnimation = new SkiAnimation(
                    multiplier =>
                    {
                        var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                        stopwatch.Restart();
                        var previousScroll = Scroll;
                        Scroll += flingLocalPerSecond.Multiply(elapsedSeconds * multiplier);
                        var scrollChanged = Scroll != previousScroll;

                        if (!scrollChanged && _flingAnimation != null)
                        {
                            UiComponent.AbortAnimation(_flingAnimation);
                            _flingAnimation = null;
                        }
                    },
                    1,
                    0,
                    TimeSpan.FromSeconds(animationSeconds)
                );
                UiComponent.StartAnimation(_flingAnimation);
            }

            touchTracker.Recycle();
            _touchTrackers.Remove(touch.Id);

            return GestureTouchResult.CancelLowerListeners;
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

                    return firstConsideredTouch.PointPixels
                        .Subtract(lastTouch.PointPixels)
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
