using System;
using SkiaSharp;
using SkiEngine.Input;
using SkiEngine.UI.Gestures;
using SkiEngine.UI.Views.Base;
using SkiEngine.UI.Views.Layouts.Base;
using SkiEngine.Util;

namespace SkiEngine.UI.Views.Layouts
{
    public class SkiScrollView : SkiSingleChildLayout
    {
        public const float VerticalScrollBarWidth = 8;

        public SkiScrollView()
        {
            CanScrollHorizontallyProp = new LinkedProperty<bool>(
                this,
                valueChanged: (sender, args) =>
                {
                    UpdateScrollMax();
                    InvalidateLayout();
                }
            );
            CanScrollVerticallyProp = new LinkedProperty<bool>(
                this,
                true, 
                valueChanged: (sender, args) =>
                {
                    UpdateScrollMax();
                    InvalidateLayout();
                }
            );
            ScrollMaxProp = new LinkedProperty<SKPoint>(
                this,
                valueChanged: (sender, args) => AdjustScrollIfOutOfBounds()
            );
            ScrollProp = new LinkedProperty<SKPoint>(
                this,
                valueChanging: (oldValue, newValue) => AdjustScrollIfOutOfBounds(newValue), 
                valueChanged: (sender, args) =>
                {
                    UpdateChildPoint();
                    InvalidateSurface();
                }
            );
            SizeProp.ValueChanged += OnSizeChanged;

            PaddingProp.ValueChanged += OnPaddingChanged;

            VerticalOptions = SkiLayoutOptions.Fill;
            Padding = new SKRect(0, 0, VerticalScrollBarWidth, 0);

            var handleGestureRecognizer = new ScrollHandleGestureRecognizer(this);
            GestureRecognizers.Add(handleGestureRecognizer);

            FlingGestureRecognizer = new FlingGestureRecognizer(
                this,
                () => CanScrollHorizontally,
                () => CanScrollVertically,
                onMove: deltaPixels =>
                {
                    var previousScroll = Scroll;
                    Scroll -= PixelToLocalMatrix.MapVector(deltaPixels);
                    return Scroll != previousScroll;
                }
            );

            GestureRecognizers.Add(FlingGestureRecognizer);
        }

        public LinkedProperty<bool> CanScrollHorizontallyProp { get; }
        public bool CanScrollHorizontally
        {
            get => this.CanScrollHorizontallyProp.Value;
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

        protected virtual SKPoint DrawnScroll => Scroll;
        
        public LinkedProperty<SKPoint> ScrollMaxProp { get; }
        public SKPoint ScrollMax
        {
            get => ScrollMaxProp.Value;
            private set => ScrollMaxProp.Value = value;
        }

        private float HandleHeight => BoundsLocal.Height * BoundsLocal.Height / Content?.Size.Height ?? 0;

        public SKRect HandleBounds => SKRect.Create(
            BoundsLocal.Right - VerticalScrollBarWidth,
            Scroll.Y / ScrollMax.Y * (BoundsLocal.Height - HandleHeight),
            VerticalScrollBarWidth,
            HandleHeight
        );

        public FlingGestureRecognizer FlingGestureRecognizer { get; }

        private void OnSizeChanged(object sender, ValueChangedArgs<SKSize> args)
        {
            UpdateScrollMax();
        }

        private void OnPaddingChanged(object sender, ValueChangedArgs<SKRect> args)
        {
            UpdateScrollMax();
        }

        protected override void OnContentSizeChanged(object sender, ValueChangedArgs<SKSize> args)
        {
            UpdateScrollMax();
            if (UpdateChildPoint())
            {
                InvalidateSurface();
            }
        }

        protected override void OnContentHorizontalOptionsChanged(object sender, ValueChangedArgs<SkiLayoutOptions> args)
        {
            if (UpdateChildPoint())
            {
                InvalidateSurface();
            }
        }

        protected override void OnContentVerticalOptionsChanged(object sender, ValueChangedArgs<SkiLayoutOptions> args)
        {
            if (UpdateChildPoint())
            {
                InvalidateSurface();
            }
        }

        protected override bool UpdateChildPoint()
        {
            var drawnScroll = DrawnScroll;
            return UpdateChildPoint(new SKPoint(-drawnScroll.X, -drawnScroll.Y));
        }

        private void UpdateScrollMax()
        {
            ScrollMax = new SKPoint(
                CanScrollHorizontally
                    ? Math.Max((Content?.Size.Width ?? 0) - Size.Width + Padding.Left + Padding.Right, 0)
                    : 0,
                CanScrollVertically
                    ? Math.Max((Content?.Size.Height ?? 0) - Size.Height + Padding.Top + Padding.Bottom, 0)
                    : 0
            );
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

        public override bool OnMouseWheelScroll(double deltaDp)
        {
            Scroll -= new SKPoint(0, (float) deltaDp);

            return true;
        }

        protected override void LayoutInternal(float? maxWidth, float? maxHeight)
        {
            Size = new SKSize(maxWidth ?? 400, maxHeight ?? 400);

            if (Content == null)
            {
                return;
            }

            var contentMaxWidth = CanScrollHorizontally ? (float?) null : Size.Width - Padding.Left - Padding.Right;
            var contentMaxHeight = CanScrollVertically ? (float?) null : Size.Height - Padding.Top - Padding.Bottom;
            Content.Layout(MathNullable.Min(contentMaxWidth, Content.WidthRequest), MathNullable.Min(contentMaxHeight, Content.HeightRequest));
            UpdateChildPoint();
        }

        protected override void DrawInternal(SKCanvas canvas)
        {
            using (new SKAutoCanvasRestore(canvas))
            {
                canvas.ClipRect(BoundsLocal);

                DrawBackgroundInternal(canvas);

                using (new SKAutoCanvasRestore(canvas))
                {
                    DrawContent(canvas);
                }

                if (CanScrollVertically)
                {
                    using var handlePaint = new SKPaint
                    {
                        Color = 0xFFB5B5B5, 
                        Style = SKPaintStyle.Fill,
                        IsAntialias = true
                    };
                    canvas.DrawRoundRect(
                        HandleBounds, 
                        VerticalScrollBarWidth / 2,
                        VerticalScrollBarWidth / 2,
                        handlePaint
                    );
                }
            }
        }

        protected virtual void DrawContent(SKCanvas canvas)
        {
            Content?.Draw(canvas);
        }
    }

    public class ScrollHandleGestureRecognizer : SkiGestureRecognizer
    {
        private readonly SkiScrollView _scrollView;

        public ScrollHandleGestureRecognizer(SkiScrollView view) : base(view)
        {
            _scrollView = view;
        }

        private SKPoint _previousPixels;
        protected override PressedGestureTouchResult OnPressedInternal(SkiTouch touch)
        {
            var touchLocal = _scrollView.Node.WorldToLocalMatrix.MapPoint(touch.PointWorld);

            if (touchLocal.X < _scrollView.HandleBounds.Left || touch.DeviceType != SKTouchDeviceType.Mouse)
            {
                return PressedGestureTouchResult.Ignore;
            }

            _scrollView.FlingGestureRecognizer.AbortAnimation();

            if (!_scrollView.HandleBounds.Contains(touchLocal))
            {
                // TODO: Simplify? And maybe put as a property in SkiScrollView
                var handleY = touchLocal.Y - _scrollView.HandleBounds.Height / 2;
                var scrollY = handleY /
                    (_scrollView.BoundsLocal.Height -
                     (_scrollView.BoundsLocal.Height * _scrollView.BoundsLocal.Height /
                         _scrollView.Content?.Size.Height ?? 0)) * _scrollView.ScrollMax.Y;

                _scrollView.Scroll = new SKPoint(_scrollView.Scroll.X, scrollY);
            }
                
            _previousPixels = touch.PointPixels;

            return PressedGestureTouchResult.CancelLowerListeners;
        }

        protected override GestureTouchResult OnMovedInternal(SkiTouch touch)
        {
            var deltaPixels = touch.PointPixels - _previousPixels;
            var deltaScale = _scrollView.Size.Height / _scrollView.HandleBounds.Height;
            var deltaDp = _scrollView.UiComponent.Camera.PixelToDpMatrix.MapVector(new SKPoint(0, deltaPixels.Y * deltaScale));
            
            _scrollView.Scroll += deltaDp;

            _previousPixels = touch.PointPixels;
            return GestureTouchResult.CancelLowerListeners;
        }

        protected override GestureTouchResult OnReleasedInternal(SkiTouch touch)
        {
            return GestureTouchResult.CancelLowerListeners;
        }

        protected override void OnCancelledInternal(SkiTouch touch)
        {
                
        }
    }
}
