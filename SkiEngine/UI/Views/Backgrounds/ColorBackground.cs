using SkiaSharp;
using SkiEngine.UI.Views.Base;

namespace SkiEngine.UI.Views.Backgrounds
{
    public class ColorBackground : ISkiBackground
    {
        public ColorBackground(SkiView view, SKColor color)
        {
            ColorProp = new LinkedProperty<SKColor>(
                this,
                color,
                valueChanged: (sender, oldValue, newValue) => view.InvalidateSurface()
            );
        }

        public LinkedProperty<SKColor> ColorProp { get; }
        public SKColor Color
        {
            get => ColorProp.Value;
            set => ColorProp.Value = value;
        }

        public void Draw(SKCanvas canvas, SKSize size)
        {
            using var paint = new SKPaint { Style = SKPaintStyle.Fill, Color = Color };
            canvas.DrawRect(0, 0, size.Width, size.Height, paint);
        }
    }
}
