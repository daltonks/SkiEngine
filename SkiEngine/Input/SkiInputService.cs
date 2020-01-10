using System;

namespace SkiEngine.Input
{
    public class SkiInputService
    {
        public event Action<SkiVirtualKey> KeyDown = key => { };
        public event Action<SkiVirtualKey> KeyUp = key => { };

        public Func<int> CalculateNumberOfMousePointersFunc { private get; set; } = () => 0;
        public Func<SkiVirtualKey, bool> IsKeyDownFunc { private get; set; } = key => false;
        public Func<SkiVirtualKey, bool> IsKeyLockedFunc { private get; set; } = key => false;
        public Func<bool> IsAnyEntryFocusedFunc { private get; set; } = () => false;

        public bool IsAnyEntryFocused => IsAnyEntryFocusedFunc.Invoke();

        public int CalculateNumberOfMousePointers()
        {
            return CalculateNumberOfMousePointersFunc.Invoke();
        }

        public void OnKeyDown(SkiVirtualKey key)
        {
            KeyDown.Invoke(key);
        }

        public void OnKeyUp(SkiVirtualKey key)
        {
            KeyUp.Invoke(key);
        }

        public bool IsKeyUp(SkiVirtualKey key)
        {
            return !IsKeyDown(key);
        }

        public bool IsKeyDown(SkiVirtualKey key)
        {
            return IsKeyDownFunc.Invoke(key);
        }

        public bool IsKeyLocked(SkiVirtualKey key)
        {
            return IsKeyLockedFunc.Invoke(key);
        }
    }
}
 