using System;
using System.Collections.Generic;

namespace SkiEngine.Input
{
    public class SkiKeyBinding
    {
        public SkiKeyBinding(
            SkiKeyCombination keyCombination,
            Func<bool> predicate,
            Action action,
            BehaviorWhenInputViewFocused behaviorWhenInputViewFocused)
        {
            KeyCombination = keyCombination;
            Action = action ?? (() => {});
            BehaviorWhenInputViewFocused = behaviorWhenInputViewFocused;
            Predicate = predicate ?? (() => true);
        }
        
        public SkiKeyCombination KeyCombination { get; }
        public Func<bool> Predicate { get; }
        public Action Action { get; }
        public BehaviorWhenInputViewFocused BehaviorWhenInputViewFocused { get; }
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