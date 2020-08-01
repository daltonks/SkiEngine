using SkiaSharp;
using SkiEngine.Camera;
using SkiEngine.Component;

namespace SkiEngine.Drawable
{
    public interface IDrawableComponent : IComponent
    {
        void Draw(SKCanvas canvas, CameraComponent camera);
    }
}
