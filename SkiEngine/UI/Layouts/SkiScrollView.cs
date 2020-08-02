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
                if (_content != null)
                {
                    _content.Node.Destroy();
                    _content.SizeChanged -= OnContentSizeChanged;
                }
                
                CreateChildNode(value);
                _content = value;
                _content.SizeChanged += OnContentSizeChanged;
            }
        }

        private void OnContentSizeChanged(SKSize oldSize, SKSize newSize)
        {
            AdjustScrollIfOutOfBounds();
        }

        public override IEnumerable<SkiView> Children
        {
            get { yield return Content; }
        }

        public override bool ListensForPressedTouches => true;

        public void Scroll(float yDelta)
        {
            Content.Node.RelativePoint = new SKPoint(Content.Node.RelativePoint.X, Content.Node.RelativePoint.Y + yDelta);
            AdjustScrollIfOutOfBounds();
        }

        private void AdjustScrollIfOutOfBounds()
        {
            if (Content.Node.RelativePoint.Y > 0)
            {
                Content.Node.RelativePoint = new SKPoint(Content.Node.RelativePoint.X, 0);
            }
            else if (Content.Node.RelativePoint.Y < -Content.Size.Height + Size.Height)
            {
                Content.Node.RelativePoint = new SKPoint(Content.Node.RelativePoint.X, -Content.Size.Height + Size.Height);
            }
        }

        protected override void OnNodeChanged()
        {
            CreateChildNode(Content);
        }

        public override void Layout(float maxWidth, float maxHeight)
        {
            Size = new SKSize(maxWidth, maxHeight);
            Content.Layout(maxWidth, float.MaxValue);
            AdjustScrollIfOutOfBounds();
        }

        protected override void DrawInternal(SKCanvas canvas)
        {
            canvas.Save();
            var skRect = new SKRect(0, 0, Size.Width, Size.Height);
            canvas.ClipRect(skRect);
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

        private SKPoint _previousPointPixels;
        public void OnTouchPressed(SkiTouch touch)
        {
            _previousPointPixels = touch.PointPixels;
        }

        public void OnTouchMoved(SkiTouch touch)
        {
            var differenceLocal = UiComponent.Camera.PixelToWorldMatrix
                .PostConcat(Node.WorldToLocalMatrix)
                .MapVector(touch.PointPixels - _previousPointPixels);
            Scroll(differenceLocal.Y);
            _previousPointPixels = touch.PointPixels;
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
