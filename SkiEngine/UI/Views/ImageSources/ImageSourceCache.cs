using Microsoft.Extensions.Caching.Memory;

namespace SkiEngine.UI.Views.ImageSources
{
    public class ImageSourceCache
    {
        private const int MemoryCacheLimitBytes = 100_000_000;

        private readonly MemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions { SizeLimit = MemoryCacheLimitBytes });

        private readonly SkiUiComponent _uiComponent;

        public ImageSourceCache(SkiUiComponent uiComponent)
        {
            _uiComponent = uiComponent;
        }
    }
}
