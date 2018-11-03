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
        
        public SpriteComponent(SKImage image, SpriteData data, SKPaint paint = null)
        {
            Image = image;
            Data = data;
            Paint = paint;
        }

        public void Draw(SKCanvas canvas, ITransform transform)
        {
            canvas.DrawImage(Image, Paint, Data);
        }
    }
}
