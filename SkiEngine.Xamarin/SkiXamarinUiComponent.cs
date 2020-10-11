using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using SkiEngine.Camera;
using SkiEngine.UI;
using SkiEngine.UI.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SkiEngine.Xamarin
{
    public class SkiXamarinUiComponent : SkiUiComponent
    {
        private readonly SKCanvasView _skiaView;
        private readonly Entry _nativeEntry;
        private SkiEntry _currentSkiEntry;
        private bool _isNativeEntryShown;

        public SkiXamarinUiComponent(
            SKCanvasView skiaView, 
            Entry nativeEntry,
            Node node, 
            CameraComponent camera, 
            Action invalidateSurface
        ) : base(node, camera, invalidateSurface)
        {
            _skiaView = skiaView;
            _nativeEntry = nativeEntry;

            _nativeEntry.TextChanged += OnNativeEntryTextChanged;
            _nativeEntry.Completed += OnNativeEntryCompleted;
            _nativeEntry.PropertyChanged += OnNativeEntryPropertyChanged;

            switch (Device.RuntimePlatform)
            {
                case Device.UWP:
                    DefaultTextStyle.FontFamily = "Segoe UI";
                    break;
                case Device.iOS:
                    DefaultTextStyle.FontFamily = "San Francisco";
                    break;
            }
        }

        public override void ShowNativeEntry(SkiEntry entry, int cursorPosition)
        {
            if (_isNativeEntryShown)
            {
                return;
            }

            _isNativeEntryShown = true;

            _currentSkiEntry = entry;

            entry.Label.TextProp.ValueChanged += OnCurrentSkiEntryValueChanged;

            _nativeEntry.Text = entry.Label.Text;
            _nativeEntry.FontSize = entry.Label.FontSize;

            var dpRect = entry.Node.LocalToWorldMatrix
                .PostConcat(Camera.WorldToDpMatrix)
                .MapRect(entry.BoundsLocal);
            AbsoluteLayout.SetLayoutBounds(_nativeEntry, new Rectangle(dpRect.Left, dpRect.Top, dpRect.Width, dpRect.Height));

            _nativeEntry.IsVisible = true;

            Device.BeginInvokeOnMainThread(() => {
                _nativeEntry.Focus();
                _nativeEntry.CursorPosition = cursorPosition;
            });
        }

        public override void HideNativeEntry()
        {
            if (!_isNativeEntryShown)
            {
                return;
            }

            _isNativeEntryShown = false;

            _currentSkiEntry.Label.TextProp.ValueChanged -= OnCurrentSkiEntryValueChanged;

            _nativeEntry.Text = "";
            _nativeEntry.Unfocus();
            _nativeEntry.IsVisible = false;

            _currentSkiEntry.IsFocused = false;
        }

        private void OnCurrentSkiEntryValueChanged(object sender, ValueChangedArgs<string> args)
        {
            _nativeEntry.Text = args.NewValue;
        }

        private void OnNativeEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isNativeEntryShown)
            {
                _currentSkiEntry.Label.Text = _nativeEntry.Text;
            }
        }

        private void OnNativeEntryCompleted(object sender, EventArgs e)
        {
            _currentSkiEntry.OnNativeEntryCompleted();
            if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            {
                _currentSkiEntry.IsFocused = false;
            }
        }

        private void OnNativeEntryPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == VisualElement.IsFocusedProperty.PropertyName && !_nativeEntry.IsFocused)
            {
                _currentSkiEntry.IsFocused = false;
            }
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
                _skiaView,
                skiAnimation.Id,
                length: (uint) skiAnimation.Length.TotalMilliseconds
            );
        }

        public override void AbortAnimation(SkiAnimation skiAnimation)
        {
            _skiaView.AbortAnimation(skiAnimation.Id);
        }
    }
}
