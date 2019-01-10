﻿using SkiaSharp.Views.Forms;
using SkiEngine.Touch;
// ReSharper disable InconsistentNaming

namespace SkiEngine.Xamarin
{
    public static class SKTouchEventArgsExtensions
    {
        public static SKTouch ToSKTouch(this SKTouchEventArgs eventArgs)
        {
            return new SKTouch(
                eventArgs.Id,
                (Touch.SKTouchAction) (int) eventArgs.ActionType,
                (Touch.SKMouseButton) (int) eventArgs.MouseButton,
                (Touch.SKTouchDeviceType) (int) eventArgs.DeviceType,
                eventArgs.Location,
                eventArgs.InContact
            );
        }
    }
}
