using System;

namespace SkiEngine.Util
{
    public interface IDestroyable<out T>
    {
        event Action<T> Destroyed;
        bool IsDestroyed { get; }

        void Destroy();
    }
}
