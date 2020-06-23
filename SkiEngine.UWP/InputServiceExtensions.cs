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

            window.KeyDown += (sender, args) =>
            {
                var intKey = (int) args.VirtualKey;
                var skiVirtualKey = (SkiVirtualKey) intKey;
                SkiInputService.Instance.OnKeyDown(skiVirtualKey);
            };

            window.KeyUp += (sender, args) =>
            {
                var intKey = (int) args.VirtualKey;
                var skiVirtualKey = (SkiVirtualKey) intKey;
                SkiInputService.Instance.OnKeyUp(skiVirtualKey);
            };
        }
    }
}
