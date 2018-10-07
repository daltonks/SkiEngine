using System.Linq;
using SkiEngine.Protobuf;

namespace SkiEngine.Sprite.Mapping
{
    public static class SpriteSheetAnimationDataMappingExtensions
    {
        public static PSpriteSheetAnimationData ToPStateData(this GSpriteSheetAnimationData stateData)
        {
            return new PSpriteSheetAnimationData
            {
                States = { stateData.States.Select(gState => gState.ToPStateData()) }
            };
        }

        public static GSpriteSheetAnimationData ToGStateData(this PSpriteSheetAnimationData stateData)
        {
            return new GSpriteSheetAnimationData(
                stateData.States.Select(pFrame => pFrame.ToGStateData()).ToArray()
            );
        }
    }
}
