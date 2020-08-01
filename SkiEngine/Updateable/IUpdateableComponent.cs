namespace SkiEngine.Updateable
{
    public interface IUpdateableComponent : IComponent
    {
        UpdateableComponentPart UpdateablePart { get; }
    }
}
