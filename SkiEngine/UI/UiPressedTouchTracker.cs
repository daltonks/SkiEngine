using System;
using System.Collections.Generic;
using SkiEngine.Input;

namespace SkiEngine.UI
{
    public class UiPressedTouchTracker
    {
        private readonly List<SkiView> _listeners = new List<SkiView>();

        public void OnPressed(SkiView rootView, SkiTouch touch)
        {
            var queue = new Queue<SkiView>();
            queue.Enqueue(rootView);

            var pointWorld = touch.PointWorld;
            while (queue.Count > 0)
            {
                var view = queue.Dequeue();
                if (view.HitTest(pointWorld))
                {
                    if (view.ListensForPressedTouches)
                    {
                        _listeners.Add(view);
                    }

                    foreach (var child in view.Children)
                    {
                        queue.Enqueue(child);
                    }
                }
            }

            _listeners.Reverse();

            HandleInProgressTouch(touch, view => view.OnPressed(touch));
        }

        public void OnMoved(SkiTouch touch)
        {
            HandleInProgressTouch(touch, view => view.OnMoved(touch));
        }

        public void OnReleased(SkiTouch touch)
        {
            HandleInProgressTouch(touch, view => view.OnReleased(touch));
        }

        public void OnCancelled(SkiTouch touch)
        {
            foreach (var listener in _listeners)
            {
                listener.OnCancelled(touch);
            }
        }

        private void HandleInProgressTouch(SkiTouch touch, Func<SkiView, ViewTouchResult> viewFunc)
        {
            for (var i = 0; i < _listeners.Count; i++)
            {
                var listener = _listeners[i];

                var touchResult = viewFunc(listener);
                if (touchResult == ViewTouchResult.CancelLowerListeners)
                {
                    while (i < _listeners.Count - 1)
                    {
                        var l = _listeners[i + 1];
                        l.OnCancelled(touch);
                        _listeners.RemoveAt(i + 1);
                    }

                    break;
                }
                if (touchResult == ViewTouchResult.CancelOtherListeners)
                {
                    foreach (var l in _listeners)
                    {
                        if (l != listener)
                        {
                            l.OnCancelled(touch);
                        }
                    }

                    _listeners.Clear();
                    _listeners.Add(listener);
                    break;
                }
            }
        }
    }

    public enum ViewTouchResult
    {
        Passthrough,
        CancelLowerListeners,
        CancelOtherListeners
    }
}