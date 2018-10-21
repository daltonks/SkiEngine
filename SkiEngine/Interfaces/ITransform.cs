using SkiaSharp;

namespace SkiEngine.Interfaces
{
    public interface ITransform
    {
        ref SKMatrix WorldToLocalMatrix { get; }
        ref SKMatrix LocalToWorldMatrix { get; }
    }
}
