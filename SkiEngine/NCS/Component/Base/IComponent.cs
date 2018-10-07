using System;

namespace SkiEngine.NCS.Component.Base
{
    public interface IComponent
    {
        event Action<IComponent> Destroyed;

        Node Node { get; }
        bool CreationHandled { get; set; }
        bool IsDestroyed { get; }

        void Destroy();
    }
}
