using Newtonsoft.Json;

namespace SkiEngine.Aseprite
{
    public class AsepriteMeta
    {
        [JsonProperty("frameTags")]
        public AsepriteTag[] Tags;
        [JsonProperty("layers")]
        public AsepriteLayer[] Layers;
    }
}