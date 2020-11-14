using System;
using System.Collections.Generic;
using SkiaSharp;
using SkiEngine.Camera;
using SkiEngine.Drawable;
using SkiEngine.Input;
using SkiEngine.UI.Views;
using SkiEngine.UI.Views.Base;
using SkiEngine.Updateable;
using Topten.RichTextKit;

namespace SkiEngine.UI
{
    public abstract class SkiUiComponent : Component, IUpdateableComponent, IDrawableComponent
    {
        private readonly Queue<Action> _updateActions = new Queue<Action>();
        private readonly Action _invalidateSurface;
        
        public SkiUiComponent(
            Node node, 
            CameraComponent camera, 
            Action invalidateSurface
        )
        {
            Node = node;
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
                _view?.Node.Destroy();
                value.UiComponent = this;
                value.Node = Node.CreateChild();
                _view = value;
                if (Size.Width != 0 && Size.Height != 0)
                {
                    View.Layout(Size.Width, Size.Height);
                    InvalidateSurface();
                }
            }
        }

        private SKSize _size;
        public SKSize Size
        {
            get => _size;
            internal set
            {
                if (_size == value)
                {
                    return;
                }
                
                _size = value;
                View.Layout(_size.Width, _size.Height);
            }
        }

        public SkiView FocusedView { get; internal set; }

        public UpdateableComponentPart UpdateablePart { get; }

        public Style DefaultTextStyle { get; } = new Style();

        public void InvalidateSurface()
        {
            _invalidateSurface();
        }

        public abstract void ShowNativeEntry(SkiEntry entry, int cursorPosition);
        public abstract void HideNativeEntry();
        public abstract void StartAnimation(SkiAnimation skiAnimation);
        public abstract void AbortAnimation(SkiAnimation skiAnimation);

        public void RunNextUpdate(Action action)
        {
            _updateActions.Enqueue(action);
        }

        private void Update(UpdateTime updateTime)
        {
            while (_updateActions.Count > 0)
            {
                _updateActions.Dequeue().Invoke();
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
                var hitTestViews = new List<SkiView>();

                var queue = new Queue<SkiView>();
                queue.Enqueue(View);
                while (queue.Count > 0)
                {
                    var view = queue.Dequeue();
                    if (!view.HitTest(touch.PointWorld))
                    {
                        continue;
                    }

                    hitTestViews.Add(view);

                    foreach (var child in view.ChildrenEnumerable)
                    {
                        queue.Enqueue(child);
                    }
                }

                hitTestViews.Reverse();

                var wheelDeltaDp = Camera.PixelToDpMatrix.MapVector(new SKPoint(touch.WheelDelta, 0)).X;
                foreach (var hitTestView in hitTestViews)
                {
                    if (hitTestView.OnMouseWheelScroll(wheelDeltaDp))
                    {
                        break;
                    }
                }
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
                        touchTracker.OnPressed(this, touch);
                        break;
                    case SKTouchAction.Moved:
                        if (_touchTrackers.TryGetValue(touch.Id, out touchTracker))
                        {
                            touchTracker.OnMoved(touch);
                        }
                        break;
                    case SKTouchAction.Released:
                        if (_touchTrackers.TryGetValue(touch.Id, out touchTracker))
                        {
                            touchTracker.OnReleased(touch);
                            _touchTrackers.Remove(touch.Id);
                            touchTracker.Recycle();
                        }
                        break;
                    case SKTouchAction.Cancelled:
                        if (_touchTrackers.TryGetValue(touch.Id, out touchTracker))
                        {
                            touchTracker.OnCancelled(touch);
                            _touchTrackers.Remove(touch.Id);
                            touchTracker.Recycle();
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public RichString CreateRichString()
        {
            return new RichString { DefaultStyle = DefaultTextStyle };
        }
    }
}
