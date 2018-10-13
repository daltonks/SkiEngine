using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using SkiaSharp;
using SkiEngine.Interfaces;
using SkiEngine.NCS.Component.Base;
using SkiEngine.NCS.System;

namespace SkiEngine.NCS
{
    public class Scene : IDrawable, IDestroyable<Scene>
    {
        private readonly List<ISystem> _systems = new List<ISystem>();

        private readonly UpdateTime _updateTime = new UpdateTime();
        private readonly Stopwatch _updateStopwatch = new Stopwatch();
        private TimeSpan _previousStopwatchElapsed = TimeSpan.Zero;
        private readonly ConcurrentQueue<Action> _runNextUpdateActions = new ConcurrentQueue<Action>();

        public event Action<Scene> Destroyed;

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

        public void RunNextUpdate(Action action)
        {
            _runNextUpdateActions.Enqueue(action);
        }

        public void Update()
        {
            while(_runNextUpdateActions.TryDequeue(out var action))
            {
                action.Invoke();
            }

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
            foreach (var system in _systems)
            {
                system.Draw(canvas, viewTarget);
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
