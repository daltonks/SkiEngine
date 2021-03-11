using System;
using System.Collections.Generic;

namespace SkiEngine.Input
{
    public class SkiKeyBinding
    {
        private readonly Action _pressedAction;
        private readonly Action _releasedAction;

        public SkiKeyBinding(
            SkiKeyCombination keyCombination,
            Func<bool> predicate,
            Action pressed,
            Action released,
            BehaviorWhenInputViewFocused behaviorWhenInputViewFocused)
        {
            KeyCombination = keyCombination;
            _pressedAction = pressed ?? (() => {});
            _releasedAction = released ?? (() => {});
            BehaviorWhenInputViewFocused = behaviorWhenInputViewFocused;
            Predicate = predicate ?? (() => true);
        }
        
        public SkiKeyCombination KeyCombination { get; }
        public Func<bool> Predicate { get; }
        public BehaviorWhenInputViewFocused BehaviorWhenInputViewFocused { get; }

        public bool IsPressed { get; private set; }

        public void OnPressed()
        {
            IsPressed = true;
            _pressedAction();
        }

        public void OnReleased()
        {
            IsPressed = false;
            _releasedAction();
        }
    }
    
    public class SkiKeyCombination
    {
        public SkiKeyCombination(SkiVirtualKey key, IReadOnlyCollection<SkiVirtualKey> modifiers = null)
        {
            Key = key;
            Modifiers = modifiers ?? new SkiVirtualKey[0];
        }

        public SkiVirtualKey Key { get; }
        public IReadOnlyCollection<SkiVirtualKey> Modifiers { get; }
    }

    public enum BehaviorWhenInputViewFocused
    {
        Active,
        Inactive
    }
}