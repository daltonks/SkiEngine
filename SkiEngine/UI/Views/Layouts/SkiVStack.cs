using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using SkiEngine.UI.Views.Base;
using SkiEngine.UI.Views.Layouts.Base;
using SkiEngine.Util;

namespace SkiEngine.UI.Views.Layouts
{
    public class SkiVStack : SkiMultiChildLayout
    {
        public SkiVStack()
        {
            SpacingProp = new LinkedProperty<float>(
                this, 
                10,
                valueChanged: (sender, oldValue, newValue) => InvalidateLayout()
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

            var width = 0f;
            var height = (Children.Count - 1) * Spacing;

            var fillVerticallyChildren = new List<SkiView>();

            foreach (var child in visibleChildren)
            {
                if (child.VerticalOptions == SkiLayoutOptions.Fill)
                {
                    fillVerticallyChildren.Add(child);
                    continue;
                }

                // Child doesn't fill vertically, so Layout
                child.Layout(MathNullable.Min(child.WidthRequest, maxWidth), child.HeightRequest);
                width = Math.Max(width, child.Size.Width);
                height += child.Size.Height;
            }

            var heightPerFillVerticallyChild = (maxHeight - height) / fillVerticallyChildren.Count;
            foreach (var child in fillVerticallyChildren)
            {
                child.Layout(
                    MathNullable.Min(child.WidthRequest, maxWidth), 
                    MathNullable.Min(child.HeightRequest, heightPerFillVerticallyChild)
                );
                width = Math.Max(width, child.Size.Width);
                height += child.Size.Height;
            }

            // Update children points
            var y = 0f;
            foreach (var child in visibleChildren)
            {
                UpdateChildPoint(child, SKRect.Create(0, y, maxWidth ?? child.Size.Width, child.Size.Height));
                y += child.Size.Height + Spacing;
            }

            Size = new SKSize(width, height);
        }

        protected override void DrawInternal(SKCanvas canvas)
        {
            DrawBackground(canvas);
            foreach (var view in Children)
            {
                view.Draw(canvas);
            }
        }
    }
}
