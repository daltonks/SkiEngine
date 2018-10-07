using System;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;
using LZ4;
using SkiEngine.Protobuf;
using SkiEngine.Sprite.Mapping;

namespace SkiEngine.Sprite
{
    public class SpriteSheetAnimationData
    {
        public IReadOnlyList<GSpriteSheetAnimationStateData> States { get; }

        public SpriteSheetAnimationData(IReadOnlyList<GSpriteSheetAnimationStateData> states)
        {
            States = states;
        }

        public int NumLayers => this.States[0].Frames[0].Sprites.Length;

        public void WriteTo(Stream stream)
        {
            using (var lz4Stream = new LZ4Stream(stream, LZ4StreamMode.Compress))
            {
                this.ToPStateData().WriteTo(lz4Stream);
                lz4Stream.Flush();
            }
        }

        public static SpriteSheetAnimationData ReadFrom(Stream stream)
        {
            using (var lz4Stream = new LZ4Stream(stream, LZ4StreamMode.Decompress))
            {
                var protobufModel = PSpriteSheetAnimationData.Parser.ParseFrom(lz4Stream);
                return protobufModel.ToGStateData();
            }
        }
    }
    
    public class GSpriteSheetAnimationStateData
    {
        public bool Loops = true;
        public GSpriteSheetAnimationFrameData[] Frames;
    }

    public class GSpriteSheetAnimationFrameData
    {
        public int IndexInState;
        public TimeSpan Duration;
        public SpriteData[] Sprites;
    }
}
