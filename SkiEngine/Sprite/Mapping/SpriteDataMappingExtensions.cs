using SkiEngine.Protobuf;
using SkiEngine.ProtobufMapping;

namespace SkiEngine.Sprite.Mapping
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
