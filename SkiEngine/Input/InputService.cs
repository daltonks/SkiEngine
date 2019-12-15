using System;
using System.Collections.Generic;

namespace SkiEngine.Input
{
    public class InputService
    {
        public Func<int> CalculateNumberOfMousePointersFunc { private get; set; } = () => 0;

        public event Action<SkiVirtualKey> KeyDown;
        public event Action<SkiVirtualKey> KeyUp;

        private readonly HashSet<SkiVirtualKey> _keysDown = new HashSet<SkiVirtualKey>();

        public int CalculateNumberOfMousePointers()
        {
            return CalculateNumberOfMousePointersFunc.Invoke();
        }

        public void OnKeyDown(SkiVirtualKey key)
        {
            _keysDown.Add(key);
            KeyDown?.Invoke(key);
        }

        public void OnKeyUp(SkiVirtualKey key)
        {
            _keysDown.Remove(key);
            KeyUp?.Invoke(key);
        }

        public bool IsKeyDown(SkiVirtualKey key)
        {
            return _keysDown.Contains(key);
        }
    }
}
 