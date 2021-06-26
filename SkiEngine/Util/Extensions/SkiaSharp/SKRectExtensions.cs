using System;
using System.Collections.Generic;
using SkiaSharp;

namespace SkiEngine.Util.Extensions.SkiaSharp
{
    // ReSharper disable once InconsistentNaming
    public static class SKRectExtensions
    {
        public static SKPoint Mid(this SKRect rect)
        {
            return new SKPoint(rect.MidX, rect.MidY);
        }

        public static SKRect BoundingBox(this IEnumerable<SKRect> rects)
        {
            using (var enumerator = rects.GetEnumerator())
            {
                var hasAtLeastOneRect = enumerator.MoveNext();
                if (!hasAtLeastOneRect)
                {
                    return SKRect.Empty;
                }

                var result = enumerator.Current;

                while (enumerator.MoveNext())
                {
                    var rect = enumerator.Current;
                    if (rect.Left < result.Left)
                    {
                        result.Left = rect.Left;
                    }

                    if (rect.Right > result.Right)
                    {
                        result.Right = rect.Right;
                    }

                    if (rect.Top < result.Top)
                    {
                        result.Top = rect.Top;
                    }

                    if (rect.Bottom > result.Bottom)
                    {
                        result.Bottom = rect.Bottom;
                    }
                }

                return result;
            }
        }

        public static SKPoint[] Points(this SKRect rect)
        {
            return new[]
            {
                new SKPoint(rect.Left, rect.Top),
                new SKPoint(rect.Right, rect.Top),
                new SKPoint(rect.Right, rect.Bottom),
                new SKPoint(rect.Left, rect.Bottom)
            };
        }

        public static SKPoint TopLeft(this SKRect rect)
        {
            return new SKPoint(rect.Left, rect.Top);
        }

        public static SKPoint TopRight(this SKRect rect)
        {
            return new SKPoint(rect.Right, rect.Top);
        }

        public static SKPoint BottomLeft(this SKRect rect)
        {
            return new SKPoint(rect.Left, rect.Bottom);
        }

        public static SKPoint BottomRight(this SKRect rect)
        {
            return new SKPoint(rect.Right, rect.Bottom);
        }

        public static SKRectI ToSKRectI(this SKRect rect)
        {
            return new SKRectI(
                (int) Math.Floor(rect.Left),
                (int) Math.Floor(rect.Top),
                (int) Math.Ceiling(rect.Right),
                (int) Math.Ceiling(rect.Bottom)
            );
        }

        public static SKRect MovedInsideOf(this SKRect smallRect, SKRect bigRect)
        {
            if (smallRect.Left < bigRect.Left)
            {
                smallRect.Offset(bigRect.Left - smallRect.Left, 0);
            }
            else if (smallRect.Right > bigRect.Right)
            {
                smallRect.Offset(bigRect.Right - smallRect.Right, 0);
            }

            if (smallRect.Top < bigRect.Top)
            {
                smallRect.Offset(0, bigRect.Top - smallRect.Top);
            }
            else if (smallRect.Bottom > bigRect.Bottom)
            {
                smallRect.Offset(0, bigRect.Bottom - smallRect.Bottom);
            }

            return smallRect;
        }
    }
}
