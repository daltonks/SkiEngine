using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using SkiaSharp;
using SkiEngine.Interfaces;
using SkiEngine.NCS.Component.Base;
using SkiEngine.NCS.System;

namespace SkiEngine.NCS
{
    public class Scene : IDrawable, IDestroyable<Scene>
    {
        private readonly List<ISystem> _systems = new List<ISystem>();
        private readonly List<IUpdateable> _updateableSystems = new List<IUpdateable>();
        private readonly List<IDrawable> _drawableSystems = new List<IDrawable>();

        private readonly UpdateTime _updateTime = new UpdateTime();
        private readonly Stopwatch _updateStopwatch = new Stopwatch();
        private TimeSpan _previousStopwatchElapsed = TimeSpan.Zero;
        private readonly ConcurrentQueue<Action> _runNextUpdateActions = new ConcurrentQueue<Action>();
        private readonly ReaderWriterLockSlim _updateReaderWriterLock = new ReaderWriterLockSlim();

        public event Action<Scene> Destroyed;

        public Scene()
        {
            RootNode = new Node(this, SKPoint.Empty, 0, new SKPoint(1, 1));

            AddSystem(new InputSystem());
            AddSystem(new UpdateSystem());
            AddSystem(new CameraSystem());
        }

        public void Start()
        {
            OnNodeCreated(RootNode);

            _updateStopwatch.Start();
        }

        public Node RootNode { get; }

        public bool IsDestroyed { get; private set; }

        public void AddSystem(ISystem system)
        {
            _systems.Add(system);

            if (system is IUpdateable updateableSystem)
            {
                _updateableSystems.Add(updateableSystem);
            }

            if (system is IDrawable drawableSystem)
            {
                _drawableSystems.Add(drawableSystem);
            }
        }

        public void RemoveSystem(ISystem system)
        {
            _systems.Remove(system);

            if (system is IUpdateable updateableSystem)
            {
                _updateableSystems.Remove(updateableSystem);
            }

            if (system is IDrawable drawableSystem)
            {
                _drawableSystems.Remove(drawableSystem);
            }
        }

        internal void OnNodeCreated(Node node)
        {
            foreach (var system in _systems)
            {
                system.OnNodeCreated(node);
            }
        }

        internal void OnNodeZChanged(Node node, int previousZ)
        {
            foreach (var system in _systems)
            {
                system.OnNodeZChanged(node, previousZ);
            }
        }

        internal void OnNodeDestroyed(Node node)
        {
            foreach (var system in _systems)
            {
                system.OnNodeDestroyed(node);
            }
        }

        internal void OnComponentCreated(IComponent component)
        {
            foreach (var system in _systems)
            {
                system.OnComponentCreated(component);
            }
        }

        internal void OnComponentDestroyed(IComponent component)
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
            var stopwatchElapsed = _updateStopwatch.Elapsed;
            _updateTime.Delta = stopwatchElapsed - _previousStopwatchElapsed;
            _previousStopwatchElapsed = stopwatchElapsed;

            _updateReaderWriterLock.EnterWriteLock();
            try
            {
                while (_runNextUpdateActions.TryDequeue(out var action))
                {
                    action.Invoke();
                }

                foreach (var system in _updateableSystems)
                {
                    system.Update(_updateTime);
                }
            }
            finally
            {
                _updateReaderWriterLock.ExitWriteLock();
            }

            if (_updateStopwatch.Elapsed.TotalHours >= 1)
            {
                _updateStopwatch.Reset();
                _previousStopwatchElapsed = TimeSpan.Zero;
            }
        }

        public void Draw(SKCanvas canvas, int viewTarget)
        {
            _updateReaderWriterLock.EnterReadLock();
            try
            {
                foreach (var system in _drawableSystems)
                {
                    system.Draw(canvas, viewTarget);
                }
            }
            finally
            {
                _updateReaderWriterLock.ExitReadLock();
            }
        }

        public void Destroy()
        {
            if (IsDestroyed)
            {
                return;
            }

            IsDestroyed = true;

            _updateReaderWriterLock.Dispose();

            RootNode.Destroy();

            Destroyed?.Invoke(this);
        }
    }
}
