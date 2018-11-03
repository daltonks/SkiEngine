using SkiaSharp;
using SkiEngine.Interfaces;

namespace SkiEngine.NCS.Component.Base
{
    public interface IDrawableComponent : IComponent
    {
        void Draw(SKCanvas canvas, ITransform transform);
    }
}
