using System;
using SkiaSharp;
using SkiEngine.UI.Gestures;
using SkiEngine.UI.Layouts;
using WeakEvent;

namespace SkiEngine.UI.Views
{
    public class SkiEntry : SkiScrollView
    {
        private readonly WeakEventSource<bool> _completedEventSource = new WeakEventSource<bool>();
        public event EventHandler<bool> Completed
        {
            add => _completedEventSource.Subscribe(value);
            remove => _completedEventSource.Unsubscribe(value);
        }

        public SkiEntry()
        {
            CanScrollHorizontally = true;
            CanScrollVertically = false;
            HeightRequest = 20;

            Content = Label = new SkiLabel { CursorPosition = 0 };
            Label.TextProp.ValueChanged += OnTextChanged;

            GestureRecognizers.Insert(0, new TapGestureRecognizer(this, OnTapped));

            IsFocusedProp.ValueChanged += (sender, oldValue, newValue) =>
            {
                if (newValue)
                {
                    UiComponent.SetHiddenEntryText(Label.Text);
                    UiComponent.FocusHiddenEntry();

                    UiComponent.HiddenEntryTextChanged += OnHiddenEntryTextChanged;
                    UiComponent.HiddenEntryUnfocused += OnUnfocused;
                    UiComponent.HiddenEntryCursorPositionChanged += OnHiddenEntryCursorPositionChanged;
                    UiComponent.HiddenEntryCompleted += OnHiddenEntryCompleted;
                }
                else
                {
                    OnUnfocused();
                }
            };
        }

        private void OnTextChanged(object sender, string oldValue, string newValue)
        {
            UiComponent.SetHiddenEntryText(newValue);
        }

        private void OnHiddenEntryCompleted()
        {
            _completedEventSource.Raise(this, true);
        }

        private void OnHiddenEntryTextChanged(string text)
        {
            Label.Text = text;
        }

        private void OnHiddenEntryCursorPositionChanged(int cursorPosition)
        {
            Label.CursorPosition = cursorPosition;
        }

        private void OnUnfocused()
        {
            IsFocused = false;
            Label.CursorPosition = null;

            UiComponent.HiddenEntryTextChanged -= OnHiddenEntryTextChanged;
            UiComponent.HiddenEntryUnfocused -= OnUnfocused;
            UiComponent.HiddenEntryCursorPositionChanged -= OnHiddenEntryCursorPositionChanged;
            UiComponent.HiddenEntryCompleted -= OnHiddenEntryCompleted;
        }

        public SkiLabel Label { get; }

        private void OnTapped()
        {
            IsFocused = true;
        }

        public LinkedProperty<SKColor> BackgroundColorProp { get; }
        public SKColor BackgroundColor
        {
            get => BackgroundColorProp.Value;
            set => BackgroundColorProp.Value = value;
        }
    }
}
