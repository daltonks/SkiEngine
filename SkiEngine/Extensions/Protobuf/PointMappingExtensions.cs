using SkiaSharp;
using SkiEngine.Protobuf;

namespace SkiEngine.Extensions.Protobuf
{
    public static class PointMappingExtensions
    {
        public static SKPoint ToSKPoint(this PPoint pPoint)
        {
            return new SKPoint(pPoint.X, pPoint.Y);
        }

        public static PPoint ToPPoint(this SKPoint skPoint)
        {
            return new PPoint
            {
                X = skPoint.X,
                Y = skPoint.Y
            };
        }
    }
}
