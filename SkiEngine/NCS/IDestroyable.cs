using System;

namespace SkiEngine.NCS
{
    public interface IDestroyable<out T>
    {
        event Action<T> Destroyed;
        bool IsDestroyed { get; }

        void Destroy();
    }
}
