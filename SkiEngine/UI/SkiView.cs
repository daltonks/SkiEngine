using System;
using System.Collections.Generic;
using SkiaSharp;
using SkiEngine.Input;
using SkiEngine.Util;

namespace SkiEngine.UI
{
    public abstract class SkiView : ILocalBounds
    {
        public event Action<SKRect, SKRect> LocalBoundsChanged;

        public SkiUiComponent UiComponent { get; private set; }
        public Node Node { get; private set; }

        private SKRect _localBounds;
        public SKRect LocalBounds
        {
            get => _localBounds;
            set
            {
                if (_localBounds == value)
                {
                    return;
                }

                var previous = _localBounds;
                _localBounds = value;
                LocalBoundsChanged?.Invoke(previous, value);
            }
        }

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
        public abstract void Draw(SKCanvas canvas);

        public bool HitTest(SKPoint pointWorld)
        {
            var localPoint = Node.WorldToLocalMatrix.MapPoint(pointWorld);
            return LocalBounds.Contains(localPoint);
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
