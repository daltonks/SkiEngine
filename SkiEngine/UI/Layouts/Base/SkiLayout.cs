using SkiaSharp;
using SkiEngine.UI.Views.Base;

namespace SkiEngine.UI.Layouts.Base
{
    public abstract class SkiLayout : SkiView
    {
        private SKSize _maxSize;
        private bool _layoutQueued;

        protected void QueueLayout()
        {
            if (_layoutQueued || UiComponent == null)
            {
                return;
            }

            _layoutQueued = true;
            UiComponent.RunNextUpdate(() => {
                LayoutInternal(_maxSize.Width, _maxSize.Height);
                _layoutQueued = false;
            });

            InvalidateSurface();
        }

        public override void Layout(float maxWidth, float maxHeight)
        {
            _maxSize = new SKSize(maxWidth, maxHeight);
            LayoutInternal(maxWidth, maxHeight);
        }

        protected abstract void LayoutInternal(float maxWidth, float maxHeight);
    }
}
