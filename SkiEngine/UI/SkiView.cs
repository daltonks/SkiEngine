using System.Collections.Generic;
using SkiaSharp;
using SkiEngine.NCS;
using SkiEngine.NCS.Component.Camera;
using SkiEngine.NCS.System;
using SkiEngine.Util;

namespace SkiEngine.UI
{
    public abstract class SkiView : ILocalBounds
    {
        public SkiUiComponent UiComponent { get; private set; }
        public Node Node { get; private set; }

        private SKRect _localBoundingBox;
        public ref SKRect LocalBounds => ref _localBoundingBox;

        public abstract IEnumerable<SkiView> Children { get; }

        public void Initialize(SkiUiComponent uiComponent, Node node)
        {
            UiComponent = uiComponent;
            Node = node;
        }

        public virtual void Update(UpdateTime updateTime) { }
        public abstract void Draw(SKCanvas canvas, CameraComponent camera);
    }
}
