using System;
using SkiaSharp;
using SkiEngine.Input;
using SkiEngine.NCS;
using SkiEngine.NCS.Component.Base;
using SkiEngine.NCS.Component.Camera;
using SkiEngine.NCS.System;

namespace SkiEngine.UI
{
    public class SkiUiComponent : Component, IUpdateableComponent, IDrawableComponent
    {
        private readonly CameraComponent _camera;
        private readonly Action _invalidateSurface;

        public SkiUiComponent(SkiView view, CameraComponent camera, Action invalidateSurface)
        {
            View = view;
            _camera = camera;
            _invalidateSurface = invalidateSurface;
            UpdateablePart = new UpdateableComponentPart(Update);
        }

        private SkiView _view;
        public SkiView View
        {
            get => _view;
            set
            {
                value.Initialize(this, Node);
                _view = value;
            }
        }

        public UpdateableComponentPart UpdateablePart { get; }

        private void Update(UpdateTime updateTime)
        {
            
        }

        public void Draw(SKCanvas canvas, CameraComponent camera)
        {
            View.Draw(canvas, camera);
        }

        public void OnTouch(SkiTouch touch)
        {

        }
    }
}
