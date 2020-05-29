using SkiaSharp;
using SkiEngine.Extensions.SkiaSharp;
using SkiEngine.NCS.Component.Base;
using SkiEngine.NCS.Component.Camera;

namespace SkiEngine.NCS.Component.Sprite
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

        public void Draw(SKCanvas canvas, CameraComponent camera)
        {
            canvas.DrawImage(Image, Paint, Data);
        }
    }
}
