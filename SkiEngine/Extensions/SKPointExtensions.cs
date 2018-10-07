using System;
using SkiaSharp;

namespace SkiEngine.Extensions
{
    public static class SKPointExtensions
    {
        public static SKPoint ToLength(this SKPoint point, double length)
        {
            point = point == SKPoint.Empty
                ? new SKPoint(1, 0)
                : point.Normalized();

            return point.Multiply(length);
        }

        public static SKPoint Normalized(this SKPoint point)
        {
            if (point == SKPoint.Empty)
            {
                return new SKPoint(0, 0);
            }

            var length = point.Length();
            return new SKPoint((float) (point.X / length), (float) (point.Y / length));
        }

        public static double Length(this SKPoint point)
        {
            return Math.Sqrt(point.X * point.X + point.Y * point.Y);
        }

        public static SKPoint Multiply(this SKPoint point, double scalar)
        {
            return new SKPoint((float) (point.X * scalar), (float) (point.Y * scalar));
        }

        public static SKPoint Multiply(this SKPoint p1, SKPoint p2)
        {
            return new SKPoint(p1.X * p2.X, p1.Y * p2.Y);
        }
        
        public static SKPoint VectorTo(this SKPoint p1, SKPoint p2)
        {
            return p2 - p1;
        }

        public static SKPoint Rotate(this SKPoint point, float radians)
        {
            var cos = Math.Cos(radians);
            var sin = Math.Sin(radians);

            return new SKPoint(
                (float) (point.X * cos - point.Y * sin),
                (float) (point.X * sin + point.Y * cos)
            );
        }
    }
}
