using System;
using SkiaSharp;

// ReSharper disable InconsistentNaming
namespace SkiEngine.Extensions.SkiaSharp
{
    public static class SKPaintExtensions
    {
        public static MeasuredText MeasureTextScaled(
            this SKPaint paint, 
            float pixelHeight,
            Func<SKPaint, SKRect> measureTextAction,
            bool centerHorizontally = false,
            bool centerVertically = false
        )
        {
            var height = paint.TextSize;
            paint.TextSize = pixelHeight;

            var scale = height / pixelHeight;
            var pixelBounds = measureTextAction.Invoke(paint);

            var width = pixelBounds.Width * scale;
            
            var offset = new SKPoint();
            if (centerHorizontally)
            {
                offset.X = -width / 2;
            }

            if (centerVertically)
            {
                offset.Y = -height / 2;
            }

            var bounds = SKRect.Create(offset.X, offset.Y, width, height);
            var drawPoint = new SKPoint(
                bounds.Left - pixelBounds.Left * scale, 
                bounds.Bottom - pixelBounds.Bottom * scale
            );

            paint.TextSize = height;

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
