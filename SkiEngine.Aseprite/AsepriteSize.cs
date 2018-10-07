using Newtonsoft.Json;

namespace SkiEngine.Aseprite
{
    public class AsepriteSize
    {
        [JsonProperty("w")]
        public int Width;
        [JsonProperty("h")]
        public int Height;
    }
}