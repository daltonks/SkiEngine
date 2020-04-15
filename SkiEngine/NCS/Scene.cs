using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly object _lock = new object();
        
        public Scene()
        {
            RootNode = new Node(this, new InitialNodeTransform());

            AddSystem(new UpdateSystem());
            AddSystem(new CanvasSystem());
        }

        internal volatile int NumberNodes;
        public int NumberOfNodes => NumberNodes;

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

        private readonly TaskQueue _actionTaskQueue = new TaskQueue();
        private readonly ConcurrentQueue<Action> _actionQueue = new ConcurrentQueue<Action>();
        public Task RunAsync(Action action)
        {
            _actionQueue.Enqueue(action);

            return _actionTaskQueue.QueueAsync(() => {
                if (!_actionQueue.TryDequeue(out var a))
                {
                    return;
                }

                lock(_lock)
                {
                    if (IsDestroyed)
                    {
                        return;
                    }

                    a.Invoke();

                    while (_actionQueue.TryDequeue(out a))
                    {
                        a.Invoke();
                    }
                }
            });
        }

        public void Update()
        {
            var stopwatchElapsed = _updateStopwatch.Elapsed;
            _updateTime.Delta = stopwatchElapsed - _previousStopwatchElapsed;
            _previousStopwatchElapsed = stopwatchElapsed;
            
            lock(_lock)
            {
                try
                {
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
            }
            
            if (_updateStopwatch.Elapsed.TotalHours >= 1)
            {
                _updateStopwatch.Restart();
                _previousStopwatchElapsed = TimeSpan.Zero;
            }
        }

        public void Draw(Action drawAction)
        {
            lock(_lock)
            {
                drawAction();
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
