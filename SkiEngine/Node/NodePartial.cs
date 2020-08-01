using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SkiaSharp;
using SkiEngine.Component;
using SkiEngine.Util;

namespace SkiEngine.Node
{
    public sealed partial class Node : IDestroyable<Node>
    {
        public event Action<Node> Destroyed;

        private readonly List<Node> _children = new List<Node>();
        private readonly HashSet<IComponent> _components = new HashSet<IComponent>(ReferenceEqualityComparer<IComponent>.Default);
        
        internal Node(Scene scene, InitialNodeTransform initialTransform)
        {
            Scene = scene;
            Interlocked.Increment(ref scene.NumberNodes);

            RelativePoint = initialTransform.RelativePoint;
            RelativeRotation = initialTransform.RelativeRotation;
            RelativeScale = initialTransform.RelativeScale;
        }

        public Node Parent { get; private set; }
        public Scene Scene { get; private set; }
        public IReadOnlyList<Node> Children => _children;
        public IReadOnlyCollection<IComponent> Components => _components;
        
        public bool IsDestroyed { get; private set; }

        public Node CreateChild()
        {
            return CreateChild(new InitialNodeTransform());
        }

        public Node CreateChild(SKPoint relativePoint)
        {
            return CreateChild(new InitialNodeTransform(relativePoint));
        }

        public Node CreateChild(SKPoint relativePoint, float relativeZ)
        {
            return CreateChild(new InitialNodeTransform(relativePoint, relativeZ));
        }

        public Node CreateChild(SKPoint relativePoint, float relativeZ, float relativeRotation)
        {
            return CreateChild(new InitialNodeTransform(relativePoint, relativeZ, relativeRotation));
        }

        public Node CreateChild(SKPoint relativePoint, float relativeZ, float relativeRotation, SKPoint relativeScale)
        {
            return CreateChild(new InitialNodeTransform(relativePoint, relativeZ, relativeRotation, relativeScale));
        }

        public Node CreateChild(InitialNodeTransform initialNodeTransform)
        {
            var child = new Node(Scene, initialNodeTransform);
            AddChild(child);
            child._worldZ = WorldZ + initialNodeTransform.RelativeZ;
            return child;
        }
        
        public void AddChild(Node child)
        {
            if (child.Parent == this)
            {
                return;
            }

            var childHadParentPreviously = child.Parent != null;
            child.Parent?.RemoveChild(child);
            
            _children.Add(child);

            var previousRelativeZ = child.RelativeZ;

            child.Parent = this;

            child.RelativeZ = previousRelativeZ;

            child.SetMatricesDirty();

            if (!childHadParentPreviously)
            {
                HandleComponentCreationRecursively(child);
            }
        }

        private void HandleComponentCreationRecursively(Node node)
        {
            node.Scene = Scene;

            foreach (var childComponent in node._components)
            {
                Scene.OnComponentCreated(childComponent);
            }

            foreach (var childNode in node._children)
            {
                HandleComponentCreationRecursively(childNode);
            }
        }

        private void RemoveChild(Node child)
        {
            if (_children.Remove(child))
            {
                child.Parent = null;
            }
        }

        public TComponent AddComponent<TComponent>(TComponent component) where TComponent : Component.Component
        {
            if (component.Node == this)
            {
                return component;
            }

            component.Node?.RemoveComponent(component);

            _components.Add(component);

            component.Node = this;

            component.Destroyed += OnComponentDestroyed;

            Scene.OnComponentCreated(component);

            return component;
        }

        private void RemoveComponent(IComponent component)
        {
            if (!_components.Remove(component))
            {
                return;
            }

            component.Destroyed -= OnComponentDestroyed;
        }

        private void OnComponentDestroyed(IComponent component)
        {
            RemoveComponent(component);
            Scene.OnComponentDestroyed(component);
        }

        public void Destroy()
        {
            if (IsDestroyed)
            {
                return;
            }

            IsDestroyed = true;

            Parent?.RemoveChild(this);

            foreach (var component in _components.ToArray())
            {
                component.Destroy();
            }

            foreach (var child in _children.ToArray())
            {
                child.Destroy();
            }

            Destroyed?.Invoke(this);

            Interlocked.Decrement(ref Scene.NumberNodes);
        }
    }

    public class InitialNodeTransform
    {
        public SKPoint RelativePoint { get; set; }
        public float RelativeRotation { get; set; }
        public SKPoint RelativeScale { get; set; } = new SKPoint(1, 1);
        public float RelativeZ { get; set; }

        public InitialNodeTransform()
        {

        }

        public InitialNodeTransform(SKPoint relativePoint)
        {
            RelativePoint = relativePoint;
        }

        public InitialNodeTransform(SKPoint relativePoint, float relativeZ)
        {
            RelativePoint = relativePoint;
            RelativeZ = relativeZ;
        }

        public InitialNodeTransform(SKPoint relativePoint, float relativeZ, float relativeRotation)
        {
            RelativePoint = relativePoint;
            RelativeZ = relativeZ;
            RelativeRotation = relativeRotation;
        }

        public InitialNodeTransform(SKPoint relativePoint, float relativeZ, float relativeRotation, SKPoint relativeScale)
        {
            RelativePoint = relativePoint;
            RelativeZ = relativeZ;
            RelativeRotation = relativeRotation;
            RelativeScale = relativeScale;
        }
    }
}
