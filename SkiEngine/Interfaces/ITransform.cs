using SkiaSharp;

namespace SkiEngine.Interfaces
{
    public interface ITransform
    {
        SKPoint WorldPoint { get; }
        float WorldRotation { get; }
        SKPoint WorldScale { get; }
    }
}
