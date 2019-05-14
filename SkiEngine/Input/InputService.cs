using System;

namespace SkiEngine.Input
{
    public class InputService
    {
        public event Action<int> MouseWheelScroll;
        public event Action<SkiVirtualKey> KeyDown;
        public event Action<SkiVirtualKey> KeyUp;

        public int NumPointersOnWindow { get; set; }

        public void OnMouseWheelScroll(int delta)
        {
            MouseWheelScroll?.Invoke(delta);
        }

        public void OnKeyDown(SkiVirtualKey key)
        {
            KeyDown?.Invoke(key);
        }

        public void OnKeyUp(SkiVirtualKey key)
        {
            KeyUp?.Invoke(key);
        }
    }
}
