using System;
using SkiaSharp;

// ReSharper disable InconsistentNaming
namespace SkiEngine.Extensions.SkiaSharp
{
    public static class SKPaintExtensions
    {
        public static MeasuredText MeasureTextScaled(
            this SKPaint paint, 
            float drawHeight,
            float pixelHeight,
            Func<SKPaint, SKRect> measureTextAction,
            bool centerHorizontally = false,
            bool centerVertically = false
        )
        {
            paint.TextSize = pixelHeight;

            var drawUnitPerPixel = drawHeight / pixelHeight;
            var pixelBounds = measureTextAction.Invoke(paint);
            var drawWidth = pixelBounds.Width * drawUnitPerPixel;
            
            var drawOffset = new SKPoint();
            if (centerHorizontally)
            {
                drawOffset.X = -drawWidth / 2;
            }

            if (centerVertically)
            {
                drawOffset.Y = -drawHeight / 2;
            }

            var drawBounds = SKRect.Create(drawOffset.X, drawOffset.Y, drawWidth, drawHeight);
            var drawPoint = new SKPoint(
                drawBounds.Left - pixelBounds.Left * drawUnitPerPixel, 
                drawBounds.Bottom - pixelBounds.Bottom * drawUnitPerPixel
            );

            paint.TextSize = drawHeight;

            return new MeasuredText(drawBounds, drawPoint);
        }
    }

    public struct MeasuredText
    {
        public MeasuredText(SKRect drawBounds, SKPoint drawPoint)
        {
            DrawBounds = drawBounds;
            DrawPoint = drawPoint;
        }

        public SKRect DrawBounds { get; }
        public SKPoint DrawPoint { get; }
    }
}
