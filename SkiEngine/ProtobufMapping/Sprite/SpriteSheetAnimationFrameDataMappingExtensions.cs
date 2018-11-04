using System;
using System.Linq;
using SkiEngine.Protobuf;
using SkiEngine.Sprite;

namespace SkiEngine.ProtobufMapping.Sprite
{
    public static class SpriteSheetAnimationFrameDataMappingExtensions
    {
        public static PSpriteSheetAnimationFrameData ToPFrameData(this SpriteSheetAnimationFrameData frameData)
        {
            return new PSpriteSheetAnimationFrameData
            {
                TimeMilliseconds = frameData.Duration.TotalMilliseconds,
                Sprites = { frameData.Sprites.Select(gSprite => gSprite.ToPSpriteData()) }
            };
        }

        public static SpriteSheetAnimationFrameData ToGFrameData(this PSpriteSheetAnimationFrameData frameData)
        {
            return new SpriteSheetAnimationFrameData
            {
                Duration = TimeSpan.FromMilliseconds(frameData.TimeMilliseconds),
                Sprites = frameData.Sprites.Select(pSprite => pSprite.ToGSpriteData()).ToArray()
            };
        }
    }
}
