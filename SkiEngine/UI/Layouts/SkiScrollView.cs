﻿using System;
using SkiaSharp;
using SkiEngine.UI.Gestures;
using SkiEngine.UI.Layouts.Base;

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
                    ScrollMaxProp.UpdateValue();
                    InvalidateLayout();
                }
            );
            CanScrollVerticallyProp = new LinkedProperty<bool>(
                this,
                true, 
                valueChanged: (sender, oldValue, newValue) =>
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
                valueChanged: (sender, oldValue, newValue) => AdjustScrollIfOutOfBounds()
            );
            ScrollProp = new LinkedProperty<SKPoint>(
                this,
                valueChanging: (oldValue, newValue) => AdjustScrollIfOutOfBounds(newValue), 
                valueChanged: (sender, oldValue, newValue) =>
                {
                    UpdateChildPoint();
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

            PaddingProp.ValueChanged += (sender, oldValue, newValue) =>
            {
                ScrollMaxProp.UpdateValue();
            };
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
            base.OnContentSizeChanged(sender, oldSize, newSize);
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
            Content.Layout(contentMaxWidth, contentMaxHeight);
            UpdateChildPoint();
        }

        protected override void DrawInternal(SKCanvas canvas)
        {
            canvas.Save();
            canvas.ClipRect(BoundsLocal);
            DrawContent(canvas);
            canvas.Restore();
        }

        protected virtual void DrawContent(SKCanvas canvas)
        {
            Content.Draw(canvas);
        }
    }
}
