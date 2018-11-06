using SkiEngine.NCS.Component.Sprite;
using SkiEngine.Protobuf;

namespace SkiEngine.Extensions.Protobuf.Sprite
{
    public static class SpriteDataMappingExtensions
    {
        public static PSpriteData ToPSpriteData(this SpriteData spriteData)
        {
            return new PSpriteData
            {
                TextureBounds = spriteData.TextureBounds.ToPRectI(),
                Origin = spriteData.Origin.ToPPoint(),
                Color = spriteData.Color.ToPColor()
            };
        }

        public static SpriteData ToGSpriteData(this PSpriteData spriteData)
        {
            return new SpriteData(spriteData.TextureBounds.ToSKRectI())
            {
                Origin = spriteData.Origin.ToSKPoint(),
                Color = spriteData.Color.ToSKColor()
            };
        }
    }
}
