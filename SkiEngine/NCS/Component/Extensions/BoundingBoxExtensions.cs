using SkiaSharp;
using SkiEngine.NCS.Component.Base;
using SkiEngine.Util;

namespace SkiEngine.NCS.Component.Extensions
{
    public static class BoundingBoxExtensions
    {
        public static bool BoundingBoxContains<T>(this T component, SKPoint worldPoint) where T : IComponent, ILocalBounds
        {
            var localPoint = component.Node.WorldToLocalMatrix.MapPoint(worldPoint);
            return component.LocalBounds.Contains(localPoint);
        }
    }
}
