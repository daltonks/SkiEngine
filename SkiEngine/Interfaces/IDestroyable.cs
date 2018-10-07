using System;

namespace SkiEngine.Interfaces
{
    public interface IDestroyable<out T>
    {
        event Action<T> Destroyed;
        bool IsDestroyed { get; }

        void Destroy();
    }
}
