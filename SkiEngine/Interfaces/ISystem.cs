using SkiEngine.NCS.Component.Base;

namespace SkiEngine.Interfaces
{
    public interface ISystem : IUpdateable, IDrawable
    {
        void OnComponentCreated(IComponent component);
        void OnComponentDestroyed(IComponent component);
    }
}
