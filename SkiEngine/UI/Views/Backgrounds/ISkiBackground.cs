using SkiaSharp;

namespace SkiEngine.UI.Views.Backgrounds
{
    public interface ISkiBackground
    {
        void Draw(SKCanvas canvas, SKSize size);
    }
}
