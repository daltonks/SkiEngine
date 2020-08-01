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
        }

        public void OnMoved(SkiTouch touch)
        {

        }

        public void OnReleased(SkiTouch touch)
        {

        }

        public void OnCancelled(SkiTouch touch)
        {

        }
    }
}