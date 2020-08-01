using System;
using SkiaSharp;

// ReSharper disable InconsistentNaming
namespace SkiEngine.Util.Extensions.SkiaSharp
{
    public static class SKPaintExtensions
    {
        public static BoundsAndTextDrawPoint MeasureTextScaled(
            this SKPaint paint, 
            float drawHeight,
            Func<SKPaint, SKRect> measureTextAction,
            bool centerHorizontally = false,
            bool centerVertically = false
        )
        {
            const int pixelHeight = 100;
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

            var bounds = SKRect.Create(drawOffset.X, drawOffset.Y, drawWidth, drawHeight);
            var textDrawPoint = new SKPoint(
                bounds.Left - pixelBounds.Left * drawUnitPerPixel, 
                bounds.Bottom - pixelBounds.Bottom * drawUnitPerPixel
            );

            paint.TextSize = drawHeight;

            return new BoundsAndTextDrawPoint(bounds, textDrawPoint);
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
