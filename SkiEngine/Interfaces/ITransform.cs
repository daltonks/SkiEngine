using SkiaSharp;

namespace SkiEngine.Interfaces
{
    public interface ITransform
    {
        ref readonly SKMatrix WorldToLocalMatrix { get; }
        ref readonly SKMatrix LocalToWorldMatrix { get; }
    }
}
