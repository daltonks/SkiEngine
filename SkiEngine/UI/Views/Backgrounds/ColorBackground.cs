using SkiaSharp;
using SkiEngine.UI.Views.Base;

namespace SkiEngine.UI.Views.Backgrounds
{
    public class ColorBackground : ISkiBackground
    {
        private readonly SkiView _view;

        public ColorBackground(SkiView view, SKColor color)
        {
            _view = view;
            ColorProp = new LinkedProperty<SKColor>(
                this,
                color,
                valueChanged: (sender, args) => _view.InvalidateSurface()
            );
        }

        public LinkedProperty<SKColor> ColorProp { get; }
        public SKColor Color
        {
            get => ColorProp.Value;
            set => ColorProp.Value = value;
        }

        public void DrawBackground(SKCanvas canvas)
        {
            using var paint = new SKPaint { Style = SKPaintStyle.Fill, Color = Color };
            canvas.DrawRect(0, 0, _view.Size.Width, _view.Size.Height, paint);
        }
    }
}
