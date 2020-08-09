using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using SkiaSharp;
using SkiEngine.UI.Layouts.Base;
using SkiEngine.UI.Views;
using SkiEngine.UI.Views.Base;

namespace SkiEngine.UI.Layouts
{
    public class SkiVStack : SkiMultiChildLayout
    {
        protected override void OnChildHorizontalOptionsChanged(object sender, SkiLayoutOptions oldValue, SkiLayoutOptions newValue)
        {
            var child = (SkiView) sender;
            child.Node.RelativePoint = new SKPoint(GetChildX(child), child.Node.RelativePoint.Y);
        }

        protected override void OnChildVerticalOptionsChanged(object sender, SkiLayoutOptions oldValue, SkiLayoutOptions newValue)
        {

        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        protected override void LayoutInternal(float maxWidth, float maxHeight)
        {
            var size = new SKSize();

            if (maxHeight == float.MaxValue)
            {
                // There is no height limit

                foreach (var child in Children)
                {
                    var width = child.WidthRequest == null
                        ? maxWidth
                        : Math.Min(child.WidthRequest.Value, maxWidth);
                    var height = child.HeightRequest ?? maxHeight;

                    child.Layout(width, height);
                    child.Node.RelativePoint = new SKPoint(0, size.Height);
                    size.Height += child.Size.Height;
                    size.Width = Math.Max(size.Width, child.Size.Width);
                }
            }
            else
            {
                // There is a height limit

                var totalHeightRequests = 0f;
                var numNoHeightRequest = 0;
                foreach (var child in Children)
                {
                    if (child.HeightRequest == null)
                    {
                        numNoHeightRequest++;
                    }
                    else
                    {
                        totalHeightRequests += child.HeightRequest.Value;
                    }
                }

                float heightOfNoHeightRequestChildren;
                float scaleOfHeightRequestChildren;

                if (totalHeightRequests < maxHeight)
                {
                    // All height requests can be honored
                    heightOfNoHeightRequestChildren = (maxHeight - totalHeightRequests) / numNoHeightRequest;
                    scaleOfHeightRequestChildren = 1;
                }
                else
                {
                    // Height requests are out-of-bounds, so they need to be shrunk
                    heightOfNoHeightRequestChildren = 0;
                    scaleOfHeightRequestChildren = maxHeight / totalHeightRequests;
                }

                foreach (var child in Children)
                {
                    var width = child.WidthRequest == null
                        ? maxWidth
                        : Math.Min(child.WidthRequest.Value, maxWidth);
                    var height = child.HeightRequest * scaleOfHeightRequestChildren ?? heightOfNoHeightRequestChildren;

                    child.Layout(width, height);
                    child.Node.RelativePoint = new SKPoint(0, size.Height);
                    size.Height += height;
                    size.Width = Math.Max(size.Width, child.Size.Width);
                }
            }

            Size = size;

            // Update children X values
            foreach (var child in Children)
            {
                child.Node.RelativePoint = new SKPoint(GetChildX(child), child.Node.RelativePoint.Y);
            }
        }

        private float GetChildX(SkiView child)
        {
            return child.HorizontalOptions switch
            {
                SkiLayoutOptions.Start => 0,
                SkiLayoutOptions.Center => Size.Width / 2 - child.Size.Width / 2,
                SkiLayoutOptions.End => Size.Width - child.Size.Width,
                SkiLayoutOptions.Fill => 0,
                _ => 0f
            };
        }
    }
}
