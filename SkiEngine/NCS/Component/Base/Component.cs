using System;

namespace SkiEngine.NCS.Component.Base
{
    public abstract class Component : IComponent
    {
        public event Action<IComponent> Destroyed;

        public Node Node { get; internal set; }
        public bool CreationHandled { get; set; }
        public bool IsDestroyed { get; private set; }

        public void Destroy()
        {
            if (IsDestroyed)
            {
                return;
            }

            IsDestroyed = true;

            Destroyed?.Invoke(this);

            Destroyed = null;
        }
    }
}
