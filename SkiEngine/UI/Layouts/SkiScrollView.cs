using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public bool Scroll(float yDelta)
        {
            Content.Node.RelativePoint = new SKPoint(Content.Node.RelativePoint.X, Content.Node.RelativePoint.Y + yDelta);
            return AdjustScrollIfOutOfBounds();
        }

        private bool AdjustScrollIfOutOfBounds()
        {
            var point = Content.Node.RelativePoint;
            if (point.Y > 0 || Content.Size.Height <= Size.Height)
            {
                Content.Node.RelativePoint = new SKPoint(point.X, 0);
                return false;
            }
            else if (point.Y < -Content.Size.Height + Size.Height)
            {
                Content.Node.RelativePoint = new SKPoint(point.X, -Content.Size.Height + Size.Height);
                return false;
            }

            return true;
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
            _secondsSinceLastMove = 0;
            _touchPointsPixels[touch.Id] = touch.PointPixels;

            return ViewTouchResult.CancelLowerListeners;
        }

        private readonly Stopwatch _timeSinceLastMoveStopwatch = new Stopwatch();
        private double _secondsSinceLastMove;
        private SKPoint _lastMoveDelta;
        protected override ViewTouchResult OnMovedInternal(SkiTouch touch)
        {
            _secondsSinceLastMove = _timeSinceLastMoveStopwatch.Elapsed.TotalSeconds;
            _timeSinceLastMoveStopwatch.Restart();

            var previousPointPixels = _touchPointsPixels[touch.Id];

            _lastMoveDelta = UiComponent.Camera.PixelToWorldMatrix
                .PostConcat(Node.WorldToLocalMatrix)
                .MapVector(touch.PointPixels - previousPointPixels);
            Scroll(_lastMoveDelta.Y);

            _touchPointsPixels[touch.Id] = touch.PointPixels;

            InvalidateSurface();

            return ViewTouchResult.CancelLowerListeners;
        }

        protected override ViewTouchResult OnReleasedInternal(SkiTouch touch)
        {
            if (NumPressedTouches == 0 && _secondsSinceLastMove > 0)
            {
                var velocity = (float) (_lastMoveDelta.Y / _secondsSinceLastMove / 16);
                UiComponent.Animate(
                    new SkiAnimation(
                        value =>
                        {
                            Scroll((float) value);
                            InvalidateSurface();
                        },
                        velocity,
                        0,
                        TimeSpan.FromSeconds(2)
                    )
                );
            }
            
            _touchPointsPixels.Remove(touch.Id);

            return ViewTouchResult.CancelLowerListeners;
        }

        protected override void OnCancelledInternal(SkiTouch touch)
        {
            _touchPointsPixels.Remove(touch.Id);
        }
    }
}
