using System;
using System.IO;
using SkiaSharp;

namespace SkiEngine.Util
{
    // ReSharper disable once InconsistentNaming
    public static class SKImageUtil
    {
        // https://github.com/mono/SkiaSharp/issues/836#issuecomment-584895517
        public static (SKBitmap SKBitmap, SKEncodedImageFormat EncodedImageFormat) FixImageOrientation(Stream stream)
        {
            using var inputStream = new SKManagedStream(stream);
            using var codec = SKCodec.Create(inputStream);
            var encodedImageFormat = codec.EncodedFormat;
            using var original = SKBitmap.Decode(codec);
            var useWidth = original.Width;
            var useHeight = original.Height;
            Action<SKCanvas> transform = canvas => { };
            switch (codec.EncodedOrigin)
            {
                case SKEncodedOrigin.TopLeft:
                    // No transform needs to be applied.
                    // Return original image.
                    return (original, encodedImageFormat);
                case SKEncodedOrigin.TopRight:
                    // flip along the x-axis
                    transform = canvas => canvas.Scale(-1, 1, useWidth / 2, useHeight / 2);
                    break;
                case SKEncodedOrigin.BottomRight:
                    transform = canvas => canvas.RotateDegrees(180, useWidth / 2, useHeight / 2);
                    break;
                case SKEncodedOrigin.BottomLeft:
                    // flip along the y-axis
                    transform = canvas => canvas.Scale(1, -1, useWidth / 2, useHeight / 2);
                    break;
                case SKEncodedOrigin.LeftTop:
                    useWidth = original.Height;
                    useHeight = original.Width;
                    transform = canvas =>
                    {
                        // Rotate 90
                        canvas.RotateDegrees(90, useWidth / 2, useHeight / 2);
                        canvas.Scale(useHeight * 1.0f / useWidth, -useWidth * 1.0f / useHeight, useWidth / 2, useHeight / 2);
                    };
                    break;
                case SKEncodedOrigin.RightTop:
                    useWidth = original.Height;
                    useHeight = original.Width;
                    transform = canvas =>
                    {
                        // Rotate 90
                        canvas.RotateDegrees(90, useWidth / 2, useHeight / 2);
                        canvas.Scale(useHeight * 1.0f / useWidth, useWidth * 1.0f / useHeight, useWidth / 2, useHeight / 2);
                    };
                    break;
                case SKEncodedOrigin.RightBottom:
                    useWidth = original.Height;
                    useHeight = original.Width;
                    transform = canvas =>
                    {
                        // Rotate 90
                        canvas.RotateDegrees(90, useWidth / 2, useHeight / 2);
                        canvas.Scale(-useHeight * 1.0f / useWidth, useWidth * 1.0f / useHeight, useWidth / 2, useHeight / 2);
                    };
                    break;
                case SKEncodedOrigin.LeftBottom:
                    useWidth = original.Height;
                    useHeight = original.Width;
                    transform = canvas =>
                    {
                        // Rotate 90
                        canvas.RotateDegrees(90, useWidth / 2, useHeight / 2);
                        canvas.Scale(-useHeight * 1.0f / useWidth, -useWidth * 1.0f / useHeight, useWidth / 2, useHeight / 2);
                    };
                    break;
                default:
                    break;
            }

            var imageInfo = new SKImageInfo(useWidth, useHeight, original.ColorType, original.AlphaType, original.ColorSpace);
            using var surface = SKSurface.Create(imageInfo);
            var canvas = surface.Canvas;

            // Transform based on origin
            transform.Invoke(canvas);

            // Draw bitmap
            using var bitmapPaint = new SKPaint
            {
                IsAntialias = true, 
                FilterQuality = SKFilterQuality.High
            };
            canvas.DrawBitmap(original, imageInfo.Rect, bitmapPaint);

            canvas.Flush();

            // Return transformed snapshot
            using var image = surface.Snapshot();
            return (SKBitmap.FromImage(image), encodedImageFormat);
        }
    }
}
