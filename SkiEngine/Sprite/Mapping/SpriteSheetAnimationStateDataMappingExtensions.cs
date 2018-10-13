﻿using System.Linq;
using SkiEngine.Protobuf;

namespace SkiEngine.Sprite.Mapping
{
    public static class SpriteSheetAnimationStateDataMappingExtensions
    {
        public static PSpriteSheetAnimationStateData ToPStateData(this SpriteSheetAnimationStateData stateData)
        {
            return new PSpriteSheetAnimationStateData
            {
                Loops = stateData.Loops,
                Frames = { stateData.Frames.Select(gFrame => gFrame.ToPFrameData()) }
            };
        }

        public static SpriteSheetAnimationStateData ToGStateData(this PSpriteSheetAnimationStateData stateData)
        {
            return new SpriteSheetAnimationStateData
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
