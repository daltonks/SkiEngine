using System;
using System.Collections.Generic;
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

        private readonly Dictionary<long, UiPressedTouchTracker> _touchTrackers = new Dictionary<long, UiPressedTouchTracker>();
        public void OnTouch(SkiTouch touch)
        {
            touch.PointWorld = Camera.PixelToWorldMatrix.MapPoint(touch.PointPixels);

            if (touch.ActionType == SKTouchAction.WheelChanged)
            {

            }
            else if (!touch.InContact 
                     && touch.ActionType != SKTouchAction.Released 
                     && touch.ActionType != SKTouchAction.Cancelled)
            {

            }
            else
            {
                switch (touch.ActionType)
                {
                    case SKTouchAction.Pressed:
                        var touchTracker = new UiPressedTouchTracker();
                        _touchTrackers[touch.Id] = touchTracker;
                        touchTracker.OnPressed(View, touch);
                        break;
                    case SKTouchAction.Moved:
                        break;
                    case SKTouchAction.Released:
                        
                        break;
                    case SKTouchAction.Cancelled:
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
