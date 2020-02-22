using SkiaSharp;

namespace SkiEngine.Util
{
    public interface ILocalBoundingBox
    {
        ref SKRect LocalBoundingBox { get; }
    }
}
