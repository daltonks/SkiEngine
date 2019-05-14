using System.Collections.Generic;
using SkiEngine.Input;
using Page = Windows.UI.Xaml.Controls.Page;

namespace SkiEngine.UWP
{
    public static class UwpPageExtensions
    {
        public static InputService CreateInputService(this Page page)
        {
            var inputService = new InputService();
            
            page.PointerWheelChanged += (sender, args) =>
            {
                var delta = args.GetCurrentPoint(page).Properties.MouseWheelDelta;
                inputService.OnMouseWheelScroll(delta);
            };

            page.KeyDown += (sender, args) =>
            {
                var intKey = (int) args.Key;
                var skiVirtualKey = (SkiVirtualKey) intKey;
                inputService.OnKeyDown(skiVirtualKey);
            };

            page.KeyUp += (sender, args) =>
            {
                var intKey = (int) args.Key;
                var skiVirtualKey = (SkiVirtualKey) intKey;
                inputService.OnKeyUp(skiVirtualKey);
            };

            var enteredPointers = new HashSet<uint>();
            page.PointerEntered += (sender, args) =>
            {
                lock (enteredPointers)
                {
                    enteredPointers.Add(args.Pointer.PointerId);
                    inputService.NumPointersOnWindow = enteredPointers.Count;
                }
            };

            page.PointerExited += (sender, args) =>
            {
                lock (enteredPointers)
                {
                    enteredPointers.Remove(args.Pointer.PointerId);
                    inputService.NumPointersOnWindow = enteredPointers.Count;
                }
            };

            return inputService;
        }
    }
}
