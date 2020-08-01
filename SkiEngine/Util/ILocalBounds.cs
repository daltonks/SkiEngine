using SkiaSharp;

namespace SkiEngine.Util
{
    public interface ILocalBounds
    {
        ref SKRect LocalBounds { get; }
    }
}
