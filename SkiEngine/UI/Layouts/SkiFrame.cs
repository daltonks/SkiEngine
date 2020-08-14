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

        protected override void OnContentChanged()
        {
            InvalidateSurface();
        }

        protected override void OnContentSizeChanged(object sender, SKSize oldSize, SKSize newSize)
        {
            if (UpdateChildPoint())
            {
                InvalidateSurface();
            }
        }

        protected override void OnContentHorizontalOptionsChanged(object sender, SkiLayoutOptions oldValue, SkiLayoutOptions newValue)
        {
            if (UpdateChildPoint())
            {
                InvalidateSurface();
            }
        }

        protected override void OnContentVerticalOptionsChanged(object sender, SkiLayoutOptions oldValue, SkiLayoutOptions newValue)
        {
            if (UpdateChildPoint())
            {
                InvalidateSurface();
            }
        }

        private bool UpdateChildPoint()
        {
            if (Content?.Node == null)
            {
                return false;
            }

            var previousPoint = Content.Node.RelativePoint;
            Content.Node.RelativePoint = new SKPoint(GetOffsetX() + Padding.Left, GetOffsetY() + Padding.Top);
            return Content.Node.RelativePoint != previousPoint;

            float GetOffsetX()
            {
                switch (Content.HorizontalOptions)
                {
                    case SkiLayoutOptions.Fill:
                        return 0;
                    case SkiLayoutOptions.Start:
                        return 0;
                    case SkiLayoutOptions.Center:
                        if (Content.Size.Width < Size.Width)
                        {
                            return Size.Width / 2 - Content.Size.Width / 2;
                        }
                        return 0;
                    case SkiLayoutOptions.End:
                        if (Content.Size.Width < Size.Width)
                        {
                            return Size.Width - Content.Size.Width;
                        }
                        return 0;
                    default:
                        return 0;
                }
            }

            float GetOffsetY()
            {
                switch (Content.VerticalOptions)
                {
                    case SkiLayoutOptions.Fill:
                        return 0;
                    case SkiLayoutOptions.Start:
                        return 0;
                    case SkiLayoutOptions.Center:
                        if (Content.Size.Height < Size.Height)
                        {
                            return Size.Height / 2 - Content.Size.Height / 2;
                        }
                        return 0;
                    case SkiLayoutOptions.End:
                        if (Content.Size.Height < Size.Height)
                        {
                            return Size.Height - Content.Size.Height;
                        }
                        return 0;
                    default:
                        return 0;
                }
            }
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
