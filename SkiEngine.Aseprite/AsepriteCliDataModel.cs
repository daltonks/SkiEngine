using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SkiaSharp;
using SkiEngine.Extensions;
using SkiEngine.NCS.Component.Sprite;

namespace SkiEngine.Aseprite
{
    public class AsepriteCliDataModel
    {
        public static AsepriteCliDataModel FromPath(string jsonOutputPath)
        {
            var json = File.ReadAllText(jsonOutputPath);
            var cliDataModel = JsonConvert.DeserializeObject<AsepriteCliDataModel>(json);
            cliDataModel.Cleanup();
            return cliDataModel;
        }

        [JsonProperty("frames")]
        public AsepriteCel[] Cels;
        [JsonProperty("meta")]
        public AsepriteMeta Metadata;

        private AsepriteCliDataModel() { }

        private void Cleanup()
        {
            Metadata.Tags = Metadata.Tags
                .DistinctBy(t => t.Name)
                .OrderBy(t => t.StartFrame)
                .ThenBy(t => t.EndFrame)
                .ToArray();
            Metadata.Layers = Metadata.Layers.Where(l => !l.IsGroup).ToArray();
            Cels = Cels.Where(c => Metadata.Layers.Select(l => l.Name).Contains(c.LayerName)).ToArray();
        }

        public SpriteSheetAnimationData ToGSpriteSheetAnimationData()
        {
            return new SpriteSheetAnimationData(
                (
                    from tag
                        in Metadata.Tags
                    select
                    (
                        from cel
                            in Cels
                        where tag.StartFrame <= cel.TotalFrame && cel.TotalFrame <= tag.EndFrame
                        orderby cel.TotalFrame
                        group cel
                            by cel.TotalFrame
                        into frameGroup
                        select frameGroup.ToArray()
                    )
                ).Select(
                    (tagGroup, tagIndex) => new SpriteSheetAnimationStateData
                    {
                        Frames =
                        (
                            tagGroup.Select((frameGroup, indexInTag) =>
                                new SpriteSheetAnimationFrameData
                                {
                                    IndexInState = indexInTag,
                                    Duration = TimeSpan.FromMilliseconds(frameGroup.First().DurationInMillis),
                                    Sprites = frameGroup.Select(
                                        cel => new SpriteData(SKRectI.Create(cel.SpriteSheetBounds.X, cel.SpriteSheetBounds.Y, cel.SpriteSheetBounds.Width, cel.SpriteSheetBounds.Height))
                                    ).ToArray()
                                }
                            )
                        ).ToArray()
                    }
                ).ToArray()
            );
        }
    }
}