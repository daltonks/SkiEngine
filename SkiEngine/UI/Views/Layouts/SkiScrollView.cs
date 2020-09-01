using System;
using SkiaSharp;
using SkiEngine.UI.Gestures;
using SkiEngine.UI.Views.Base;
using SkiEngine.UI.Views.Layouts.Base;
using SkiEngine.Util;

namespace SkiEngine.UI.Views.Layouts
{
    public class SkiScrollView : SkiSingleChildLayout
    {
        public SkiScrollView()
        {
            CanScrollHorizontallyProp = new LinkedProperty<bool>(
                this,
                valueChanged: (sender, args) =>
                {
                    ScrollMaxProp.UpdateValue();
                    InvalidateLayout();
                }
            );
            CanScrollVerticallyProp = new LinkedProperty<bool>(
                this,
                true, 
                valueChanged: (sender, args) =>
                {
                    ScrollMaxProp.UpdateValue();
                    InvalidateLayout();
                }
            );
            ScrollMaxProp = new LinkedProperty<SKPoint>(
                this,
                updateValue: () => new SKPoint(
                    CanScrollHorizontally ? Math.Max((Content?.Size.Width ?? 0) - Size.Width + Padding.Left + Padding.Right, 0) : 0, 
                    CanScrollVertically ? Math.Max((Content?.Size.Height ?? 0) - Size.Height + Padding.Top + Padding.Bottom, 0) : 0
                ),
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
            SizeProp.ValueChanged += (sender, args) => ScrollMaxProp.UpdateValue();

            var flingGestureRecognizer = new FlingGestureRecognizer(
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

            GestureRecognizers.Add(flingGestureRecognizer);

            PaddingProp.ValueChanged += (sender, args) =>
            {
                ScrollMaxProp.UpdateValue();
            };

            VerticalOptions = SkiLayoutOptions.Fill;
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
        
        public LinkedProperty<SKPoint> ScrollMaxProp { get; }
        public SKPoint ScrollMax => ScrollMaxProp.Value;

        protected override void OnContentSizeChanged(object sender, ValueChangedArgs<SKSize> args)
        {
            ScrollMaxProp.UpdateValue();
            if (UpdateChildPoint())
            {
                InvalidateSurface();
            }
        }

        protected override void OnContentHorizontalOptionsChanged(object sender,
            ValueChangedArgs<SkiLayoutOptions> args)
        {
            if (UpdateChildPoint())
            {
                InvalidateSurface();
            }
        }

        protected override void OnContentVerticalOptionsChanged(object sender,
            ValueChangedArgs<SkiLayoutOptions> args)
        {
            if (UpdateChildPoint())
            {
                InvalidateSurface();
            }
        }

        protected override bool UpdateChildPoint() => UpdateChildPoint(new SKPoint(-Scroll.X, -Scroll.Y));

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
                DrawContent(canvas);
            }
        }

        protected virtual void DrawContent(SKCanvas canvas)
        {
            Content?.Draw(canvas);
        }
    }
}
