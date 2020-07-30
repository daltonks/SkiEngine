using SkiEngine.Input;

namespace SkiEngine.Touch
{
    public class SingleTouchInterceptorSystemTouchHandler : ISingleTouchHandler
    {
        private readonly SingleTouchInterceptorSystem _system;
        private readonly ISingleTouchHandler _nextTouchHandler;

        public SingleTouchInterceptorSystemTouchHandler(
            ISingleTouchHandler nextTouchHandler,
            SingleTouchInterceptorSystem system
        )
        {
            _system = system;
            _nextTouchHandler = nextTouchHandler;
        }

        private ISingleTouchInterceptor _interceptor;
        public void OnTouchPressed(SkiTouch touch)
        {
            foreach (var i in _system.TouchInterceptors)
            {
                if (i.OnTouchPressed(touch))
                {
                    _interceptor = i;
                    return;
                }
            }

            _nextTouchHandler?.OnTouchPressed(touch);
            _interceptor = null;
        }

        public void OnTouchMoved(SkiTouch touch)
        {
            if (_interceptor == null)
            {
                _nextTouchHandler?.OnTouchMoved(touch);
            }
            else
            {
                _interceptor.OnTouchMoved(touch);
            }
        }

        public void OnTouchReleased(SkiTouch touch)
        {
            if (_interceptor == null)
            {
                _nextTouchHandler?.OnTouchReleased(touch);
            }
            else
            {
                _interceptor.OnTouchReleased(touch);
            }
        }

        public void OnTouchCancelled(SkiTouch touch)
        {
            if (_interceptor == null)
            {
                _nextTouchHandler?.OnTouchCancelled(touch);
            }
            else
            {
                _interceptor.OnTouchCancelled(touch);
            }
        }
    }
}
