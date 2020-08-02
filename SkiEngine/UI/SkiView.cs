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

        public SkiUiComponent UiComponent { get; private set; }
        public Node Node { get; private set; }

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

        public abstract IEnumerable<SkiView> Children { get; }
        public abstract bool ListensForPressedTouches { get; }

        public void CreateChildNode(SkiView child, InitialNodeTransform transform = null)
        {
            if (Node != null)
            {
                child.SetNode(UiComponent, Node.CreateChild(transform ?? new InitialNodeTransform()));
            }
        }

        public void SetNode(SkiUiComponent uiComponent, Node node)
        {
            UiComponent = uiComponent;
            if (Node != node)
            {
                Node?.Destroy();
                Node = node;
                OnNodeChanged();
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
            UiComponent.InvalidateSurface();
        }

        public bool HitTest(SKPoint pointWorld)
        {
            var localPoint = Node.WorldToLocalMatrix.MapPoint(pointWorld);
            return new SKRect(0, 0, Size.Width, Size.Height).Contains(localPoint);
        }

        public virtual ViewTouchResult OnPressed(SkiTouch touch)
        {
            return ViewTouchResult.Passthrough;
        }

        public virtual ViewTouchResult OnMoved(SkiTouch touch)
        {
            return ViewTouchResult.Passthrough;
        }

        public virtual ViewTouchResult OnReleased(SkiTouch touch)
        {
            return ViewTouchResult.Passthrough;
        }

        public virtual void OnCancelled(SkiTouch touch)
        {
            
        }
    }
}
