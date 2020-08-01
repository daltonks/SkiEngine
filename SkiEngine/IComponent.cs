using SkiEngine.Util;

namespace SkiEngine
{
    public interface IComponent : IDestroyable<IComponent>
    {
        Node Node { get; }
        bool CreationHandled { get; set; }
    }
}
