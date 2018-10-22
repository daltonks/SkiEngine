using SkiaSharp;

namespace SkiEngine.Extensions
{
    public static class SKRectExtensions
    {
        public static SKPoint Mid(this SKRect rect)
        {
            return new SKPoint(rect.MidX, rect.MidY);
        }
    }
}
