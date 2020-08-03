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
        private readonly Action<SkiAnimation> _startAnimation;

        public SkiUiComponent(Node node, CameraComponent camera, Action invalidateSurface, Action<SkiAnimation> startAnimation)
        {
            Node = node;
            Camera = camera;
            _invalidateSurface = invalidateSurface;
            _startAnimation = startAnimation;
        }

        public CameraComponent Camera { get; }

        private SkiView _view;
        public SkiView View
        {
            get => _view;
            set
            {
                _view?.Node.Destroy();
                value.UiComponent = this;
                value.Node = Node.CreateChild();
                _view = value;
                if (Size.Width != 0 && Size.Height != 0)
                {
                    View.Layout(Size.Width, Size.Height);
                }
            }
        }

        private SKSize _size;
        public SKSize Size
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

        public void InvalidateSurface()
        {
            _invalidateSurface();
        }

        public void Animate(SkiAnimation animation)
        {
            _startAnimation(animation);
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
                UiPressedTouchTracker touchTracker;
                switch (touch.ActionType)
                {
                    case SKTouchAction.Pressed:
                        touchTracker = UiPressedTouchTracker.Get();
                        _touchTrackers[touch.Id] = touchTracker;
                        touchTracker.OnPressed(View, touch);
                        break;
                    case SKTouchAction.Moved:
                        touchTracker = _touchTrackers[touch.Id];
                        touchTracker.OnMoved(touch);
                        break;
                    case SKTouchAction.Released:
                        touchTracker = _touchTrackers[touch.Id];
                        touchTracker.OnReleased(touch);
                        _touchTrackers.Remove(touch.Id);
                        touchTracker.Recycle();
                        break;
                    case SKTouchAction.Cancelled:
                        touchTracker = _touchTrackers[touch.Id];
                        touchTracker.OnCancelled(touch);
                        _touchTrackers.Remove(touch.Id);
                        touchTracker.Recycle();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
