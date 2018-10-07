namespace SkiEngine.NCS.Component.Base
{
    public interface IDrawableComponent : IComponent
    {
        DrawableComponentPart DrawablePart { get; }
    }
}
