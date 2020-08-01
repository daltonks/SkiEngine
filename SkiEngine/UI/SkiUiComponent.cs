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
        private readonly Action _invalidateSurface;

        public SkiUiComponent(CameraComponent camera, Action invalidateSurface)
        {
            Camera = camera;
            _invalidateSurface = invalidateSurface;
            UpdateablePart = new UpdateableComponentPart(Update);
        }

        public CameraComponent Camera { get; }

        private SkiView _view;
        public SkiView View
        {
            get => _view;
            set
            {
                value.Initialize(this, Node);
                _view = value;
                if (_width != 0 && _height != 0)
                {
                    View.Layout(_width, _height);
                }
            }
        }

        public UpdateableComponentPart UpdateablePart { get; }

        private int _width;
        private int _height;
        private void Update(UpdateTime updateTime)
        {
            if (Camera.PixelViewport.Width != _width || Camera.PixelViewport.Height != _height)
            {
                _width = Camera.PixelViewport.Width;
                _height = Camera.PixelViewport.Height;
                View.Layout(_width, _height);
            }
        }

        public void Draw(SKCanvas canvas, CameraComponent camera)
        {
            View.Draw(canvas);
        }

        public void OnTouch(SkiTouch touch)
        {

        }
    }
}
