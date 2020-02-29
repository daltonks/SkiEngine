using System;
using System.Collections.Generic;

namespace SkiEngine.Input
{
    public class SkiKeyBinding
    {
        public SkiKeyBinding(
            SkiVirtualKey key, 
            IReadOnlyCollection<SkiVirtualKey> modifiers,
            Func<bool> predicate,
            Action action,
            BehaviorWhenInputViewFocused behaviorWhenInputViewFocused)
        {
            Key = key;
            Modifiers = modifiers ?? new SkiVirtualKey[0];
            Action = action ?? (() => {});
            BehaviorWhenInputViewFocused = behaviorWhenInputViewFocused;
            Predicate = predicate ?? (() => true);
        }

        public SkiVirtualKey Key { get; }
        public IReadOnlyCollection<SkiVirtualKey> Modifiers { get; }
        public Func<bool> Predicate { get; }
        public Action Action { get; }
        public BehaviorWhenInputViewFocused BehaviorWhenInputViewFocused { get; }
    }

    public enum BehaviorWhenInputViewFocused
    {
        Active,
        Inactive
    }
}