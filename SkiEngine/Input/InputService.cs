using System;

namespace SkiEngine.Input
{
    public class InputService
    {
        public static readonly InputService Current = new InputService();

        public event Action<int> MouseWheelScroll;

        public void OnMouseWheelScroll(int delta)
        {
            MouseWheelScroll?.Invoke(delta);
        }
    }
}
