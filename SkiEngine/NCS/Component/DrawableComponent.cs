using SkiaSharp;
using SkiEngine.Interfaces;
using SkiEngine.NCS.Component.Base;

namespace SkiEngine.NCS.Component
{
    public class DrawableComponent : Base.Component, IDrawableComponent
    {
        public delegate void DrawDelegate(SKCanvas canvas, ITransform transform);

        public DrawDelegate DrawAction { get; }
        
        public DrawableComponent(DrawDelegate drawAction)
        {
            DrawAction = drawAction;
        }

        public void Draw(SKCanvas canvas, ITransform transform)
        {
            DrawAction.Invoke(canvas, transform);
        }
    }
}
