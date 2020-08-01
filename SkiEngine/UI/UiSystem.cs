using System.Linq;
using SkiEngine.Input;
using SkiEngine.Util;

namespace SkiEngine.UI
{
    public class UiSystem : ISystem
    {
        private readonly LayeredSets<float, SkiUiComponent> _components = new LayeredSets<float, SkiUiComponent>(c => -c.Node.WorldZ);

        public void OnNodeZChanged(Node node, float previousZ)
        {
            foreach (var uiComponent in node.Components.OfType<SkiUiComponent>())
            {
                _components.Update(uiComponent, -previousZ);
            }
        }

        public void OnComponentCreated(IComponent component)
        {
            if (component is SkiUiComponent uiComponent)
            {
                _components.Add(uiComponent);
            }
        }

        public void OnComponentDestroyed(IComponent component)
        {
            if (component is SkiUiComponent uiComponent)
            {
                _components.Remove(uiComponent);
            }
        }

        public void OnTouch(SkiTouch touch)
        {
            foreach (var component in _components)
            {
                component.OnTouch(touch);
            }
        }
    }
}
