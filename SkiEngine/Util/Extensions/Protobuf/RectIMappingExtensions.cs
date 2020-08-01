using SkiaSharp;
using SkiEngine.Protobuf;

namespace SkiEngine.Util.Extensions.Protobuf
{
    public static class RectIMappingExtensions
    {
        public static PRectI ToPRectI(this SKRectI skRectI)
        {
            return new PRectI
            {
                Left = skRectI.Left,
                Right = skRectI.Right,
                Top = skRectI.Top,
                Bottom = skRectI.Bottom
            };
        }

        public static SKRectI ToSKRectI(this PRectI pRectI)
        {
            return new SKRectI(pRectI.Left, pRectI.Right, pRectI.Top, pRectI.Bottom);
        }
    }
}
