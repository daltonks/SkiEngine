using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SkiaSharp;

namespace SkiEngine
{
    public static class ResourceCache
    {
        private static readonly Dictionary<string, CachedResource> Resources = new Dictionary<string, CachedResource>();

        public static long UnusedBytes { get; private set; }
        public static long UnusedBytesLimit { get; set; } = 0;
        private static readonly HashSet<CachedResource> UnusedResources = new HashSet<CachedResource>();

        // ReSharper disable once InconsistentNaming
        public static CachedResourceUsage<SKImage> GetAppPackageSKImage(string path)
        {
            return Get(
                $"{nameof(ResourceCache)}-AppPackage-{nameof(SKImage)}",
                path,
                () => SkiFile.OpenAppPackageFileAsync(path),
                stream => Task.FromResult(SKImage.FromEncodedData(stream))
            );
        }

        public static CachedResourceUsage<TResource> Get<TResource>(
            string group, 
            string name, 
            Func<Task<Stream>> getStream, 
            Func<MemoryStream, Task<TResource>> transform
        )
        {
            var key = $"{group}:{name}";

            CachedResource resource;

            lock (Resources)
            {
                if (!Resources.TryGetValue(key, out resource))
                {
                    resource = Resources[key] = new CachedResource(
                        key, 
                        getStream, 
                        async memoryStream => await transform(memoryStream)
                    );
                }
            }

            return new CachedResourceUsage<TResource>(async () => {
                await resource.WaitForLoadAsync();
                return resource;
            });
        }

        public static void ClearAllUnusedResources()
        {
            lock (UnusedResources)
            lock (Resources)
            {
                foreach (var unusedResource in UnusedResources)
                {
                    Resources.Remove(unusedResource.Key);
                    unusedResource.Dispose();
                }

                UnusedResources.Clear();
                UnusedBytes = 0;
            }
        }

        internal static void AddUsage(CachedResource resource, Guid usageId)
        {
            lock (UnusedResources)
            {
                resource.Usages.Add(usageId);
                if (resource.Usages.Count == 1 && UnusedResources.Remove(resource))
                {
                    UnusedBytes -= resource.Bytes;
                }
            }
        }

        internal static void RemoveUsage(CachedResource resource, Guid usageId)
        {
            lock (UnusedResources)
            {
                if (resource.Usages.Remove(usageId) && resource.Usages.Count == 0)
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
        private readonly Task _loadTask;

        public CachedResource(string key, Func<Task<Stream>> getStream, Func<MemoryStream, Task<object>> transform)
        {
            Key = key;
            LastAccessed = DateTimeOffset.UtcNow;

            _loadTask = LoadAsync(getStream, transform);
        }

        private async Task LoadAsync(Func<Task<Stream>> getStream, Func<MemoryStream, Task<object>> transform)
        {
            try
            {
                using var stream = await getStream();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);

                Bytes = memoryStream.Position;
                memoryStream.Position = 0;

                Value = await transform(memoryStream);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                Value = default;
                Bytes = 0;
            }
        }

        public string Key { get; }

        private object _value;
        public object Value
        {
            get
            {
                LastAccessed = DateTimeOffset.UtcNow;
                return _value;
            }
            private set => _value = value;
        }

        public long Bytes { get; private set; }
        public DateTimeOffset LastAccessed { get; private set; }
        public HashSet<Guid> Usages { get; } = new HashSet<Guid>();

        public Task WaitForLoadAsync()
        {
            return _loadTask;
        }

        public void Dispose()
        {
            _ = Task.Run(async () => {
                await _loadTask;
                if (Value is IDisposable disposableValue)
                {
                    disposableValue.Dispose();
                }
            });
        }
    }

    public class CachedResourceUsage<T> : IDisposable
    {
        private readonly Task<CachedResource> _loadingTask;
        private readonly object _usageLock = new object();

        internal CachedResourceUsage(Func<Task<CachedResource>> loadingTask)
        {
            _loadingTask = loadingTask.Invoke().ContinueWith(t => {
                lock (_usageLock)
                {
                    if (IsDisposed)
                    {
                        return null;
                    }

                    var cachedResource = t.Result;
                    Value = (T) cachedResource.Value;
                    ResourceCache.AddUsage(cachedResource, Id);
                    return cachedResource;
                }
            });
        }

        public Guid Id { get; } = Guid.NewGuid();
        public T Value { get; private set; }
        public bool LoadingIsFinished => _loadingTask.IsCompleted;
        public bool IsDisposed { get; private set; }

        public async Task<T> GetValueAsync()
        {
            await _loadingTask;
            return Value;
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;

            _loadingTask.ContinueWith(t => {
                var cachedResource = t.Result;
                if (cachedResource != null)
                {
                    ResourceCache.RemoveUsage(t.Result, Id);
                }
            });
        }

        ~CachedResourceUsage()
        {
            if (!IsDisposed)
            {
                lock (_usageLock)
                {
                    IsDisposed = true;
                    ResourceCache.RemoveUsage(_loadingTask.Result, Id);
                }
            }
        }
    }
}
