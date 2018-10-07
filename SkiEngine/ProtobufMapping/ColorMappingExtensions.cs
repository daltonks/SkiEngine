using SkiaSharp;
using SkiEngine.Protobuf;

namespace SkiEngine.ProtobufMapping
{
    public static class ColorMappingExtensions
    {
        public static SKColor ToSKColor(this PColor pColor)
        {
            return new SKColor(pColor.PackedValue);
        }

        public static PColor ToPColor(this SKColor skColor)
        {
            return new PColor
            {
                PackedValue = (uint) skColor
            };
        }
    }
}
