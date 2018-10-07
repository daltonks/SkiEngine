using System;
using System.Collections.Generic;
using SkiaSharp;

namespace SkiEngine.NCS.Component.Base
{
    public class InputComponentPart
    {
        public delegate void HandlingOrderChangedDelegate(InputComponentPart componentPart, int previousHandlingOrder);

        public event HandlingOrderChangedDelegate HandlingOrderChanged;

        public List<Func<SKPoint, bool>> RightMouseClickHandlers;
        public List<Func<SKPoint, bool>> RightMouseUpHandlers;

        private int _handlingOrder;

        public int HandlingOrder
        {
            get => _handlingOrder;
            set
            {
                var previousHandlingOrder = _handlingOrder;
                if (value == previousHandlingOrder)
                {
                    return;
                }

                _handlingOrder = value;
                HandlingOrderChanged?.Invoke(this, previousHandlingOrder);
            }
        }
    }
}
