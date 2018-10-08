using System;
using System.Collections.Generic;
using SkiaSharp;
using SkiEngine.Extensions;
using SkiEngine.Interfaces;
using SkiEngine.NCS.Component.Base;

namespace SkiEngine.NCS
{
    public class Node : IDestroyable<Node>, ITransform
    {
        private Node _parent;

        private readonly List<Node> _children = new List<Node>();

        private SKPoint _relativePoint;
        private float _relativeRotation;
        private SKPoint _relativeScale;

        internal readonly List<IComponent> Components = new List<IComponent>();

        public event Action<Node> Destroyed;
        public Scene Scene { get; private set; }

        internal Node(Scene scene, SKPoint relativePoint, float relativeRotation, SKPoint relativeScale)
        {
            Scene = scene;
            _relativePoint = relativePoint;
            _relativeRotation = relativeRotation;
            _relativeScale = relativeScale;
            RecalculateWorldTransform();
        }

        public SKPoint RelativePoint
        {
            get => _relativePoint;
            set
            {
                _relativePoint = value;
                RecalculateWorldTransform();
            }
        }

        public float RelativeRotation
        {
            get => _relativeRotation;
            set
            {
                _relativeRotation = value;
                RecalculateWorldTransform();
            }
        }

        public SKPoint RelativeScale
        {
            get => _relativeScale;
            set
            {
                _relativeScale = value;
                RecalculateWorldTransform();
            }
        }

        public SKPoint WorldPoint { get; private set; }
        public float WorldRotation { get; private set; }
        public SKPoint WorldScale { get; private set; }

        public bool IsDestroyed { get; private set; }

        private void RecalculateWorldTransform()
        {
            if (_parent == null)
            {
                WorldRotation = RelativeRotation;
                WorldScale = RelativeScale;
                WorldPoint = RelativePoint;
            }
            else
            {
                WorldRotation = _parent.WorldRotation + RelativeRotation;
                WorldScale = _parent.WorldScale.Multiply(RelativeScale);
                WorldPoint = _parent.WorldPoint + RelativePoint.Multiply(_parent.WorldScale).Rotate(_parent.WorldRotation);
            }

            foreach (var child in _children)
            {
                child.RecalculateWorldTransform();
            }
        }

        public Node CreateChild()
        {
            return CreateChild(new SKPoint(0, 0), 0, new SKPoint(1, 1));
        }

        public Node CreateChild(SKPoint relativePoint)
        {
            return CreateChild(relativePoint, 0, new SKPoint(1, 1));
        }

        public Node CreateChild(SKPoint relativePoint, float relativeRotation)
        {
            return CreateChild(relativePoint, relativeRotation, new SKPoint(1, 1));
        }

        public Node CreateChild(SKPoint relativePoint, float relativeRotation, SKPoint relativeScale)
        {
            var child = new Node(Scene, relativePoint, relativeRotation, relativeScale);
            AddChild(child);
            return child;
        }
        
        public void AddChild(Node child)
        {
            if (child._parent == this)
            {
                return;
            }

            var childHadParentPreviously = child._parent != null;
            child._parent?.RemoveChild(child);
            
            _children.Add(child);
            
            child._parent = this;

            child.RecalculateWorldTransform();

            if (!childHadParentPreviously)
            {
                HandleComponentCreationRecursively(child);
            }
        }

        private void HandleComponentCreationRecursively(Node node)
        {
            node.Scene = Scene;

            foreach (var childComponent in node.Components)
            {
                if (!childComponent.CreationHandled)
                {
                    Scene.OnComponentCreated(childComponent);
                    childComponent.CreationHandled = true;
                }
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
                child._parent = null;
            }
        }

        public void AddComponent(Component.Base.Component component)
        {
            if (component.Node == this)
            {
                return;
            }

            component.Node?.RemoveComponent(component);

            Components.Add(component);

            component.Node = this;

            component.Destroyed += OnComponentDestroyed;

            if(Scene != null && !component.CreationHandled)
            {
                Scene.OnComponentCreated(component);
                component.CreationHandled = true;
            }
        }

        private void RemoveComponent(IComponent component)
        {
            if (!Components.Remove(component))
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

            _parent?.RemoveChild(this);

            foreach (var component in Components.ToArray())
            {
                component.Destroy();
            }

            foreach (var child in _children.ToArray())
            {
                child.Destroy();
            }

            Destroyed?.Invoke(this);
        }
    }
}
