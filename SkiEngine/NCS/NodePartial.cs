using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using SkiEngine.Interfaces;
using SkiEngine.NCS.Component.Base;
using SkiEngine.Util;

namespace SkiEngine.NCS
{
    public partial class Node : IDestroyable<Node>
    {
        public event Action<Node> Destroyed;

        private readonly List<Node> _children = new List<Node>();
        private readonly HashSet<IComponent> _components = new HashSet<IComponent>(ReferenceEqualityComparer<IComponent>.Default);
        
        internal Node(Scene scene, InitialNodeTransform initialTransform)
        {
            Scene = scene;

            RelativePoint = initialTransform.RelativePoint;
            RelativeRotation = initialTransform.RelativeRotation;
            RelativeScale = initialTransform.RelativeScale;
        }

        public Node Parent { get; private set; }

        public Scene Scene { get; private set; }
        
        public bool IsDestroyed { get; private set; }

        public Node CreateChild()
        {
            return CreateChild(new InitialNodeTransform());
        }

        public Node CreateChild(SKPoint relativePoint)
        {
            return CreateChild(new InitialNodeTransform(relativePoint));
        }

        public Node CreateChild(SKPoint relativePoint, int relativeZ)
        {
            return CreateChild(new InitialNodeTransform(relativePoint, relativeZ));
        }

        public Node CreateChild(SKPoint relativePoint, int relativeZ, float relativeRotation)
        {
            return CreateChild(new InitialNodeTransform(relativePoint, relativeZ, relativeRotation));
        }

        public Node CreateChild(SKPoint relativePoint, int relativeZ, float relativeRotation, SKPoint relativeScale)
        {
            return CreateChild(new InitialNodeTransform(relativePoint, relativeZ, relativeRotation, relativeScale));
        }

        public Node CreateChild(InitialNodeTransform initialNodeTransform)
        {
            var child = new Node(Scene, initialNodeTransform);
            AddChild(child);
            child._worldZ = WorldZ + initialNodeTransform.RelativeZ;
            Scene.OnNodeCreated(child);
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
                child.Parent = null;
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

            Parent?.RemoveChild(this);

            foreach (var component in _components.ToArray())
            {
                component.Destroy();
            }

            foreach (var child in _children.ToArray())
            {
                child.Destroy();
            }

            Scene.OnNodeDestroyed(this);

            Destroyed?.Invoke(this);
        }
    }

    public class InitialNodeTransform
    {
        public SKPoint RelativePoint { get; set; }
        public float RelativeRotation { get; set; }
        public SKPoint RelativeScale { get; set; } = new SKPoint(1, 1);
        public int RelativeZ { get; set; }

        public InitialNodeTransform()
        {

        }

        public InitialNodeTransform(SKPoint relativePoint)
        {
            RelativePoint = relativePoint;
        }

        public InitialNodeTransform(SKPoint relativePoint, int relativeZ)
        {
            RelativePoint = relativePoint;
            RelativeZ = relativeZ;
        }

        public InitialNodeTransform(SKPoint relativePoint, int relativeZ, float relativeRotation)
        {
            RelativePoint = relativePoint;
            RelativeZ = relativeZ;
            RelativeRotation = relativeRotation;
        }

        public InitialNodeTransform(SKPoint relativePoint, int relativeZ, float relativeRotation, SKPoint relativeScale)
        {
            RelativePoint = relativePoint;
            RelativeZ = relativeZ;
            RelativeRotation = relativeRotation;
            RelativeScale = relativeScale;
        }
    }
}
