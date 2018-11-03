﻿using SkiEngine.NCS;
using SkiEngine.NCS.Component.Base;

namespace SkiEngine.Interfaces
{
    public interface ISystem
    {
        void OnNodeZChanged(Node node, int previousZ);
        void OnComponentCreated(IComponent component);
        void OnComponentDestroyed(IComponent component);
    }
}
