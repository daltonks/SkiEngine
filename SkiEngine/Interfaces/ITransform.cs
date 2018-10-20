using SkiaSharp;

namespace SkiEngine.Interfaces
{
    public interface ITransform
    {
        SKPoint WorldPoint { get; set; }
        float WorldRotation { get; set; }
        SKPoint WorldScale { get; set; }
    }
}
