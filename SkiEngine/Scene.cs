using System;
using System.Collections.Generic;
using System.Diagnostics;
using SkiEngine.Canvas;
using SkiEngine.Input;
using SkiEngine.UI;
using SkiEngine.Updateable;

namespace SkiEngine
{
    public class Scene
    {
        public event Action<Scene> Destroyed;

        private readonly List<ISystem> _systems = new List<ISystem>();
        private readonly List<IUpdateableSystem> _updateableSystems = new List<IUpdateableSystem>();

        private readonly UpdateTime _updateTime = new UpdateTime();
        private readonly Stopwatch _updateStopwatch = new Stopwatch();
        private TimeSpan _previousStopwatchElapsed = TimeSpan.Zero;
        
        public Scene()
        {
            RootNode = new Node(this, new InitialNodeTransform());

            AddSystem(UpdateSystem);
            AddSystem(CanvasSystem);
            AddSystem(UiSystem);
        }

        internal volatile int NumberNodes;
        public int NumberOfNodes => NumberNodes;
        public Node RootNode { get; }
        public bool IsDestroyed { get; private set; }

        public UpdateSystem UpdateSystem { get; } = new UpdateSystem();
        public CanvasSystem CanvasSystem { get; } = new CanvasSystem();
        public UiSystem UiSystem { get; } = new UiSystem();

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

        internal void OnNodeZChanged(Node node, float previousZ)
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

        public void OnTouch(SkiTouch touch)
        {
            UiSystem.OnTouch(touch);
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
