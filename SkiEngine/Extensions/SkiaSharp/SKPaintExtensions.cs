using System;
using SkiaSharp;

// ReSharper disable InconsistentNaming
namespace SkiEngine.Extensions.SkiaSharp
{
    public static class SKPaintExtensions
    {
        public static MeasuredText MeasureTextImprovedPrecision(
            this SKPaint paint, 
            Func<SKPaint, SKRect> measureTextAction,
            bool centerHorizontally = false,
            bool centerVertically = false
        )
        {
            // Scale up TextSize to get a more accurate measurement.
            // It is inaccurate at small text sizes.
            var originalHeight = paint.TextSize;
            paint.TextSize = 100;
            var scaleToOriginal = originalHeight / 100;
            var bigTextBounds = measureTextAction.Invoke(paint);

            var width = bigTextBounds.Width * scaleToOriginal;
            
            var offset = new SKPoint();
            if (centerHorizontally)
            {
                offset.X = -width / 2;
            }

            if (centerVertically)
            {
                offset.Y = -originalHeight / 2;
            }

            var bounds = SKRect.Create(offset.X, offset.Y, width, originalHeight);
            var drawPoint = new SKPoint(
                bounds.Left - bigTextBounds.Left * scaleToOriginal, 
                bounds.Bottom - bigTextBounds.Bottom * scaleToOriginal
            );

            paint.TextSize = originalHeight;

            return new MeasuredText(bounds, drawPoint);
        }
    }

    public struct MeasuredText
    {
        public MeasuredText(SKRect rect, SKPoint drawPoint)
        {
            Rect = rect;
            DrawPoint = drawPoint;
        }

        public SKRect Rect { get; }
        public SKPoint DrawPoint { get; }
    }
}
