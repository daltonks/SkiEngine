using SkiaSharp;
using SkiEngine.Interfaces;
using SkiEngine.NCS.System;

namespace SkiEngine.NCS.Component.Base
{
    public class DrawableComponentPart
    {
        public delegate void DrawDelegate(SKCanvas canvas, ITransform transform, UpdateTime updateTime);

        private readonly DrawDelegate _drawAction;

        public DrawableComponentPart(DrawDelegate drawAction)
        {
            _drawAction = drawAction;
        }

        public void Draw(SKCanvas canvas, ITransform transform, UpdateTime updateTime)
        {
            _drawAction.Invoke(canvas, transform, updateTime);
        }
    }
}