using SkiaSharp;
using SkiEngine.UI.Views.Base;
using SkiEngine.UI.Views.Layouts.Base;

namespace SkiEngine.UI.Views.Layouts
{
    public class SkiFrame : SkiSingleChildLayout
    {
        private static readonly SKPaint Paint = new SKPaint
        {
            IsAntialias = true, 
            Color = SKColors.White, 
            ImageFilter = SKImageFilter.CreateDropShadow(0, 3, 3, 3, 0x66000000)
        };

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
            const float radius = 10;

            canvas.DrawRoundRect(BoundsLocal, radius, radius, Paint);

            if (Background != null)
            {
                using (new SKAutoCanvasRestore(canvas))
                {
                    canvas.ClipRoundRect(new SKRoundRect(BoundsLocal, radius), antialias: true);
                    DrawBackground(canvas);
                }
            }

            Content.Draw(canvas);
        }
    }
}
