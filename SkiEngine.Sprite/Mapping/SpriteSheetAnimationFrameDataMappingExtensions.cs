using System;
using System.Linq;
using SkiEngine.Protobuf;

namespace SkiEngine.Sprite.Mapping
{
    public static class SpriteSheetAnimationFrameDataMappingExtensions
    {
        public static PSpriteSheetAnimationFrameData ToPFrameData(this GSpriteSheetAnimationFrameData frameData)
        {
            return new PSpriteSheetAnimationFrameData
            {
                TimeMilliseconds = frameData.Duration.TotalMilliseconds,
                Sprites = { frameData.Sprites.Select(gSprite => gSprite.ToPSpriteData()) }
            };
        }

        public static GSpriteSheetAnimationFrameData ToGFrameData(this PSpriteSheetAnimationFrameData frameData)
        {
            return new GSpriteSheetAnimationFrameData
            {
                Duration = TimeSpan.FromMilliseconds(frameData.TimeMilliseconds),
                Sprites = frameData.Sprites.Select(pSprite => pSprite.ToGSpriteData()).ToArray()
            };
        }
    }
}
