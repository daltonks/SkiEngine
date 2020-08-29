using SkiaSharp;

namespace SkiEngine.UI.Views.Backgrounds
{
    public interface ISkiBackground
    {
        void DrawBackground(SKCanvas canvas, SKSize size);
    }
}
