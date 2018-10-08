﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using SkiaSharp;
using SkiEngine.Interfaces;
using SkiEngine.NCS.Component.Base;
using SkiEngine.NCS.System;

namespace SkiEngine.NCS
{
    public class Scene : IDestroyable<Scene>
    {
        private readonly List<ISystem> _systems = new List<ISystem>();

        private readonly UpdateTime _updateTime = new UpdateTime();
        private readonly Stopwatch _updateStopwatch = new Stopwatch();
        private TimeSpan _previousStopwatchElapsed = TimeSpan.Zero;

        public event Action<Scene> Destroyed;
        public SKColor ClearColor { get; set; }

        public Scene()
        {
            RootNode = new Node(this, SKPoint.Empty, 0, new SKPoint(1, 1));

            AddSystem(new InputSystem());
            AddSystem(new UpdateSystem());
            AddSystem(new CameraSystem());

            _updateStopwatch.Start();
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

        public void Update()
        {
            var stopwatchElapsed = _updateStopwatch.Elapsed;
            _updateTime.Delta = stopwatchElapsed - _previousStopwatchElapsed;
            _previousStopwatchElapsed = stopwatchElapsed;

            foreach (var system in _systems)
            {
                system.Update(_updateTime);
            }

            if (_updateStopwatch.Elapsed.TotalHours >= 1)
            {
                _updateStopwatch.Reset();
                _previousStopwatchElapsed = TimeSpan.Zero;
            }
        }

        public void Draw(SKCanvas canvas, int viewTarget)
        {
            canvas.Clear(ClearColor);

            foreach (var system in _systems)
            {
                system.Draw(canvas, viewTarget);
            }

            canvas.Flush();
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
