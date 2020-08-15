﻿using System;
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
            var size = new SKSize();

            if (maxHeight == null)
            {
                // There is no height limit

                float? width = null;
                foreach (var child in Children)
                {
                    // Wait to layout horizontal Fill children,
                    // because their width can depend on the
                    // width of the other elements
                    if (child.HorizontalOptions == SkiLayoutOptions.Fill)
                    {
                        continue;
                    }
                    child.Layout(MathNullable.Min(child.WidthRequest, maxWidth), child.HeightRequest);
                    width = MathNullable.Max(width, child.Size.Width);
                }

                var fillChildMaxWidth = MathNullable.Max(width, maxWidth);

                foreach (var child in Children)
                {
                    if (child.HorizontalOptions == SkiLayoutOptions.Fill)
                    {
                        child.Layout(fillChildMaxWidth, child.HeightRequest);
                        width = MathNullable.Max(width, child.Size.Width);
                    }

                    child.Node.RelativePoint = new SKPoint(0, size.Height);
                    size.Height += child.Size.Height;
                }

                size.Width = width ?? 0;
            }
            else
            {
                // There is a height limit

                float? width = null;
                foreach (var child in Children)
                {
                    // Wait to layout horizontal Fill children,
                    // because their width can depend on the
                    // width of the other elements
                    if (child.HorizontalOptions == SkiLayoutOptions.Fill)
                    {
                        continue;
                    }
                    child.Layout(MathNullable.Min(child.WidthRequest, maxWidth), child.HeightRequest);
                    width = MathNullable.Max(width, child.Size.Width);
                }

                var fillChildMaxWidth = MathNullable.Max(width, maxWidth);

                foreach (var child in Children)
                {
                    if (child.HorizontalOptions == SkiLayoutOptions.Fill)
                    {
                        child.Layout(fillChildMaxWidth, child.HeightRequest);
                        width = MathNullable.Max(width, child.Size.Width);
                    }

                    child.Node.RelativePoint = new SKPoint(0, size.Height);
                    size.Height += child.Size.Height;
                }

                size.Width = width ?? 0;
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
                SkiLayoutOptions.Fill => 0,
                SkiLayoutOptions.Start => 0,
                SkiLayoutOptions.Center => Size.Width / 2 - child.Size.Width / 2,
                SkiLayoutOptions.End => Size.Width - child.Size.Width,
                _ => 0f
            };
        }
    }
}
