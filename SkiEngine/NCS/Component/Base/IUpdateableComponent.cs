namespace SkiEngine.NCS.Component.Base
{
    public interface IUpdateableComponent : IComponent
    {
        UpdateableComponentPart UpdateablePart { get; }
    }
}
