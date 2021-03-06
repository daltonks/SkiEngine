﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SkiEngine.Input;
using SkiEngine.UI.Gestures;
using SkiEngine.UI.Views.Base;

namespace SkiEngine.UI
{
    public class UiPressedTouchTracker
    {
        private static readonly ConcurrentBag<UiPressedTouchTracker> Cached = new ConcurrentBag<UiPressedTouchTracker>();

        public static UiPressedTouchTracker Get()
        {
            return Cached.TryTake(out var cachedTracker) 
                ? cachedTracker 
                : new UiPressedTouchTracker();
        }

        private UiPressedTouchTracker() { }

        private readonly List<SkiGestureRecognizer> _recognizers = new List<SkiGestureRecognizer>();

        public void OnPressed(SkiUiComponent uiComponent, SkiTouch touch)
        {
            var queue = new Queue<SkiView>();
            queue.Enqueue(uiComponent.View);

            var pressedFocusedView = false;
            var pointWorld = touch.PointWorld;
            while (queue.Count > 0)
            {
                var view = queue.Dequeue();
                if (!view.HitTest(pointWorld))
                {
                    continue;
                }

                if (view == uiComponent.FocusedView)
                {
                    pressedFocusedView = true;
                }

                // Add recognizers in reverse-order, because all 
                // eligible recognizers will be reversed
                for (var i = view.GestureRecognizers.Count - 1; i >= 0; i--)
                {
                    var recognizer = view.GestureRecognizers[i];
                    _recognizers.Add(recognizer);
                }

                foreach (var child in view.ChildrenEnumerable)
                {
                    queue.Enqueue(child);
                }
            }

            if (!pressedFocusedView && uiComponent.FocusedView != null)
            {
                uiComponent.FocusedView.IsFocused = false;
            }

            // Reverse the recognizers so that higher views are processed first
            _recognizers.Reverse();

            for (var i = 0; i < _recognizers.Count; i++)
            {
                var recognizer = _recognizers[i];

                var ignoreMultiTouch = !recognizer.IsMultiTouchEnabled && recognizer.NumPressedTouches > 0;

                var touchResult = ignoreMultiTouch
                    ? recognizer.MultiTouchIgnoredResult
                    : recognizer.OnPressed(touch);

                var ignoreTouch = touchResult == PressedGestureTouchResult.Ignore || ignoreMultiTouch;

                if (ignoreTouch)
                {
                    _recognizers.RemoveAt(i);
                    i--;
                }

                if (touchResult == PressedGestureTouchResult.CancelLowerListeners)
                {
                    while (i < _recognizers.Count - 1)
                    {
                        _recognizers.RemoveAt(i + 1);
                    }

                    break;
                }
                if (touchResult == PressedGestureTouchResult.CancelOtherListeners)
                {
                    _recognizers.Clear();
                    if (!ignoreTouch)
                    {
                        _recognizers.Add(recognizer);
                    }
                    break;
                }
            }
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
            foreach (var listener in _recognizers)
            {
                listener.OnCancelled(touch);
            }
        }

        private void HandleInProgressTouch(SkiTouch touch, Func<SkiGestureRecognizer, GestureTouchResult> recognizerFunc)
        {
            for (var i = 0; i < _recognizers.Count; i++)
            {
                var recognizer = _recognizers[i];
                
                var touchResult = recognizerFunc(recognizer);
                if (touchResult == GestureTouchResult.CancelLowerListeners)
                {
                    while (i < _recognizers.Count - 1)
                    {
                        var l = _recognizers[i + 1];
                        _recognizers.RemoveAt(i + 1);
                        l.OnCancelled(touch);
                    }

                    break;
                }
                if (touchResult == GestureTouchResult.CancelOtherListeners)
                {
                    foreach (var r in _recognizers)
                    {
                        if (r != recognizer)
                        {
                            r.OnCancelled(touch);
                        }
                    }
                    
                    _recognizers.Clear();
                    _recognizers.Add(recognizer);
                    break;
                }
            }
        }

        public void Recycle()
        {
            _recognizers.Clear();
            Cached.Add(this);
        }
    }
}