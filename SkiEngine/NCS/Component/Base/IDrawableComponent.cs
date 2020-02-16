using SkiaSharp;
using SkiEngine.NCS.Component.Camera;

namespace SkiEngine.NCS.Component.Base
{
    public interface IDrawableComponent : IComponent
    {
        void Draw(SKCanvas canvas, CameraComponent camera);
    }
}
