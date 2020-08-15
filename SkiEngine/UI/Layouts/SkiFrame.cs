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
            HorizontalOptions = VerticalOptions = SkiLayoutOptions.Start;
        }

        protected override void LayoutInternal(float? maxWidth, float? maxHeight)
        {
            Content.Layout(maxWidth - Padding.Left - Padding.Right, maxHeight - Padding.Top - Padding.Bottom);
            float? width;
            float? height;
            switch (HorizontalOptions)
            {
                case SkiLayoutOptions.Fill:
                    width = maxWidth;
                    break;
                default:
                    width = Content.Size.Width + Padding.Left + Padding.Right;
                    break;
            }
            switch (VerticalOptions)
            {
                case SkiLayoutOptions.Fill:
                    height = maxHeight;
                    break;
                default:
                    height = Content.Size.Height + Padding.Top + Padding.Bottom;
                    break;
            }
            Size = new SKSize(width ?? float.MaxValue, height ?? float.MaxValue);
            ViewPreferredWidth = Size.Width;
            ViewPreferredHeight = Size.Height;
            UpdateChildPoint();
        }

        protected override void DrawInternal(SKCanvas canvas)
        {
            canvas.DrawRoundRect(BoundsLocal, 10, 10, Paint);
            Content.Draw(canvas);
        }
    }
}
