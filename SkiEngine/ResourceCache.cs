using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SkiEngine
{
    public static class ResourceCache
    {
        private static readonly Dictionary<string, CachedResource> Resources = new Dictionary<string, CachedResource>();

        public static long UnusedBytes { get; private set; }
        public static long UnusedBytesLimit { get; set; } = 100_000_000;
        private static readonly HashSet<CachedResource> UnusedResources = new HashSet<CachedResource>();

        public static Task<CachedResourceUsage<T>> GetAppPackageFileAsync<T>(string path, Func<Stream, Task<T>> transform)
        {
            return GetAsync(
                "PACKAGE_FILE",
                path,
                () => SkiFile.OpenAppPackageFileAsync(path),
                transform
            );
        }

        public static async Task<CachedResourceUsage<TResource>> GetAsync<TResource, TStream>(
            string group, 
            string name, 
            Func<Task<TStream>> getStream, 
            Func<Stream, Task<TResource>> transform
        ) where TStream : Stream
        {
            var key = $"{group}/{name}";

            lock (Resources)
            {
                if (Resources.TryGetValue(key, out var resource))
                {
                    return new CachedResourceUsage<TResource>(resource);
                }
            }

            using var stream = await getStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            var value = await transform(memoryStream);
            var valueBytes = memoryStream.Length;

            lock (Resources)
            {
                if (!Resources.TryGetValue(key, out var resource))
                {
                    resource = Resources[key] = new CachedResource(key, value, valueBytes);
                }

                return new CachedResourceUsage<TResource>(resource);
            }
        }

        internal static void AddUsage(CachedResource resource)
        {
            lock (UnusedResources)
            {
                resource.Usages++;
                if (resource.Usages == 1 && UnusedResources.Remove(resource))
                {
                    UnusedBytes -= resource.Bytes;
                }
            }
        }

        internal static void RemoveUsage(CachedResource resource)
        {
            lock (UnusedResources)
            {
                resource.Usages--;
                if (resource.Usages == 0)
                {
                    UnusedResources.Add(resource);
                    UnusedBytes += resource.Bytes;

                    if (UnusedBytes > UnusedBytesLimit)
                    {
                        // Remove least recently used resources until remaining are under the byte limit
                        var resourcesToRemove = new List<CachedResource>();
                        foreach (var unusedResource in UnusedResources.OrderBy(r => r.LastAccessed))
                        {
                            resourcesToRemove.Add(unusedResource);
                            UnusedBytes -= unusedResource.Bytes;
                            if (UnusedBytes <= UnusedBytesLimit)
                            {
                                break;
                            }
                        }

                        lock (Resources)
                        {
                            foreach (var resourceToRemove in resourcesToRemove)
                            {
                                resourceToRemove.Dispose();
                                Resources.Remove(resourceToRemove.Key);
                                UnusedResources.Remove(resourceToRemove);
                            }
                        }
                    }
                }
            }
        }
    }

    public class CachedResource : IDisposable
    {
        public CachedResource(string key, object value, long bytes)
        {
            Key = key;
            _value = value;
            Bytes = bytes;
            LastAccessed = DateTimeOffset.UtcNow;
        }

        public string Key { get; }

        private readonly object _value;
        public object Value
        {
            get
            {
                LastAccessed = DateTimeOffset.UtcNow;
                return _value;
            }
        }

        public long Bytes { get; }
        public int Usages { get; set; }
        public DateTimeOffset LastAccessed { get; private set; }

        public void Dispose()
        {
            if (Value is IDisposable disposableValue)
            {
                disposableValue.Dispose();
            }
        }
    }

    public class CachedResourceUsage<T> : IDisposable
    {
        private bool _isDisposed;
        private readonly CachedResource _resource;

        public CachedResourceUsage(CachedResource resource)
        {
            _resource = resource;
            ResourceCache.AddUsage(_resource);
        }

        public T Value => (T) _resource.Value;

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            ResourceCache.RemoveUsage(_resource);
        }
    }
}
