namespace SkiEngine.Input.Touch
{
    public interface ISingleTouchHandler
    {
        void OnTouchPressed(SkiTouch touch);
        void OnTouchMoved(SkiTouch touch);
        void OnTouchReleased(SkiTouch touch);
        void OnTouchCancelled(SkiTouch touch);
    }
}
