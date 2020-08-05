using System;
using System.Collections.Generic;
using SkiaSharp;
using SkiEngine.Input;
using SkiEngine.Util;

namespace SkiEngine.UI
{
    public abstract class SkiView
    {
        public event Action<SKSize, SKSize> SizeChanged;
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

        private SKSize _size;
        public SKSize Size
        {
            get => _size;
            protected set
            {
                if (_size == value)
                {
                    return;
                }

                var previous = _size;
                _size = value;
                SizeChanged?.Invoke(previous, value);
            }
        }

        public SKRect WorldBounds => Node.LocalToWorldMatrix.MapRect(new SKRect(0, 0, Size.Width, Size.Height));

        public abstract IEnumerable<SkiView> ChildrenEnumerable { get; }
        public abstract bool ListensForPressedTouches { get; }
        public virtual bool IsMultiTouchEnabled => false;
        public virtual ViewTouchResult MultiTouchIgnoredResult => ViewTouchResult.CancelLowerListeners;
        public int NumPressedTouches { get; private set; }

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

        public ViewTouchResult OnPressed(SkiTouch touch)
        {
            NumPressedTouches++;
            return OnPressedInternal(touch);
        }

        public ViewTouchResult OnMoved(SkiTouch touch)
        {
            return OnMovedInternal(touch);
        }

        public ViewTouchResult OnReleased(SkiTouch touch)
        {
            NumPressedTouches--;
            return OnReleasedInternal(touch);
        }

        public void OnCancelled(SkiTouch touch)
        {
            NumPressedTouches--;
            OnCancelledInternal(touch);
        }

        protected virtual ViewTouchResult OnPressedInternal(SkiTouch touch)
        {
            return ViewTouchResult.Passthrough;
        }

        protected virtual ViewTouchResult OnMovedInternal(SkiTouch touch)
        {
            return ViewTouchResult.Passthrough;
        }

        protected virtual ViewTouchResult OnReleasedInternal(SkiTouch touch)
        {
            return ViewTouchResult.Passthrough;
        }

        protected virtual void OnCancelledInternal(SkiTouch touch)
        {
            
        }
    }
}
