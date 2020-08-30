using Xamarin.Essentials;

namespace SkiEngine.Xamarin
{
    public static class SkiXamarinForms
    {
        public static void Init()
        {
            SkiFile.OpenAppPackageFileFunc = FileSystem.OpenAppPackageFileAsync;
        }
    }
}
