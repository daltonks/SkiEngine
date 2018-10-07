using Engine.DataModel.Mapping;
using SkiEngine.Protobuf;

namespace SkiEngine.Sprite.Mapping
{
    public static class SpriteDataMappingExtensions
    {
        public static PSpriteData ToPSpriteData(this GSpriteData spriteData)
        {
            return new PSpriteData
            {
                TextureBounds = spriteData.TextureBounds.ToPRectI(),
                Origin = spriteData.Origin.ToPPoint(),
                Color = spriteData.Color.ToPColor()
            };
        }

        public static GSpriteData ToGSpriteData(this PSpriteData spriteData)
        {
            return new GSpriteData(spriteData.TextureBounds.ToSKRectI())
            {
                Origin = spriteData.Origin.ToSKPoint(),
                Color = spriteData.Color.ToSKColor()
            };
        }
    }
}
