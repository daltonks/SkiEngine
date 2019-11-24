using System;

namespace SkiEngine.Input
{
    public class InputService
    {
        public Func<int> CalculateNumberOfMousePointersFunc { private get; set; } = () => 0;

        public event Action<SkiVirtualKey> KeyDown;
        public event Action<SkiVirtualKey> KeyUp;

        public int CalculateNumberOfMousePointers()
        {
            return CalculateNumberOfMousePointersFunc.Invoke();
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
