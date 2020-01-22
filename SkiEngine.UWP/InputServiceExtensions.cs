using System.Linq;
using Windows.Devices.Input;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using SkiEngine.Input;
using Page = Windows.UI.Xaml.Controls.Page;

namespace SkiEngine.UWP
{
    public static class InputServiceExtensions
    {
        public static SkiInputService CreateSkiInputService(this Page _)
        {
            var window = CoreWindow.GetForCurrentThread();

            var inputService = new SkiInputService
            {
                CalculateNumberOfMousePointersFunc = () => 
                    PointerDevice
                        .GetPointerDevices()
                        .Count(pointer => pointer.PointerDeviceType == PointerDeviceType.Mouse),
                IsInputViewFocusedFunc = () => FocusManager.GetFocusedElement() is TextBox
            };
            
            window.KeyDown += (sender, args) =>
            {
                var intKey = (int) args.VirtualKey;
                var skiVirtualKey = (SkiVirtualKey) intKey;
                inputService.OnKeyDown(skiVirtualKey);
            };

            window.KeyUp += (sender, args) =>
            {
                var intKey = (int) args.VirtualKey;
                var skiVirtualKey = (SkiVirtualKey) intKey;
                inputService.OnKeyUp(skiVirtualKey);
            };

            return inputService;
        }
    }
}
