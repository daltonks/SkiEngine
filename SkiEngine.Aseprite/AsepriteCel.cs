using Newtonsoft.Json;

namespace SkiEngine.Aseprite
{
    public class AsepriteCel
    {
        [JsonProperty("filename")]
        public string FullName;
        [JsonProperty("frame")]
        public AsepriteBounds SpriteSheetBounds;
        [JsonProperty("spriteSourceSize")]
        public AsepriteBounds SourceBounds;
        [JsonProperty("sourceSize")]
        public AsepriteSize SourceTotalSize;
        [JsonProperty("duration")]
        public int DurationInMillis;
        [JsonProperty("rotated")]
        public bool IsRotated;
        [JsonProperty("trimmed")]
        public bool IsTrimmed;

        public string LayerName 
        {
            get
            {
                var firstIndex = FullName.IndexOf('(') + 1;
                var lastIndex = FullName.IndexOf(')');
                return FullName.Substring(firstIndex, lastIndex - firstIndex);
            }
        }

        public int TotalFrame
        {
            get
            {
                var firstIndex = FullName.IndexOf(')') + 1;
                var lastIndex = FullName.IndexOf('.');
                return int.Parse(FullName.Substring(firstIndex, lastIndex - firstIndex).Trim());
            }
        }
    }
}