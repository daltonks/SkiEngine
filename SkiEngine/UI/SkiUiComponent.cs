using System;
using SkiaSharp;
using SkiEngine.Camera;
using SkiEngine.Drawable;
using SkiEngine.Input;

namespace SkiEngine.UI
{
    public class SkiUiComponent : Component, IDrawableComponent
    {
        private readonly Action _invalidateSurface;

        public SkiUiComponent(CameraComponent camera, Action invalidateSurface)
        {
            Camera = camera;
            _invalidateSurface = invalidateSurface;
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
                if (Size.Width != 0 && Size.Height != 0)
                {
                    View.Layout(Size.Width, Size.Height);
                }
            }
        }

        private SKSizeI _size;
        public SKSizeI Size
        {
            get => _size;
            set
            {
                if (_size == value)
                {
                    return;
                }

                _size = value;
                View.Layout(_size.Width, _size.Height);
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
