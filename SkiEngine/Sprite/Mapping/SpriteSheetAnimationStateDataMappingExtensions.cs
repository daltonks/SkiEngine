using System.Linq;
using SkiEngine.Protobuf;

namespace SkiEngine.Sprite.Mapping
{
    public static class SpriteSheetAnimationStateDataMappingExtensions
    {
        public static PSpriteSheetAnimationStateData ToPStateData(this GSpriteSheetAnimationStateData stateData)
        {
            return new PSpriteSheetAnimationStateData
            {
                Loops = stateData.Loops,
                Frames = { stateData.Frames.Select(gFrame => gFrame.ToPFrameData()) }
            };
        }

        public static GSpriteSheetAnimationStateData ToGStateData(this PSpriteSheetAnimationStateData stateData)
        {
            return new GSpriteSheetAnimationStateData
            {
                Loops = stateData.Loops,
                Frames = stateData.Frames.Select(
                    (pFrame, index) =>
                    {
                        var frameData = pFrame.ToGFrameData();
                        frameData.IndexInState = index;
                        return frameData;
                    }
                ).ToArray()
            };
        }
    }
}
