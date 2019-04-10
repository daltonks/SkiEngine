using SkiaSharp;

namespace SkiEngine.NCS.System
{
    public interface IDrawableSystem
    {
        void Draw(SKCanvas canvas, int viewTarget, double widthXamarinUnits, double heightXamarinUnits);
    }
}