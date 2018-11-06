using System.Linq;
using SkiEngine.NCS.Component.Sprite;
using SkiEngine.Protobuf;

namespace SkiEngine.Extensions.Protobuf.Sprite
{
    public static class SpriteSheetAnimationDataMappingExtensions
    {
        public static PSpriteSheetAnimationData ToPStateData(this SpriteSheetAnimationData stateData)
        {
            return new PSpriteSheetAnimationData
            {
                States = { stateData.States.Select(gState => gState.ToPStateData()) }
            };
        }

        public static SpriteSheetAnimationData ToGStateData(this PSpriteSheetAnimationData stateData)
        {
            return new SpriteSheetAnimationData(
                stateData.States.Select(pFrame => pFrame.ToGStateData()).ToArray()
            );
        }
    }
}
