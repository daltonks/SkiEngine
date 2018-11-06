using SkiaSharp;

namespace SkiEngine.NCS.Component.Base
{
    public interface IDrawableComponent : IComponent
    {
        void Draw(SKCanvas canvas);
    }
}
