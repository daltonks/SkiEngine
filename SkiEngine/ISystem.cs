﻿namespace SkiEngine
{
    public interface ISystem
    {
        void OnNodeZChanged(Node node, float previousZ);
        void OnComponentCreated(IComponent component);
        void OnComponentDestroyed(IComponent component);
    }
}
