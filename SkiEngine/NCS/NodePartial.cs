using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using SkiEngine.Extensions;
using SkiEngine.Interfaces;
using SkiEngine.NCS.Component.Base;
using SkiEngine.Util;

namespace SkiEngine.NCS
{
    public partial class Node : IDestroyable<Node>
    {
        public event Action<Node> Destroyed;

        private Node _parent;

        private readonly List<Node> _children = new List<Node>();
        
        private readonly HashSet<IComponent> _components = new HashSet<IComponent>(ReferenceEqualityComparer<IComponent>.Default);
        
        internal Node(Scene scene, SKPoint relativePoint, float relativeRotation, SKPoint relativeScale)
        {
            Scene = scene;

            RelativePoint = relativePoint;
            RelativeRotation = relativeRotation;
            RelativeScale = relativeScale;
        }

        public Scene Scene { get; private set; }
        
        public bool IsDestroyed { get; private set; }
        
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

            child.SetWorldTransformDirty();

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

            _components.Add(component);

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

            _parent?.RemoveChild(this);

            foreach (var component in _components.ToArray())
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
