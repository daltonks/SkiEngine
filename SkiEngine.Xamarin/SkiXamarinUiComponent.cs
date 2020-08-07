using System;
using SkiaSharp.Views.Forms;
using SkiEngine.Camera;
using SkiEngine.UI;
using Xamarin.Forms;

namespace SkiEngine.Xamarin
{
    public class SkiXamarinUiComponent : SkiUiComponent
    {
        protected override event Action<string> HiddenEntryTextChanged;
        protected override event Action HiddenEntryUnfocused;

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

            _hiddenEntry.TextChanged += (sender, args) =>
            {
                HiddenEntryTextChanged?.Invoke(args.NewTextValue);
            };

            _hiddenEntry.Unfocused += (sender, args) =>
            {
                HiddenEntryUnfocused?.Invoke();
            };
        }

        protected override void FocusHiddenEntry()
        {
            _hiddenEntry.Focus();
        }

        protected override void SetHiddenEntryText(string text)
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
