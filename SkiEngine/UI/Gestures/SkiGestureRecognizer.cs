using SkiEngine.Input;
using SkiEngine.UI.Views.Base;

namespace SkiEngine.UI.Gestures
{
    public abstract class SkiGestureRecognizer
    {
        public SkiGestureRecognizer(SkiView view)
        {
            View = view;
        }

        public SkiView View { get; }
        public SkiUiComponent UiComponent => View.UiComponent;
        public virtual bool IsMultiTouchEnabled => false;
        public virtual PressedGestureTouchResult MultiTouchIgnoredResult => PressedGestureTouchResult.CancelLowerListeners;
        public int NumPressedTouches { get; private set; }

        public PressedGestureTouchResult OnPressed(SkiTouch touch)
        {
            var result = OnPressedInternal(touch);
            if (result != PressedGestureTouchResult.Ignore)
            {
                NumPressedTouches++;
            }
            return result;
        }

        public GestureTouchResult OnMoved(SkiTouch touch)
        {
            return OnMovedInternal(touch);
        }

        public GestureTouchResult OnReleased(SkiTouch touch)
        {
            NumPressedTouches--;
            return OnReleasedInternal(touch);
        }

        public void OnCancelled(SkiTouch touch)
        {
            NumPressedTouches--;
            OnCancelledInternal(touch);
        }

        protected abstract PressedGestureTouchResult OnPressedInternal(SkiTouch touch);
        protected abstract GestureTouchResult OnMovedInternal(SkiTouch touch);
        protected abstract GestureTouchResult OnReleasedInternal(SkiTouch touch);
        protected abstract void OnCancelledInternal(SkiTouch touch);
    }

    public enum PressedGestureTouchResult
    {
        Ignore,
        Passthrough,
        CancelLowerListeners,
        CancelOtherListeners
    }

    public enum GestureTouchResult
    {
        Passthrough,
        CancelLowerListeners,
        CancelOtherListeners
    }
}
