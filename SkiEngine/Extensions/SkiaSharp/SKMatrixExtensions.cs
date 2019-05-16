using SkiaSharp;

namespace SkiEngine.Extensions.SkiaSharp
{
    // ReSharper disable once InconsistentNaming
    public static class SKMatrixExtensions
    {
        public static SKPoint MapVector(this SKMatrix matrix, SKPoint point)
        {
            return matrix.MapVector(point.X, point.Y);
        }
    }
}
