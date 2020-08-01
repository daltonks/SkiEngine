using SkiEngine.Component;

namespace SkiEngine.Updateable
{
    public interface IUpdateableComponent : IComponent
    {
        UpdateableComponentPart UpdateablePart { get; }
    }
}
