using Newtonsoft.Json;

namespace SkiEngine.Aseprite
{
    public class AsepriteTag
    {
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("from")]
        public int StartFrame;
        [JsonProperty("to")]
        public int EndFrame;
        [JsonProperty("direction")]
        public string Direction;
    }
}