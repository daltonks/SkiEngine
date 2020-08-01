using SkiEngine.Util;

namespace SkiEngine.Component
{
    public interface IComponent : IDestroyable<IComponent>
    {
        Node.Node Node { get; }
        bool CreationHandled { get; set; }
    }
}
