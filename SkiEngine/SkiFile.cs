using System;
using System.IO;
using System.Threading.Tasks;

namespace SkiEngine
{
    public static class SkiFile
    {
        public static Func<string, Task<Stream>> OpenAppPackageFileFunc { get; set; }

        public static Task<Stream> OpenAppPackageFileAsync(string path)
        {
            return OpenAppPackageFileFunc(path);
        }
    }
}
