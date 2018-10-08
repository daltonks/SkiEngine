using SkiEngine.NCS.Component.Base;

namespace SkiEngine.NCS.Component
{
    public class DrawableComponent : Base.Component, IDrawableComponent
    {
        public DrawableComponentPart DrawablePart { get; }

        public DrawableComponent(DrawableComponentPart.DrawDelegate drawAction)
        {
            DrawablePart = new DrawableComponentPart(drawAction);
        }
    }
}
