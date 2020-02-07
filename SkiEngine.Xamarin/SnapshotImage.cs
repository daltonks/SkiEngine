using System;
using System.Diagnostics;
using System.Threading;
using SkiaSharp;

namespace SkiEngine.Xamarin
{
    public class SnapshotImage
    {
        private volatile int _activeUsers = 1;

        public SnapshotImage(SKImage skImage, SKSizeI size)
        {
            SkImage = skImage;
            Size = size;
        }

        public SKImage SkImage { get; }
        public SKSizeI Size { get; }

        public void AddUser()
        {
            Interlocked.Increment(ref _activeUsers);
        }

        public void RemoveUser()
        {
            if (Interlocked.Decrement(ref _activeUsers) == 0)
            {
                SkImage.Dispose();
            }
        }

        public SKColor? GetPixelColor(int pixelX, int pixelY)
        {
            if (pixelX < 0 || pixelX >= Size.Width || pixelY < 0 || pixelY >= Size.Height)
            {
                return null;
            }

            try
            {
                using (var pixels = SkImage.PeekPixels())
                {
                    return pixels.GetPixelColor(pixelX, pixelY);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                return null;
            }
        }
    }
}
