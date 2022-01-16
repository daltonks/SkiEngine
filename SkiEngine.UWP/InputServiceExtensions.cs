using System.Linq;
using Windows.Devices.Input;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using SkiEngine.Input;
using Page = Windows.UI.Xaml.Controls.Page;

namespace SkiEngine.UWP
{
    public static class InputServiceExtensions
    {
        public static void InitializeSkiInputService(this Page _)
        {
            var window = CoreWindow.GetForCurrentThread();
            
            SkiInputService.Instance.CalculateNumberOfMousePointersFunc = () =>
                PointerDevice
                    .GetPointerDevices()
                    .Count(pointer => pointer.PointerDeviceType == PointerDeviceType.Mouse);

            SkiInputService.Instance.IsInputViewFocusedFunc = () => FocusManager.GetFocusedElement() is TextBox;

            window.Dispatcher.AcceleratorKeyActivated += (sender, args) =>
            {
                var intKey = (int) args.VirtualKey;
                var skiVirtualKey = (SkiVirtualKey) intKey;
                var keyState = window.GetKeyState(args.VirtualKey);
                
                if (keyState.HasFlag(CoreVirtualKeyStates.Down))
                {
                    SkiInputService.Instance.OnKeyDown(skiVirtualKey);
                }
                else
                {
                    SkiInputService.Instance.OnKeyUp(skiVirtualKey);
                }
            };
        }
    }
}
