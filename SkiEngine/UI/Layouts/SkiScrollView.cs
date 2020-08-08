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

            var flingGestureRecognizer = new FlingGestureRecognizer(
                this,
                () => CanScrollHorizontally,
                () => CanScrollVertically,
                onMove: deltaPixels =>
                {
                    var previousScroll = Scroll;
                    Scroll -= PixelToLocalMatrix.MapVector(deltaPixels);
                    return Scroll != previousScroll;
                });

            GestureRecognizers.Add(flingGestureRecognizer);
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
    }
}
