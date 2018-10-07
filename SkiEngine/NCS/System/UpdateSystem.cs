using System;
using System.Collections.Generic;
using SkiEngine.Interfaces;
using SkiEngine.NCS.Component.Base;
using SkiEngine.Util;

namespace SkiEngine.NCS.System
{
    public class UpdateSystem : ISystem
    {
        private readonly LayeredSets<int, UpdateableComponentPart> _layeredUpdateableParts = 
            new LayeredSets<int, UpdateableComponentPart>(componentPart => componentPart.UpdateOrder);

        private bool _currentlyUpdating;
        private readonly List<IComponent> _componentsToAdd = new List<IComponent>();
        private readonly List<IComponent> _componentsToRemove = new List<IComponent>();

        public void OnComponentCreated(IComponent component)
        {
            if (_currentlyUpdating)
            {
                _componentsToAdd.Add(component);
            }
            else if(component is IUpdateableComponent updateableComponent)
            {
                _layeredUpdateableParts.Add(updateableComponent.UpdateablePart);
                updateableComponent.UpdateablePart.UpdateOrderChanged += PartUpdateOrderChanged;
            }
        }

        public void OnComponentDestroyed(IComponent component)
        {
            if (_currentlyUpdating)
            {
                _componentsToRemove.Add(component);
            }
            else if(component is IUpdateableComponent updateableComponent)
            {
                _layeredUpdateableParts.Remove(updateableComponent.UpdateablePart);
                updateableComponent.UpdateablePart.UpdateOrderChanged -= PartUpdateOrderChanged;
            }
        }

        private void PartUpdateOrderChanged(UpdateableComponentPart updateablePart, int previousUpdateOrder)
        {
            _layeredUpdateableParts.Update(updateablePart, previousUpdateOrder);
        }

        public void Update(UpdateTime updateTime)
        {
            _currentlyUpdating = true;
            foreach(var updateableComponent in _layeredUpdateableParts)
            {
                updateableComponent.Update(updateTime);
            }

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

        public void Draw(UpdateTime updateTime)
        {
            
        }
    }

    public class UpdateTime
    {
        public TimeSpan Delta { get; internal set; }
    }
}
