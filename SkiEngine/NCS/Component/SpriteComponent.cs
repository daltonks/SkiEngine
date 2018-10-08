using SkiaSharp;
using SkiEngine.Extensions;
using SkiEngine.Interfaces;
using SkiEngine.NCS.Component.Base;
using SkiEngine.Sprite;

namespace SkiEngine.NCS.Component
{
    public class SpriteComponent : Base.Component, IDrawableComponent
    {
        public SKImage Image { get; set; }
        public SKPaint Paint { get; set; }
        public SpriteData Data { get; set; }

        public DrawableComponentPart DrawablePart { get; }

        public SpriteComponent(SKImage image, SpriteData data, SKPaint paint = null)
        {
            Image = image;
            Data = data;
            Paint = paint;

            DrawablePart = new DrawableComponentPart(Draw);
        }

        public void Draw(SKCanvas canvas, ITransform transform)
        {
            canvas.DrawImage(Image, Paint, Data, transform);
        }
    }
}
