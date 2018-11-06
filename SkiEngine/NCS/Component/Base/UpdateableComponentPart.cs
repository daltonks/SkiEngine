using System;
using SkiEngine.NCS.System;

namespace SkiEngine.NCS.Component.Base
{
    public class UpdateableComponentPart
    {
        public delegate void UpdateOrderChangedDelegate(UpdateableComponentPart componentPart, int previousUpdateOrder);

        public event Action<UpdateableComponentPart> CanUpdateChanged;
        public event UpdateOrderChangedDelegate UpdateOrderChanged;

        private bool _canUpdate;
        private int _updateOrder;
        private readonly Action<UpdateTime> _onUpdateAction;

        public UpdateableComponentPart(Action<UpdateTime> onUpdateAction)
        {
            _onUpdateAction = onUpdateAction;
        }

        public bool CanUpdate
        {
            get => _canUpdate;
            set
            {
                if (value == _canUpdate)
                {
                    return;
                }

                _canUpdate = value;
                CanUpdateChanged?.Invoke(this);
            }
        }

        public int UpdateOrder
        {
            get => _updateOrder;
            set
            {
                var previousUpdateOrder = _updateOrder;
                if (value == previousUpdateOrder)
                {
                    return;
                }

                _updateOrder = value;
                UpdateOrderChanged?.Invoke(this, previousUpdateOrder);
            }
        }

        public void Update(UpdateTime updateTime)
        {
            _onUpdateAction.Invoke(updateTime);
        }
    }
}
