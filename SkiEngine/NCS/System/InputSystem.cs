using System.Collections.Generic;
using SkiEngine.Interfaces;
using SkiEngine.NCS.Component.Base;
using SkiEngine.Util;

namespace SkiEngine.NCS.System
{
    public class InputSystem : ISystem, IUpdateable
    {
        private readonly LayeredSets<int, InputComponentPart> _layeredComponentParts = 
            new LayeredSets<int, InputComponentPart>(inputPart => inputPart.HandlingOrder);

        private bool _currentlyUpdating;
        private readonly List<IComponent> _componentsToAdd = new List<IComponent>();
        private readonly List<IComponent> _componentsToRemove = new List<IComponent>();

        public void OnNodeZChanged(Node node, int previousZ)
        {
            
        }

        public void OnComponentCreated(IComponent component)
        {
            if (_currentlyUpdating)
            {
                _componentsToAdd.Add(component);
            }
            else if(component is IInputComponent inputComponent)
            {
                _layeredComponentParts.Add(inputComponent.InputPart);
                inputComponent.InputPart.HandlingOrderChanged += PartHandlingOrderChanged;
            }
        }

        public void OnComponentDestroyed(IComponent component)
        {
            if (_currentlyUpdating)
            {
                _componentsToRemove.Add(component);
            }
            else if(component is IInputComponent inputComponent)
            {
                _layeredComponentParts.Remove(inputComponent.InputPart);
                inputComponent.InputPart.HandlingOrderChanged -= PartHandlingOrderChanged;
            }
        }

        private void PartHandlingOrderChanged(InputComponentPart inputPart, int previousLayer)
        {
            _layeredComponentParts.Update(inputPart, previousLayer);
        }

        public void Update(UpdateTime updateTime)
        {
            _currentlyUpdating = true;

            _currentlyUpdating = false;

            foreach (var componentToAdd in _componentsToAdd)
            {
                OnComponentCreated(componentToAdd);
            }
            _componentsToAdd.Clear();

            foreach (var componentToRemove in _componentsToRemove)
            {
                OnComponentDestroyed(componentToRemove);
            }
            _componentsToRemove.Clear();
        }
    }
}
