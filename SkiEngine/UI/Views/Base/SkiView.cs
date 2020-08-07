using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SkiaSharp;
using SkiEngine.Input;
using SkiEngine.UI.Gestures;
using SkiEngine.Util;

namespace SkiEngine.UI
{
    public abstract class SkiView
    {
        public SkiUiComponent UiComponent { get; internal set; }

        private Node _node;
        public Node Node
        {
            get => _node;
            internal set
            {
                if (_node == value)
                {
                    return;
                }

                _node?.Destroy();
                _node = value;
                OnNodeChanged();
            }
        }

        public LinkedProperty<SKSize> SizeProp { get; } = new LinkedProperty<SKSize>();
        public SKSize Size
        {
            get => SizeProp.Value;
            protected set => SizeProp.Value = value;
        }

        public SKRect WorldBounds => Node.LocalToWorldMatrix.MapRect(new SKRect(0, 0, Size.Width, Size.Height));
        public abstract IEnumerable<SkiView> ChildrenEnumerable { get; }

        public abstract bool ListensForPressedTouches { get; }
        public List<SkiGestureRecognizer> GestureRecognizers { get; } = new List<SkiGestureRecognizer>();

        public SKMatrix PixelToLocalMatrix => UiComponent.Camera.PixelToWorldMatrix.PostConcat(Node.WorldToLocalMatrix);

        public void UpdateChildNode(SkiView child, InitialNodeTransform transform = null)
        {
            if (Node != null)
            {
                child.UiComponent = UiComponent;
                child.Node = Node.CreateChild(transform ?? new InitialNodeTransform());
            }
        }

        protected abstract void OnNodeChanged();
        public abstract void Layout(float maxWidth, float maxHeight);
        protected abstract void DrawInternal(SKCanvas canvas);

        public void Draw(SKCanvas canvas)
        {
            var drawMatrix = Node.LocalToWorldMatrix.PostConcat(UiComponent.Camera.WorldToPixelMatrix);
            canvas.SetMatrix(drawMatrix);

            if (canvas.QuickReject(new SKRect(0, 0, Size.Width, Size.Height)))
            {
                return;
            }

            DrawInternal(canvas);
        }

        public void InvalidateSurface()
        {
            UiComponent?.InvalidateSurface();
        }

        public bool HitTest(SKPoint pointWorld)
        {
            var localPoint = Node.WorldToLocalMatrix.MapPoint(pointWorld);
            return new SKRect(0, 0, Size.Width, Size.Height).Contains(localPoint);
        }
    }

    public static class SkiViewExtensions
    {
        public static T Run<T>(this T view, Action<T> action) where T : SkiView
        {
            action(view);
            return view;
        }
    }
}
