using System;

namespace SkiEngine.Input
{
    public class InputService
    {
        public event Action<int> MouseWheelScroll;

        public void OnMouseWheelScroll(int delta)
        {
            MouseWheelScroll?.Invoke(delta);
        }
    }
}
