using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using SkiEngine.Camera;
using SkiEngine.UI;
using SkiEngine.UI.Views;
using Xamarin.Forms;

namespace SkiEngine.Xamarin
{
    public class SkiXamarinUiComponent : SkiUiComponent
    {
        private readonly SKCanvasView _canvasView;
        private readonly Entry _nativeEntry;
        private readonly View _nativeEntryLayout;
        private SkiEntry _currentSkiEntry;

        public SkiXamarinUiComponent(
            SKCanvasView canvasView, 
            Entry nativeEntry,
            View nativeEntryLayout,
            Node node, 
            CameraComponent camera, 
            Action invalidateSurface
        ) : base(node, camera, invalidateSurface)
        {
            _canvasView = canvasView;
            _nativeEntry = nativeEntry;
            _nativeEntryLayout = nativeEntryLayout;
        }

        public override void ShowNativeEntry(SkiEntry entry)
        {
            _currentSkiEntry = entry;

            entry.Label.TextProp.ValueChanged += OnCurrentSkiEntryValueChanged;

            _nativeEntry.Text = entry.Label.Text;
            _nativeEntry.FontSize = entry.Label.FontSize;

            _nativeEntryLayout.IsVisible = true;
            
            var dpRect = entry.Node.LocalToWorldMatrix
                .PostConcat(Camera.WorldToDpMatrix)
                .MapRect(entry.BoundsLocal);
            AbsoluteLayout.SetLayoutBounds(_nativeEntry, new Rectangle(dpRect.Left, dpRect.Top, dpRect.Width, dpRect.Height));

            Device.BeginInvokeOnMainThread(() => {
                _nativeEntry.Focus();

                _nativeEntry.TextChanged += OnNativeEntryTextChanged;
                _nativeEntry.Completed += OnNativeEntryCompleted;
            });
        }

        public override void HideNativeEntry()
        {
            _currentSkiEntry.Label.TextProp.ValueChanged -= OnCurrentSkiEntryValueChanged;

            _nativeEntry.TextChanged -= OnNativeEntryTextChanged;
            _nativeEntry.Completed -= OnNativeEntryCompleted;

            _nativeEntry.Text = "";
            _nativeEntry.Unfocus();
            _nativeEntryLayout.IsVisible = false;

            _currentSkiEntry.IsFocused = false;
        }

        private void OnNativeEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            _currentSkiEntry.Label.Text = e.NewTextValue;
        }

        private void OnCurrentSkiEntryValueChanged(object sender, string oldValue, string newValue)
        {
            _nativeEntry.Text = newValue;
        }

        private void OnNativeEntryCompleted(object sender, EventArgs e)
        {
            _currentSkiEntry.OnNativeEntryCompleted();
            if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
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
