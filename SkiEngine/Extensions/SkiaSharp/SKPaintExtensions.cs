using System;
using SkiaSharp;

// ReSharper disable InconsistentNaming
namespace SkiEngine.Extensions.SkiaSharp
{
    public static class SKPaintExtensions
    {
        public static BoundsAndTextDrawPoint MeasureTextScaled(
            this SKPaint paint, 
            float drawHeight,
            Func<float, float> getPixelHeight,
            Func<SKPaint, SKRect> measureTextAction,
            bool centerHorizontally = false,
            bool centerVertically = false
        )
        {
            var pixelHeight = getPixelHeight(drawHeight);
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

            return new BoundsAndTextDrawPoint(drawBounds, drawPoint);
        }
    }

    public struct BoundsAndTextDrawPoint
    {
        public BoundsAndTextDrawPoint(SKRect bounds, SKPoint textDrawPoint)
        {
            Bounds = bounds;
            TextDrawPoint = textDrawPoint;
        }

        public SKRect Bounds { get; set; }
        public SKPoint TextDrawPoint { get; set; }
    }
}
