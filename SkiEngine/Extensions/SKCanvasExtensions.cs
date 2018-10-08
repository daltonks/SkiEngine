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
            SpriteData spriteData, 
            ITransform transform
        )
        {
            var rotated = transform.WorldRotation != 0;
            if (rotated)
            {
                canvas.Save();
                canvas.RotateRadians(transform.WorldRotation);
            }
            
            canvas.DrawImage(
                image, 
                spriteData.TextureBounds, 
                SKRect.Create(
                    transform.WorldPoint.X - spriteData.Origin.X * transform.WorldScale.X, 
                    transform.WorldPoint.Y - spriteData.Origin.Y * transform.WorldScale.Y,
                    spriteData.TextureBounds.Width * transform.WorldScale.X,
                    spriteData.TextureBounds.Height * transform.WorldScale.Y
                ),
                paint
            );

            if (rotated)
            {
                canvas.Restore();
            }
        }
    }
}
