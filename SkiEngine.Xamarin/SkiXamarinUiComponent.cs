using System;
using System.ComponentModel;
using SkiaSharp.Views.Forms;
using SkiEngine.Camera;
using SkiEngine.UI;
using Xamarin.Forms;

namespace SkiEngine.Xamarin
{
    public class SkiXamarinUiComponent : SkiUiComponent
    {
        public override event Action<string> HiddenEntryTextChanged;
        public override event Action HiddenEntryUnfocused;
        public override event Action<int> HiddenEntryCursorPositionChanged;
        public override event Action HiddenEntryCompleted;

        private readonly SKCanvasView _canvasView;
        private readonly Entry _hiddenEntry;

        public SkiXamarinUiComponent(
            SKCanvasView canvasView, 
            Entry hiddenEntry,
            Node node, 
            CameraComponent camera, 
            Action invalidateSurface
        ) : base(node, camera, invalidateSurface)
        {
            _canvasView = canvasView;
            _hiddenEntry = hiddenEntry;
            
            _hiddenEntry.PropertyChanged += OnHiddenEntryPropertyChanged;

            _hiddenEntry.TextChanged += (sender, args) =>
            {
                HiddenEntryTextChanged?.Invoke(args.NewTextValue);
            };

            _hiddenEntry.Unfocused += (sender, args) =>
            {
                HiddenEntryUnfocused?.Invoke();
            };

            _hiddenEntry.Completed += (sender, args) =>
            {
                HiddenEntryCompleted?.Invoke();
            };
        }

        private void OnHiddenEntryPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Entry.CursorPositionProperty.PropertyName)
            {
                HiddenEntryCursorPositionChanged?.Invoke(_hiddenEntry.CursorPosition);
            }
        }

        public override void FocusHiddenEntry()
        {
            _hiddenEntry.Focus();
        }

        public override void SetHiddenEntryText(string text)
        {
            _hiddenEntry.Text = text;
        }

        public override void StartAnimation(SkiAnimation skiAnimation)
        {
            new Animation(
                skiAnimation.Callback,
                skiAnimation.Start,
                skiAnimation.End,
                Easing.CubicOut,
                skiAnimation.Finished
            ).Commit(
                _canvasView,
                skiAnimation.Id,
                length: (uint) skiAnimation.Length.TotalMilliseconds
            );
        }

        public override void AbortAnimation(SkiAnimation skiAnimation)
        {
            _canvasView.AbortAnimation(skiAnimation.Id);
        }
    }
}
