using SkiaSharp;

namespace SkiEngine.Interfaces
{
    public interface ITransform
    {
        SKPoint WorldPoint { get; set; }
        double WorldRotation { get; set; }
        SKPoint WorldScale { get; set; }
    }
}
