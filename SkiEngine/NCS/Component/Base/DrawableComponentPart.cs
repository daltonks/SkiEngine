using SkiaSharp;
using SkiEngine.Interfaces;

namespace SkiEngine.NCS.Component.Base
{
    public class DrawableComponentPart
    {
        public delegate void DrawDelegate(SKCanvas canvas, ITransform transform);

        private readonly DrawDelegate _drawAction;

        public DrawableComponentPart(DrawDelegate drawAction)
        {
            _drawAction = drawAction;
        }

        public void Draw(SKCanvas canvas, ITransform transform)
        {
            _drawAction.Invoke(canvas, transform);
        }
    }
}