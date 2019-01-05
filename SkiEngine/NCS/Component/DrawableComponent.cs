using System;
using SkiaSharp;
using SkiEngine.NCS.Component.Base;

namespace SkiEngine.NCS.Component
{
    public class DrawableComponent : Base.Component, IDrawableComponent
    {
        public Action<SKCanvas> DrawAction { get; }
        
        public DrawableComponent(Action<SKCanvas> drawAction)
        {
            DrawAction = drawAction;
        }

        public void Draw(SKCanvas canvas)
        {
            DrawAction.Invoke(canvas);
        }
    }
}
