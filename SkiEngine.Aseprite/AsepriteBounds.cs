using Newtonsoft.Json;

namespace SkiEngine.Aseprite
{
    public class AsepriteBounds : AsepriteSize
    {
        [JsonProperty("x")]
        public int X;
        [JsonProperty("y")]
        public int Y;
    }
}