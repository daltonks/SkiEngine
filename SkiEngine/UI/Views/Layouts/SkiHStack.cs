using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using SkiEngine.UI.Views.Base;
using SkiEngine.UI.Views.Layouts.Base;
using SkiEngine.Util;

namespace SkiEngine.UI.Views.Layouts
{
    public class SkiHStack : SkiMultiChildLayout
    {
        public SkiHStack()
        {
            SpacingProp = new LinkedProperty<float>(
                this, 
                10,
                valueChanged: (sender, args) => InvalidateLayout()
            );
        }

        public LinkedProperty<float> SpacingProp { get; }
        public float Spacing
        {
            get => SpacingProp.Value;
            set => SpacingProp.Value = value;
        }

        protected override void LayoutInternal(float? maxWidth, float? maxHeight)
        {
            var visibleChildren = Children;

            if (!visibleChildren.Any())
            {
                Size = new SKSize();
                return;
            }

            var width = (Children.Count - 1) * Spacing + Padding.Left + Padding.Right;
            var height = 0f;
            var maxChildHeight = maxHeight - Padding.Top - Padding.Bottom;

            var fillHorizontallyChildren = new List<SkiView>();

            foreach (var child in visibleChildren)
            {
                if (child.HorizontalOptions == SkiLayoutOptions.Fill)
                {
                    fillHorizontallyChildren.Add(child);
                    continue;
                }

                // Child doesn't fill horizontally, so Layout
                child.Layout(child.WidthRequest, MathNullable.Min(child.HeightRequest, maxChildHeight));
                width += child.Size.Width;
                height = Math.Max(height, child.Size.Height);
            }

            var widthPerFillHorizontallyChild = (maxWidth - width) / fillHorizontallyChildren.Count;
            foreach (var child in fillHorizontallyChildren)
            {
                child.Layout(
                    MathNullable.Min(child.WidthRequest, widthPerFillHorizontallyChild),
                    MathNullable.Min(child.HeightRequest, maxChildHeight)
                );
                width += child.Size.Width;
                height = Math.Max(height, child.Size.Height);
            }

            // Update children points
            var x = Padding.Left;
            foreach (var child in visibleChildren)
            {
                UpdateChildPoint(child, SKRect.Create(x, Padding.Top, child.Size.Width, maxChildHeight ?? child.Size.Height));
                x += child.Size.Width + Spacing;
            }

            Size = new SKSize(width, height);
        }

        protected override void DrawInternal(SKCanvas canvas)
        {
            DrawBackgroundInternal(canvas);
            foreach (var view in Children)
            {
                view.Draw(canvas);
            }
        }
    }
}
