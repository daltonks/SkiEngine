using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SkiEngine.Xamarin
{
    public static class SkiXamarinForms
    {
        public static void Init()
        {
            SkiEngineInitializer.Init(new Initializer());
        }

        private class Initializer : ISkiEngineInitializer
        {
            public double DisplayDensity => DeviceDisplay.MainDisplayInfo.Density;
            public bool AllowInvalidateSurfaceIfDrawStillPending => Device.RuntimePlatform == Device.iOS;

            public Task InvokeOnMainThreadAsync(Action action)
            {
                return global::Xamarin.Essentials.MainThread.InvokeOnMainThreadAsync(action);
            }

            public Task<Stream> OpenAppPackageFileAsync(string path)
            {
                return FileSystem.OpenAppPackageFileAsync(path);
            }
        }
    }
}
