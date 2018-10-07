using System;
using System.Collections.Generic;
using SkiaSharp;
using SkiEngine.Interfaces;
using SkiEngine.NCS.Component.Base;
using SkiEngine.NCS.System;

namespace SkiEngine.NCS
{
    public class Scene : IDestroyable<Scene>, IUpdateable, IDrawable
    {
        private readonly List<ISystem> _systems = new List<ISystem>();

        public SKCanvas Canvas { get; set; }
        public event Action<Scene> Destroyed;

        public Scene(SKCanvas canvas)
        {
            Canvas = canvas;

            RootNode = new Node { Scene = this };

            AddSystem(new InputSystem());
            AddSystem(new UpdateSystem());
            AddSystem(new CameraSystem(this));
        }

        public Node RootNode { get; }

        public bool IsDestroyed { get; private set; }

        public void AddSystem(ISystem system)
        {
            _systems.Add(system);
        }

        public void RemoveSystem(ISystem system)
        {
            _systems.Remove(system);
        }

        public void OnComponentCreated(IComponent component)
        {
            foreach (var system in _systems)
            {
                system.OnComponentCreated(component);
            }
        }

        public void OnComponentDestroyed(IComponent component)
        {
            foreach (var system in _systems)
            {
                system.OnComponentDestroyed(component);
            }
        }

        public void Update(UpdateTime updateTime)
        {
            foreach (var system in _systems)
            {
                system.Update(updateTime);
            }
        }

        public void Draw(UpdateTime updateTime)
        {
            foreach (var system in _systems)
            {
                system.Draw(updateTime);
            }
        }

        public void Destroy()
        {
            if (IsDestroyed)
            {
                return;
            }

            IsDestroyed = true;

            RootNode.Destroy();

            Destroyed?.Invoke(this);
        }
    }
}
