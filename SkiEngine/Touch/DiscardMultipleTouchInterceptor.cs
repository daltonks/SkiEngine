using SkiaSharp;
using SkiEngine.Input;

namespace SkiEngine.Touch
{
    public class DiscardMultipleTouchInterceptor : IMultiTouchHandler
    {
        private readonly ISingleTouchHandler _nextTouchHandler;

        public DiscardMultipleTouchInterceptor(ISingleTouchHandler nextTouchHandler)
        {
            _nextTouchHandler = nextTouchHandler;
        }

        private long? _handledTouchId;
        private SKPoint _previousPixelPoint;
        public void OnTouch(SkiTouch touch)
        {
            switch (touch.ActionType)
            {
                case SKTouchAction.Pressed:
                    if (_handledTouchId == null)
                    {
                        _handledTouchId = touch.Id;
                        _previousPixelPoint = touch.PointPixels;

                        _nextTouchHandler.OnTouchPressed(touch);
                    }
                    break;
                case SKTouchAction.Moved:
                    // Check if pixel point is different than the previous one,
                    // because it can be the same on Windows sometimes
                    if (touch.Id == _handledTouchId && touch.PointPixels != _previousPixelPoint)
                    {
                        _nextTouchHandler.OnTouchMoved(touch);

                        _previousPixelPoint = touch.PointPixels;
                    }
                    break;
                case SKTouchAction.Released:
                    if (touch.Id == _handledTouchId)
                    {
                        _handledTouchId = null;

                        _nextTouchHandler.OnTouchReleased(touch);
                    }
                    break;
                case SKTouchAction.Cancelled:
                    if (touch.Id == _handledTouchId)
                    {
                        _handledTouchId = null;

                        _nextTouchHandler.OnTouchCancelled(touch);
                    }
                    break;
                case SKTouchAction.Entered:
                case SKTouchAction.Exited:
                default:
                    break;
            }
        }
    }
}
