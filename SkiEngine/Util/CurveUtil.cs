using SkiaSharp;
using SkiEngine.Util.Extensions.SkiaSharp;

namespace SkiEngine.Util
{
    public static class CurveUtil
    {
        private const double TwoThirds = 2.0 / 3.0;

        public static (SKPoint p1, SKPoint p2) QuadToCubic(SKPoint start, SKPoint control, SKPoint end)
        {
            return (
                start + (control - start).Multiply(TwoThirds), 
                end + (control - end).Multiply(TwoThirds)
            );
        }
    }
}
