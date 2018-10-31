using SkiEngine.NCS;
using SkiEngine.NCS.Component.Base;

namespace SkiEngine.Interfaces
{
    public interface ISystem
    {
        void OnNodeCreated(Node node);
        void OnNodeDestroyed(Node node);
        void OnComponentCreated(IComponent component);
        void OnComponentDestroyed(IComponent component);
    }
}
