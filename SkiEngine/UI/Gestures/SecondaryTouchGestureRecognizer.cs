using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;
using SkiEngine.Input;
using SkiEngine.UI.Views.Base;

namespace SkiEngine.UI.Gestures
{
    public class SecondaryTouchGestureRecognizer : SkiGestureRecognizer
    {
        private readonly Action _onPressDown;
        private readonly Action _onGestureEnded;
        private readonly Action<SKPoint> _onActivated;

        private readonly TapGestureRecognizer _tapGestureRecognizer;

        public SecondaryTouchGestureRecognizer(
            SkiView view, 
            Action onPressDown,
            Action onGestureEnded,
            Action<SKPoint> onActivated
        ) : base(view)
        {
            _onPressDown = onPressDown;
            _onGestureEnded = onGestureEnded;
            _onActivated = onActivated;

            _tapGestureRecognizer = new TapGestureRecognizer(view, onTapped: _ => {});
            _tapGestureRecognizer.ConsideredNotATap += OnConsideredNotATap;
        }

        private bool _consideredNotATap;
        private bool _longPressTriggered;
        private CancellationTokenSource _longPressCancellationSource;
        private bool _gestureEnded;
        [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
        protected override PressedGestureTouchResult OnPressedInternal(SkiTouch touch)
        {
            if (touch.DeviceType == SKTouchDeviceType.Mouse
                && touch.MouseButton != SKMouseButton.Right)
            {
                return PressedGestureTouchResult.Ignore;
            }

            _consideredNotATap = false;
            _longPressTriggered = false;
            _longPressCancellationSource = new CancellationTokenSource();
            _gestureEnded = false;

            _onPressDown.Invoke();

            if (touch.DeviceType != SKTouchDeviceType.Mouse)
            {
                var longPressCancellationSource = _longPressCancellationSource;
                _ = Task.Run(async () => {
                    await Task.Delay(333);
                    await MainThread.InvokeOnMainThreadAsync(() => {
                        if (!longPressCancellationSource.IsCancellationRequested
                            && !_consideredNotATap)
                        {
                            _onActivated.Invoke(touch.PointWorld);
                            _longPressTriggered = true;
                            TryEndGesture();
                        }
                    });
                });
            }

            return _tapGestureRecognizer.OnPressed(touch);
        }

        protected override GestureTouchResult OnMovedInternal(SkiTouch touch)
        {
            if (_longPressTriggered)
            {
                return GestureTouchResult.CancelOtherListeners;
            }

            return _tapGestureRecognizer.OnMoved(touch);
        }

        private void OnConsideredNotATap()
        {
            _consideredNotATap = true;
            TryEndGesture();
        }

        protected override GestureTouchResult OnReleasedInternal(SkiTouch touch)
        {
            var result = GestureTouchResult.Passthrough;

            _longPressCancellationSource.Cancel();

            if (!_consideredNotATap)
            {
                if (touch.DeviceType == SKTouchDeviceType.Mouse
                    && touch.MouseButton == SKMouseButton.Right)
                {
                    _onActivated.Invoke(touch.PointWorld);
                }

                result = GestureTouchResult.CancelOtherListeners;
            }

            TryEndGesture();

            return result;
        }

        protected override void OnCancelledInternal(SkiTouch touch)
        {
            _longPressCancellationSource.Cancel();

            TryEndGesture();
        }

        private void TryEndGesture()
        {
            if (_gestureEnded)
            {
                return;
            }

            _gestureEnded = true;
            _onGestureEnded.Invoke();
        }
    }
}
