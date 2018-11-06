using SkiaSharp;
using SkiEngine.NCS.Component.Base;

namespace SkiEngine.NCS.Component
{
    public class DrawableComponent : Base.Component, IDrawableComponent
    {
        public delegate void DrawDelegate(SKCanvas canvas);

        public DrawDelegate DrawAction { get; }
        
        public DrawableComponent(DrawDelegate drawAction)
        {
            DrawAction = drawAction;
        }

        public void Draw(SKCanvas canvas)
        {
            DrawAction.Invoke(canvas);
        }
    }
}
