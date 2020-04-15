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
    public class Scene
    {
        public event Action<Scene> Destroyed;

        private readonly List<ISystem> _systems = new List<ISystem>();
        private readonly List<IUpdateableSystem> _updateableSystems = new List<IUpdateableSystem>();

        private readonly UpdateTime _updateTime = new UpdateTime();
        private readonly Stopwatch _updateStopwatch = new Stopwatch();
        private TimeSpan _previousStopwatchElapsed = TimeSpan.Zero;
        private readonly TaskQueue _taskQueue = new TaskQueue();
        
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

        public Task RunAsync(Action action)
        {
            return _taskQueue.QueueAsync(action);
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

        public void Update()
        {
            var stopwatchElapsed = _updateStopwatch.Elapsed;
            _updateTime.Delta = stopwatchElapsed - _previousStopwatchElapsed;
            _previousStopwatchElapsed = stopwatchElapsed;

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

            if (_updateStopwatch.Elapsed.TotalHours >= 1)
            {
                _updateStopwatch.Restart();
                _previousStopwatchElapsed = TimeSpan.Zero;
            }
        }

        public async Task DestroyAsync()
        {
            if (IsDestroyed)
            {
                return;
            }

            IsDestroyed = true;

            await _taskQueue.ShutdownAsync();

            RootNode.Destroy();
            
            Destroyed?.Invoke(this);
        }
    }
}
