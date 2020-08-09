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
        public virtual GestureTouchResult MultiTouchIgnoredResult => GestureTouchResult.CancelLowerListeners;
        public int NumPressedTouches { get; private set; }

        public GestureTouchResult OnPressed(SkiTouch touch)
        {
            NumPressedTouches++;
            return OnPressedInternal(touch);
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

        protected abstract GestureTouchResult OnPressedInternal(SkiTouch touch);
        protected abstract GestureTouchResult OnMovedInternal(SkiTouch touch);
        protected abstract GestureTouchResult OnReleasedInternal(SkiTouch touch);
        protected abstract void OnCancelledInternal(SkiTouch touch);
    }

    public enum GestureTouchResult
    {
        Passthrough,
        CancelLowerListeners,
        CancelOtherListeners
    }
}
