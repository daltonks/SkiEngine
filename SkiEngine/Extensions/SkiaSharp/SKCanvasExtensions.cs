using SkiaSharp;
using SkiEngine.NCS.Component.Sprite;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace SkiEngine.Extensions.SkiaSharp
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
