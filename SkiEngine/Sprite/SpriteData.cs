using SkiaSharp;

namespace SkiEngine.Sprite
{
    public class SpriteData {
        public SKRectI TextureBounds { get; set; }
        public SKPoint Origin { get; set; }
        public SKColor Color { get; set; } = SKColors.White;

        public SpriteData(SKRectI textureBounds)
        {
            TextureBounds = textureBounds;
            OriginNormalized = new SKPoint(.5f, .5f);
        }

        public SKPoint OriginNormalized
        {
            get => new SKPoint(Origin.X / TextureBounds.Width, Origin.Y / TextureBounds.Height);
            set => Origin = new SKPoint(value.X * TextureBounds.Width, value.Y * TextureBounds.Height);
        }
    }
}