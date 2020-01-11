using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiEngine.Input
{
    public class SkiInputService
    {
        public event Action<SkiVirtualKey> KeyDown = key => { };
        public event Action<SkiVirtualKey> KeyUp = key => { };

        private readonly Dictionary<SkiVirtualKey, List<SkiKeyBinding>> _keyBindingsMap = new Dictionary<SkiVirtualKey, List<SkiKeyBinding>>();

        public Func<int> CalculateNumberOfMousePointersFunc { private get; set; } = () => 0;
        public Func<SkiVirtualKey, bool> IsKeyDownFunc { private get; set; } = key => false;
        public Func<SkiVirtualKey, bool> IsKeyLockedFunc { private get; set; } = key => false;
        public Func<bool> IsInputViewFocusedFunc { private get; set; } = () => false;

        public bool IsInputViewFocused => IsInputViewFocusedFunc.Invoke();

        public int CalculateNumberOfMousePointers()
        {
            return CalculateNumberOfMousePointersFunc.Invoke();
        }

        public void OnKeyDown(SkiVirtualKey key)
        {
            lock (_keyBindingsMap)
            {
                if (_keyBindingsMap.TryGetValue(key, out var keyBindings))
                {
                    var isInputViewFocused = IsInputViewFocused;
                    foreach (var keyBinding in keyBindings.Where(keyBinding => keyBinding.Modifiers?.All(IsKeyDown) ?? true))
                    {
                        if (keyBinding.InputViewFocusedBehavior == InputViewFocusedOption.Inactive && isInputViewFocused)
                        {
                            continue;
                        }

                        if (keyBinding.Predicate?.Invoke() ?? true)
                        {
                            keyBinding.Action?.Invoke();
                        }
                    }
                }
            }

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

        public void AddKeyBinding(SkiKeyBinding keyBinding)
        {
            lock (_keyBindingsMap)
            {
                if (!_keyBindingsMap.TryGetValue(keyBinding.Key, out var list))
                {
                    list = _keyBindingsMap[keyBinding.Key] = new List<SkiKeyBinding>();
                }

                list.Add(keyBinding);
            }
        }

        public void RemoveKeyBinding(SkiKeyBinding keyBinding)
        {
            lock (_keyBindingsMap)
            {
                if (!_keyBindingsMap.TryGetValue(keyBinding.Key, out var list))
                {
                    return;
                }

                list.Remove(keyBinding);

                if (!list.Any())
                {
                    _keyBindingsMap.Remove(keyBinding.Key);
                }
            }
        }
    }
}
 