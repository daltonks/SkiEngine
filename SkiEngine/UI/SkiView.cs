using System;
using System.Collections.Generic;
using SkiaSharp;
using SkiEngine.Util;

namespace SkiEngine.UI
{
    public abstract class SkiView : ILocalBounds
    {
        public event Action<SKRect, SKRect> LocalBoundsChanged;

        public SkiUiComponent UiComponent { get; private set; }
        public Node.Node Node { get; private set; }

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

        public void Initialize(SkiUiComponent uiComponent, Node.Node node)
        {
            UiComponent = uiComponent;
            Node = node;
        }

        public abstract void Layout(float maxWidth, float maxHeight);

        public abstract void Draw(SKCanvas canvas);
    }
}
