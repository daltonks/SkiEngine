using SkiaSharp;
using SkiEngine.NCS.Component.Base;
using SkiEngine.NCS.Component.Camera;

namespace SkiEngine.NCS.Component
{
    public class DrawableComponent : Base.Component, IDrawableComponent
    {
        public delegate void DrawDelegate(SKCanvas canvas, CameraComponent camera);

        public DrawDelegate DrawAction { get; }
        
        public DrawableComponent(DrawDelegate drawAction)
        {
            DrawAction = drawAction;
        }

        public void Draw(SKCanvas canvas, CameraComponent camera)
        {
            DrawAction.Invoke(canvas, camera);
        }
    }
}
