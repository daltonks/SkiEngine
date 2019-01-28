using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using SkiaSharp;
using SkiEngine.NCS.Component.Base;
using SkiEngine.NCS.System;

namespace SkiEngine.NCS
{
    public class Scene : IDestroyable<Scene>
    {
        public event Action<Scene> Destroyed;

        private readonly List<ISystem> _systems = new List<ISystem>();
        private readonly List<IUpdateableSystem> _updateableSystems = new List<IUpdateableSystem>();
        private readonly List<IDrawableSystem> _drawableSystems = new List<IDrawableSystem>();

        private readonly UpdateTime _updateTime = new UpdateTime();
        private readonly Stopwatch _updateStopwatch = new Stopwatch();
        private TimeSpan _previousStopwatchElapsed = TimeSpan.Zero;
        private readonly ConcurrentQueue<Action> _runNextUpdateActions = new ConcurrentQueue<Action>();
        private readonly ReaderWriterLockSlim _updateReaderWriterLock = new ReaderWriterLockSlim();
        
        public Scene()
        {
            RootNode = new Node(this, new InitialNodeTransform());

            AddSystem(new InputSystem());
            AddSystem(new UpdateSystem());
            AddSystem(new CameraSystem());
        }

        public Node RootNode { get; }

        public bool IsDestroyed { get; private set; }

        public IReadOnlyList<ISystem> Systems => _systems;

        public void Start()
        {
            _updateStopwatch.Start();
        }
        
        public void AddSystem(ISystem system)
        {
            _systems.Add(system);

            if (system is IUpdateableSystem updateableSystem)
            {
                _updateableSystems.Add(updateableSystem);
            }

            if (system is IDrawableSystem drawableSystem)
            {
                _drawableSystems.Add(drawableSystem);
            }
        }

        public void RemoveSystem(ISystem system)
        {
            _systems.Remove(system);

            if (system is IUpdateableSystem updateableSystem)
            {
                _updateableSystems.Remove(updateableSystem);
            }

            if (system is IDrawableSystem drawableSystem)
            {
                _drawableSystems.Remove(drawableSystem);
            }
        }

        internal void OnNodeZChanged(Node node, int previousZ)
        {
            foreach (var system in _systems)
            {
                system.OnNodeZChanged(node, previousZ);
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
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                throw;
            }
            finally
            {
                _updateReaderWriterLock.ExitWriteLock();
            }

            if (_updateStopwatch.Elapsed.TotalHours >= 1)
            {
                _updateStopwatch.Restart();
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
