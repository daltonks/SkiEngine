using SkiaSharp;
using SkiEngine.Camera;

namespace SkiEngine.Drawable
{
    public class DrawableComponent : Component.Component, IDrawableComponent
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
