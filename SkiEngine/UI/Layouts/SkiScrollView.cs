using System.Collections.Generic;
using SkiaSharp;
using SkiEngine.Input;
using SkiEngine.Input.Touch;

namespace SkiEngine.UI.Layouts
{
    public class SkiScrollView : SkiView, ISingleTouchHandler
    {
        private readonly DiscardMultipleTouchInterceptor _touchInterceptor;

        public SkiScrollView()
        {
            _touchInterceptor = new DiscardMultipleTouchInterceptor(this);
        }

        private SkiView _content;
        public SkiView Content
        {
            get => _content;
            set
            {
                _content?.Node.Destroy();
                CreateChildNode(value);
                _content = value;
            }
        }

        public override IEnumerable<SkiView> Children
        {
            get { yield return Content; }
        }

        public override bool ListensForPressedTouches => true;

        protected override void OnNodeChanged()
        {
            CreateChildNode(Content);
        }

        public override void Layout(float maxWidth, float maxHeight)
        {
            LocalBounds = new SKRect(0, 0, maxWidth, maxHeight);
            Content.Layout(maxWidth, maxHeight);
        }

        public override void Draw(SKCanvas canvas)
        {
            canvas.Save();
            canvas.ClipRect(LocalBounds);
            Content.Draw(canvas);
            canvas.Restore();
        }

        public override ViewTouchResult OnPressed(SkiTouch touch)
        {
            _touchInterceptor.OnTouch(touch);
            return ViewTouchResult.CancelLowerListeners;
        }

        public override ViewTouchResult OnMoved(SkiTouch touch)
        {
            _touchInterceptor.OnTouch(touch);
            return ViewTouchResult.CancelLowerListeners;
        }

        public override ViewTouchResult OnReleased(SkiTouch touch)
        {
            _touchInterceptor.OnTouch(touch);
            return ViewTouchResult.CancelLowerListeners;
        }

        public override void OnCancelled(SkiTouch touch)
        {
            _touchInterceptor.OnTouch(touch);
        }

        private SKPoint _previousPointLocal;
        public void OnTouchPressed(SkiTouch touch)
        {
            _previousPointLocal = Node.WorldToLocalMatrix.MapPoint(touch.PointWorld);
        }

        public void OnTouchMoved(SkiTouch touch)
        {
            var pointLocal = Node.WorldToLocalMatrix.MapPoint(touch.PointWorld);
            var difference = pointLocal - _previousPointLocal;
            Content.Node.RelativePoint = new SKPoint(Content.Node.RelativePoint.X, Content.Node.RelativePoint.Y + difference.Y);
            _previousPointLocal = pointLocal;
            InvalidateSurface();
        }

        public void OnTouchReleased(SkiTouch touch)
        {
            
        }

        public void OnTouchCancelled(SkiTouch touch)
        {
            
        }
    }
}
