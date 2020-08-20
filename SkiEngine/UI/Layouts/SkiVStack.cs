using System;
using System.Collections.Generic;
using SkiaSharp;
using SkiEngine.UI.Layouts.Base;
using SkiEngine.UI.Views.Base;
using SkiEngine.Util;

namespace SkiEngine.UI.Layouts
{
    public class SkiVStack : SkiMultiChildLayout
    {
        protected override void LayoutInternal(float? maxWidth, float? maxHeight)
        {
            var width = 0f;
            var height = 0f;

            var fillVerticallyChildren = new List<SkiView>();

            foreach (var child in Children)
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
            foreach (var child in Children)
            {
                UpdateChildPoint(child, SKRect.Create(0, y, maxWidth ?? child.Size.Width, child.Size.Height));
                y += child.Size.Height;
            }

            Size = new SKSize(width, height);
        }
    }
}
