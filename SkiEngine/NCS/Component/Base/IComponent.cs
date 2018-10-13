using SkiEngine.Interfaces;

namespace SkiEngine.NCS.Component.Base
{
    public interface IComponent : IDestroyable<IComponent>
    {
        Node Node { get; }
        bool CreationHandled { get; set; }
    }
}
