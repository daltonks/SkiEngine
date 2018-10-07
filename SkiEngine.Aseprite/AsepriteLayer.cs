using Newtonsoft.Json;

namespace SkiEngine.Aseprite
{
    public class AsepriteLayer
    {
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("group")]
        public string Group;
        [JsonProperty("opacity")]
        public int? Opacity;
        [JsonProperty("blendMode")]
        public string BlendMode;

        public bool IsGroup => Opacity == null;
    }
}