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
                    ScrollMaxProp.UpdateValue();
                    QueueLayout();
                }
            );
            CanScrollVerticallyProp = new LinkedProperty<bool>(
                this,
                true, 
                valueChanged: (sender, oldValue, newValue) =>
                {
                    ScrollMaxProp.UpdateValue();
                    QueueLayout();
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

        protected override void OnContentChanged()
        {
            UpdateChildPoint();
        }

        protected override void OnContentSizeChanged(object sender, SKSize oldSize, SKSize newSize)
        {
            ScrollMaxProp.UpdateValue();
            if (UpdateChildPoint())
            {
                InvalidateSurface();
            }
        }

        protected override void OnContentHorizontalOptionsChanged(object sender, SkiLayoutOptions oldValue, SkiLayoutOptions newValue)
        {
            if (UpdateChildPoint())
            {
                InvalidateSurface();
            }
        }

        protected override void OnContentVerticalOptionsChanged(object sender, SkiLayoutOptions oldValue, SkiLayoutOptions newValue)
        {
            if (UpdateChildPoint())
            {
                InvalidateSurface();
            }
        }

        private bool UpdateChildPoint()
        {
            if (Content?.Node == null)
            {
                return false;
            }

            var previousPoint = Content.Node.RelativePoint;
            Content.Node.RelativePoint = new SKPoint(-Scroll.X + GetOffsetX() + Padding.Left, -Scroll.Y + GetOffsetY() + Padding.Top);
            return Content.Node.RelativePoint != previousPoint;

            float GetOffsetX()
            {
                switch (Content.HorizontalOptions)
                {
                    case SkiLayoutOptions.Fill:
                        return 0;
                    case SkiLayoutOptions.Start:
                        return 0;
                    case SkiLayoutOptions.Center:
                        if (Content.Size.Width < Size.Width)
                        {
                            return Size.Width / 2 - Content.Size.Width / 2;
                        }
                        return 0;
                    case SkiLayoutOptions.End:
                        if (Content.Size.Width < Size.Width)
                        {
                            return Size.Width - Content.Size.Width;
                        }
                        return 0;
                    default:
                        return 0;
                }
            }

            float GetOffsetY()
            {
                switch (Content.VerticalOptions)
                {
                    case SkiLayoutOptions.Fill:
                        return 0;
                    case SkiLayoutOptions.Start:
                        return 0;
                    case SkiLayoutOptions.Center:
                        if (Content.Size.Height < Size.Height)
                        {
                            return Size.Height / 2 - Content.Size.Height / 2;
                        }
                        return 0;
                    case SkiLayoutOptions.End:
                        if (Content.Size.Height < Size.Height)
                        {
                            return Size.Height - Content.Size.Height;
                        }
                        return 0;
                    default:
                        return 0;
                }
            }
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

        protected override void LayoutInternal(float? maxWidth, float? maxHeight)
        {
            Size = new SKSize(maxWidth ?? float.MaxValue, maxHeight ?? float.MaxValue);

            if (Content == null)
            {
                return;
            }

            UpdateChildPoint();
            var contentMaxWidth = CanScrollHorizontally ? (float?) null : Size.Width - Padding.Left - Padding.Right;
            var contentMaxHeight = CanScrollVertically ? (float?) null : Size.Height - Padding.Top - Padding.Bottom;
            Content.Layout(contentMaxWidth, contentMaxHeight);
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
