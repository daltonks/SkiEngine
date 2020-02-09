using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using SkiEngine.NCS.Component;
using SkiEngine.NCS.Component.Base;
using SkiEngine.NCS.System;
using SkiEngine.Util;

namespace SkiEngine.NCS
{
    public class Scene : IDestroyable<Scene>
    {
        public event Action<Scene> Destroyed;

        private readonly List<ISystem> _systems = new List<ISystem>();
        private readonly List<IUpdateableSystem> _updateableSystems = new List<IUpdateableSystem>();

        private readonly UpdateTime _updateTime = new UpdateTime();
        private readonly Stopwatch _updateStopwatch = new Stopwatch();
        private TimeSpan _previousStopwatchElapsed = TimeSpan.Zero;
        private readonly ConcurrentQueue<Action> _runDuringUpdateActions = new ConcurrentQueue<Action>();
        private readonly ReaderWriterLockSlim _updateReaderWriterLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        
        public Scene()
        {
            RootNode = new Node(this, new InitialNodeTransform());

            AddSystem(new UpdateSystem());
            AddSystem(new CanvasSystem());
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
        }

        public void RemoveSystem(ISystem system)
        {
            _systems.Remove(system);

            if (system is IUpdateableSystem updateableSystem)
            {
                _updateableSystems.Remove(updateableSystem);
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
            if (component.CreationHandled)
            {
                return;
            }

            component.CreationHandled = true;

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

        public void RunDuringUpdate(Action action)
        {
            _runDuringUpdateActions.Enqueue(action);
        }

        private readonly TaskQueue _outsideOfUpdateTaskQueue = new TaskQueue();
        private readonly ConcurrentQueue<Action> _outsideOfUpdateActionQueue = new ConcurrentQueue<Action>();
        public Task QueueOutsideOfUpdateAsync(Action action)
        {
            _outsideOfUpdateActionQueue.Enqueue(action);

            return _outsideOfUpdateTaskQueue.QueueAsync(() => {
                if (!_outsideOfUpdateActionQueue.TryDequeue(out var a))
                {
                    return;
                }

                _updateReaderWriterLock.EnterWriteLock();
                try
                {
                    if (IsDestroyed)
                    {
                        return;
                    }

                    a.Invoke();

                    while (_outsideOfUpdateActionQueue.TryDequeue(out a))
                    {
                        a.Invoke();
                    }
                }
                finally
                {
                    _updateReaderWriterLock.ExitWriteLock();
                }
            });
        }

        public void Update()
        {
            var stopwatchElapsed = _updateStopwatch.Elapsed;
            _updateTime.Delta = stopwatchElapsed - _previousStopwatchElapsed;
            _previousStopwatchElapsed = stopwatchElapsed;
            
            _updateReaderWriterLock.EnterWriteLock();
            try
            {
                while (_runDuringUpdateActions.TryDequeue(out var action))
                {
                    action.Invoke();
                }

                foreach (var system in _updateableSystems)
                {
                    system.Update(_updateTime);
                }

                while (_runDuringUpdateActions.TryDequeue(out var action))
                {
                    action.Invoke();
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

        public void Draw(Action drawAction)
        {
            _updateReaderWriterLock.EnterReadLock();
            try
            {
                drawAction();
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

            RootNode.Destroy();

            Destroyed?.Invoke(this);

            _updateReaderWriterLock.Dispose();
        }
    }
}
