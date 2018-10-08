using SkiaSharp;
using SkiEngine.NCS.Component.Base;

namespace SkiEngine.Interfaces
{
    public interface ISystem : IUpdateable
    {
        void OnComponentCreated(IComponent component);
        void OnComponentDestroyed(IComponent component);
        void Draw(SKCanvas canvas, int viewTarget);
    }
}
