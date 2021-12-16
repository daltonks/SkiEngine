using System;
using System.Collections.Generic;
using System.Linq;

namespace SkiEngine.Input
{
    public class SkiInputService
    {
        public static SkiInputService Instance { get; } = new SkiInputService();

        public event Action<SkiVirtualKey> KeyDown = key => { };
        public event Action<SkiVirtualKey> KeyUp = key => { };

        private readonly HashSet<SkiVirtualKey> _downKeys = new HashSet<SkiVirtualKey>();

        private readonly Dictionary<SkiVirtualKey, List<SkiKeyBinding>> _keyBindingsMap = new Dictionary<SkiVirtualKey, List<SkiKeyBinding>>();

        private SkiInputService() { }

        public Func<int> CalculateNumberOfMousePointersFunc { get; set; } = () => 0;
        public Func<bool> IsInputViewFocusedFunc { get; set; } = () => false;

        public bool IsInputViewFocused => IsInputViewFocusedFunc.Invoke();

        public int CalculateNumberOfMousePointers()
        {
            return CalculateNumberOfMousePointersFunc.Invoke();
        }

        public void OnKeyDown(SkiVirtualKey key)
        {
            lock (_downKeys)
            {
                if (!_downKeys.Add(key))
                {
                    return;
                }
            }

            lock (_keyBindingsMap)
            {
                if (_keyBindingsMap.TryGetValue(key, out var keyBindings))
                {
                    var isInputViewFocused = IsInputViewFocused;

                    var matchedKeyBindings = keyBindings
                        .Where(b => !isInputViewFocused || b.BehaviorWhenInputViewFocused == BehaviorWhenInputViewFocused.Active)
                        .Where(b => b.KeyCombination.Modifiers.All(IsKeyDown))
                        .Where(b => b.Predicate.Invoke())
                        .OrderByDescending(b => b.KeyCombination.Modifiers.Count);

                    var maxModifierCount = 0;
                    foreach (var matchedKeyBinding in matchedKeyBindings)
                    {
                        var keyCombinationModifierCount = matchedKeyBinding.KeyCombination.Modifiers.Count;
                        if (maxModifierCount > keyCombinationModifierCount)
                        {
                            break;
                        }

                        maxModifierCount = keyCombinationModifierCount;
                        matchedKeyBinding.OnPressed();
                    }
                }
            }

            KeyDown.Invoke(key);
        }

        public void OnKeyUp(SkiVirtualKey key)
        {
            lock (_downKeys)
            {
                if (!_downKeys.Remove(key))
                {
                    return;
                }
            }

            lock (_keyBindingsMap)
            {
                if (_keyBindingsMap.TryGetValue(key, out var keyBindings))
                {
                    var matchedKeyBindings = keyBindings.Where(b => b.IsPressed);
                    foreach (var matchedKeyBinding in matchedKeyBindings)
                    {
                        matchedKeyBinding.OnReleased();
                    }
                }
            }

            KeyUp.Invoke(key);
        }

        public bool IsKeyUp(SkiVirtualKey key)
        {
            return !IsKeyDown(key);
        }

        public bool IsKeyDown(SkiVirtualKey key)
        {
            lock (_downKeys)
            {
                return _downKeys.Contains(key);
            }
        }

        public void AddKeyBinding(SkiKeyBinding keyBinding)
        {
            lock (_keyBindingsMap)
            {
                if (!_keyBindingsMap.TryGetValue(keyBinding.KeyCombination.Key, out var list))
                {
                    list = _keyBindingsMap[keyBinding.KeyCombination.Key] = new List<SkiKeyBinding>();
                }

                list.Add(keyBinding);
            }
        }

        public void RemoveKeyBinding(SkiKeyBinding keyBinding)
        {
            lock (_keyBindingsMap)
            {
                if (!_keyBindingsMap.TryGetValue(keyBinding.KeyCombination.Key, out var list))
                {
                    return;
                }

                list.Remove(keyBinding);

                if (!list.Any())
                {
                    _keyBindingsMap.Remove(keyBinding.KeyCombination.Key);
                }
            }
        }
    }
}
 