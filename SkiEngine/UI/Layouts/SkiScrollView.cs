using System.Collections.Generic;
using SkiaSharp;
using SkiEngine.Input;

namespace SkiEngine.UI.Layouts
{
    public class SkiScrollView : SkiView
    {
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
                
                UpdateChildNode(value);
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
        public override bool IsMultiTouchEnabled => true;

        public void Scroll(float yDelta)
        {
            Content.Node.RelativePoint = new SKPoint(Content.Node.RelativePoint.X, Content.Node.RelativePoint.Y + yDelta);
            AdjustScrollIfOutOfBounds();
        }

        private void AdjustScrollIfOutOfBounds()
        {
            var point = Content.Node.RelativePoint;
            if (point.Y > 0)
            {
                Content.Node.RelativePoint = new SKPoint(point.X, 0);
            }
            else if (point.Y < -Content.Size.Height + Size.Height)
            {
                Content.Node.RelativePoint = new SKPoint(point.X, -Content.Size.Height + Size.Height);
            }
        }

        protected override void OnNodeChanged()
        {
            UpdateChildNode(Content);
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

        private readonly Dictionary<long, SKPoint> _touchPointsPixels = new Dictionary<long, SKPoint>();
        protected override ViewTouchResult OnPressedInternal(SkiTouch touch)
        {
            _touchPointsPixels[touch.Id] = touch.PointPixels;

            return ViewTouchResult.CancelLowerListeners;
        }

        protected override ViewTouchResult OnMovedInternal(SkiTouch touch)
        {
            var previousPointPixels = _touchPointsPixels[touch.Id];
            var differenceLocal = UiComponent.Camera.PixelToWorldMatrix
                .PostConcat(Node.WorldToLocalMatrix)
                .MapVector(touch.PointPixels - previousPointPixels);
            Scroll(differenceLocal.Y);
            _touchPointsPixels[touch.Id] = touch.PointPixels;
            InvalidateSurface();

            return ViewTouchResult.CancelLowerListeners;
        }

        protected override ViewTouchResult OnReleasedInternal(SkiTouch touch)
        {
            _touchPointsPixels.Remove(touch.Id);

            return ViewTouchResult.CancelLowerListeners;
        }

        protected override void OnCancelledInternal(SkiTouch touch)
        {
            _touchPointsPixels.Remove(touch.Id);
        }
    }
}
