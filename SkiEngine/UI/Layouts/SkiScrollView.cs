using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SkiaSharp;
using SkiEngine.Input;
using SkiEngine.UI.Gestures;
using SkiEngine.UI.Layouts.Base;
using SkiEngine.UI.Views.Base;
using SkiEngine.Util.Extensions.SkiaSharp;

namespace SkiEngine.UI.Layouts
{
    public class SkiScrollView : SkiSingleChildLayout
    {
        public SkiScrollView()
        {
            CanScrollHorizontallyProp = new LinkedProperty<bool>(
                this,
                valueChanged: (sender, oldValue, newValue) =>
                {
                    LayoutInternal();
                    ScrollMaxProp.UpdateValue();
                }
            );
            CanScrollVerticallyProp = new LinkedProperty<bool>(
                this,
                true, 
                valueChanged: (sender, oldValue, newValue) =>
                {
                    LayoutInternal();
                    ScrollMaxProp.UpdateValue();
                }
            );
            ScrollMaxProp = new LinkedProperty<SKPoint>(
                this,
                updateValue: () => new SKPoint(
                    CanScrollHorizontally ? Math.Max((Content?.Size.Width ?? 0) - Size.Width, 0) : 0, 
                    CanScrollVertically ? Math.Max((Content?.Size.Height ?? 0) - Size.Height, 0) : 0
                ),
                valueChanged: (sender, oldValue, newValue) => AdjustScrollIfOutOfBounds()
            );
            ScrollProp = new LinkedProperty<SKPoint>(
                this,
                valueChanging: (oldValue, newValue) => AdjustScrollIfOutOfBounds(newValue), 
                valueChanged: (sender, oldValue, newValue) =>
                {
                    Content.Node.RelativePoint = new SKPoint(-newValue.X, -newValue.Y);
                    InvalidateSurface();
                }
            );
            SizeProp.ValueChanged += (sender, size, skSize) => ScrollMaxProp.UpdateValue();

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

        protected override void OnContentSizeChanged(object sender, SKSize oldSize, SKSize newSize)
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

        protected override void LayoutInternal()
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
    }
}
