using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace SkiEngine.UI
{
    public class ResourceCache
    {
        private const int MemoryCacheLimitBytes = 100_000_000;

        private readonly MemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions { SizeLimit = MemoryCacheLimitBytes });

        private readonly SkiUiComponent _uiComponent;

        public ResourceCache(SkiUiComponent uiComponent)
        {
            _uiComponent = uiComponent;
        }

        public Task<T> GetAppPackageFileAsync<T>(string path, Func<Stream, Task<T>> transform)
        {
            return GetAsync(
                "PACKAGE_FILE", 
                path, 
                () => _uiComponent.OpenAppPackageFileAsync(path), 
                transform
            );
        }

        public Task<T> GetAsync<T>(string group, string id, Func<Task<Stream>> getStream, Func<Stream, Task<T>> transform)
        {
            return _memoryCache.GetOrCreateAsync(
                $"{group}/{id}",
                async cacheEntry =>
                {
                    using var stream = await getStream();
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    var result = await transform(memoryStream);

                    cacheEntry.Size = memoryStream.Length;
                    if (result is IDisposable disposableResult)
                    {
                        cacheEntry.RegisterPostEvictionCallback((_, __, ___, ____) => disposableResult.Dispose());
                    }
                    
                    return result;
                }
            );
        }
    }
}
