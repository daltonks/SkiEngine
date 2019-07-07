using SkiaSharp.Views.Forms;
using SkiEngine.Input;
using SKMouseButton = SkiEngine.Input.SKMouseButton;
using SKTouchAction = SkiEngine.Input.SKTouchAction;
using SKTouchDeviceType = SkiEngine.Input.SKTouchDeviceType;

// ReSharper disable InconsistentNaming

namespace SkiEngine.Xamarin
{
    public static class SKTouchEventArgsExtensions
    {
        public static SkiTouch ToSKTouch(this SKTouchEventArgs eventArgs)
        {
            return SkiTouch.Get(
                eventArgs.Id,
                (SKTouchAction) (int) eventArgs.ActionType,
                (SKMouseButton) (int) eventArgs.MouseButton,
                (SKTouchDeviceType) (int) eventArgs.DeviceType,
                eventArgs.Location,
                eventArgs.InContact
            );
        }
    }
}
