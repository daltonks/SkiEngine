using SkiaSharp;
using SkiEngine.Camera;

namespace SkiEngine.Drawable
{
    public interface IDrawableComponent : IComponent
    {
        void Draw(SKCanvas canvas, CameraComponent camera);
    }
}
