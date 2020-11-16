using SkiEngine.UI;

namespace SkiEngine
{
    public static class SkiEngineInitializer
    {
        public static void Init(ISkiEngineInitializer initializer)
        {
            SkiUiScene.AllowInvalidateSurfaceIfDrawStillPending = initializer.AllowInvalidateSurfaceIfDrawStillPending;
            SkiFile.OpenAppPackageFileFunc = initializer.OpenAppPackageFileAsync;
            Display.DensityFunc = () => initializer.DisplayDensity;
            MainThread.InvokeOnMainThreadFunc = initializer.InvokeOnMainThreadAsync;
        }
    }
}
