namespace SkiEngine
{
    public static class SkiEngineInitializer
    {
        public static void Init(ISkiEngineInitializer initializer)
        {
            SkiFile.OpenAppPackageFileFunc = initializer.OpenAppPackageFileAsync;
            Display.DensityFunc = () => initializer.DisplayDensity;
            MainThread.InvokeOnMainThreadFunc = initializer.InvokeOnMainThreadAsync;
        }
    }
}
