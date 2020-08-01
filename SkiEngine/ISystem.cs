using SkiEngine.Component;

namespace SkiEngine
{
    public interface ISystem
    {
        void OnNodeZChanged(Node.Node node, float previousZ);
        void OnComponentCreated(IComponent component);
        void OnComponentDestroyed(IComponent component);
    }
}
