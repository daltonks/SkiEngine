using System;
using SkiaSharp;
using SkiEngine.UI.Views.Base;

namespace SkiEngine.UI.Layouts.Base
{
    public abstract class SkiLayout : SkiView
    {
        protected bool UpdateChildPoint(SkiView child, SKRect bounds)
        {
            if (child?.Node == null)
            {
                return false;
            }

            var previousPoint = child.Node.RelativePoint;
            child.Node.RelativePoint = new SKPoint(GetX(), GetY());
            return child.Node.RelativePoint != previousPoint;

            float GetX()
            {
                switch (child.HorizontalOptions)
                {
                    case SkiLayoutOptions.Fill:
                    case SkiLayoutOptions.FillAndExpand:
                    case SkiLayoutOptions.Start:
                    case SkiLayoutOptions.StartAndExpand:
                        return bounds.Left;
                    case SkiLayoutOptions.Center:
                    case SkiLayoutOptions.CenterAndExpand:
                        if (child.Size.Width < bounds.Width)
                        {
                            return bounds.MidX - child.Size.Width / 2;
                        }
                        return bounds.Left;
                    case SkiLayoutOptions.End:
                    case SkiLayoutOptions.EndAndExpand:
                        if (child.Size.Width < bounds.Width)
                        {
                            return bounds.Right - child.Size.Width;
                        }
                        return bounds.Left;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            float GetY()
            {
                switch (child.VerticalOptions)
                {
                    case SkiLayoutOptions.Fill:
                    case SkiLayoutOptions.FillAndExpand:
                    case SkiLayoutOptions.Start:
                    case SkiLayoutOptions.StartAndExpand:
                        return bounds.Top;
                    case SkiLayoutOptions.Center:
                    case SkiLayoutOptions.CenterAndExpand:
                        if (child.Size.Height < bounds.Height)
                        {
                            return bounds.MidY - child.Size.Height / 2;
                        }
                        return bounds.Top;
                    case SkiLayoutOptions.End:
                    case SkiLayoutOptions.EndAndExpand:
                        if (child.Size.Height < bounds.Height)
                        {
                            return bounds.Bottom - child.Size.Height;
                        }
                        return bounds.Top;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
