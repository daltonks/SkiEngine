using System;
using SkiaSharp;
using SkiEngine.UI.Layouts.Base;
using SkiEngine.UI.Views.Base;

namespace SkiEngine.UI.Layouts
{
    public class SkiFrame : SkiSingleChildLayout
    {
        private static readonly SKPaint Paint = new SKPaint
            {IsAntialias = true, Color = SKColors.White, ImageFilter = SKImageFilter.CreateDropShadow(0, 5, 4, 4, SKColors.Gray)};

        public SkiFrame()
        {
            Padding = new SKRect(10, 10, 10, 10);
        }

        protected override void OnContentSizeChanged(object sender, SKSize oldSize, SKSize newSize)
        {
            InvalidateLayout();
        }

        protected override void OnContentHorizontalOptionsChanged(object sender, SkiLayoutOptions oldValue, SkiLayoutOptions newValue)
        {
            InvalidateLayout();
        }

        protected override void OnContentVerticalOptionsChanged(object sender, SkiLayoutOptions oldValue, SkiLayoutOptions newValue)
        {
            InvalidateLayout();
        }

        protected override void LayoutInternal(float? maxWidth, float? maxHeight)
        {
            Content.Layout(maxWidth - Padding.Left - Padding.Right, maxHeight - Padding.Top - Padding.Bottom);
            Size = new SKSize(
                Content.Size.Width + Padding.Left + Padding.Right, 
                Content.Size.Height + Padding.Top + Padding.Bottom
            );
            UpdateChildPoint();
        }

        protected override void DrawInternal(SKCanvas canvas)
        {
            canvas.DrawRoundRect(BoundsLocal, 10, 10, Paint);
            Content.Draw(canvas);
        }
    }
}
