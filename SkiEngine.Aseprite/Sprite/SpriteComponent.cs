using SkiaSharp;
using SkiEngine.Camera;
using SkiEngine.Drawable;
using SkiEngine.Extensions.SkiaSharp;

namespace SkiEngine.NCS.Component.Sprite
{
    public class SpriteComponent : SkiEngine.Component, IDrawableComponent
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

        public void Draw(SKCanvas canvas, CameraComponent camera)
        {
            canvas.DrawImage(Image, Paint, Data);
        }
    }
}
