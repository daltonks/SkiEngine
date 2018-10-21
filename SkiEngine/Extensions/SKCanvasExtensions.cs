using SkiaSharp;
using SkiEngine.Interfaces;
using SkiEngine.Sprite;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace SkiEngine.Extensions
{
    public static class SKCanvasExtensions
    {
        public static void DrawImage(
            this SKCanvas canvas,
            SKImage image, 
            SKPaint paint, 
            SpriteData spriteData
        )
        {
            var origin = spriteData.Origin;
            var textureBounds = spriteData.TextureBounds;
            canvas.DrawImage(
                image, 
                textureBounds, 
                SKRect.Create(
                    -origin.X, 
                    -origin.Y,
                    textureBounds.Width, 
                    textureBounds.Height
                ),
                paint
            );
        }
    }
}
