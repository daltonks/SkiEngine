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
            InputViewFocusedOption inputViewFocusedBehavior)
        {
            Key = key;
            Modifiers = modifiers;
            Action = action;
            InputViewFocusedBehavior = inputViewFocusedBehavior;
            Predicate = predicate;
        }

        public SkiVirtualKey Key { get; }
        public IReadOnlyCollection<SkiVirtualKey> Modifiers { get; }
        public Func<bool> Predicate { get; }
        public Action Action { get; }
        public InputViewFocusedOption InputViewFocusedBehavior { get; }
    }

    public enum InputViewFocusedOption
    {
        Active,
        Inactive
    }
}