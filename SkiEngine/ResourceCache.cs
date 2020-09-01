﻿using System;
using System.Collections.Generic;
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
        public static long UnusedBytesLimit { get; set; } = 100_000_000;
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

        public static CachedResourceUsage<TResource> Get<TResource, TStream>(
            string group, 
            string name, 
            Func<Task<TStream>> getStream, 
            Func<Stream, Task<TResource>> transform
        ) where TStream : Stream
        {
            var key = $"{group}:{name}";

            lock (Resources)
            {
                if (Resources.TryGetValue(key, out var resource))
                {
                    return new CachedResourceUsage<TResource>(Task.FromResult(resource));
                }
            }

            return new CachedResourceUsage<TResource>(LoadResourceAsync());

            async Task<CachedResource> LoadResourceAsync()
            {
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

                    return resource;
                }
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
        private readonly Task<CachedResource> _loadingTask;

        internal CachedResourceUsage(Task<CachedResource> loadingTask)
        {
            _loadingTask = loadingTask.ContinueWith(t => {
                var cachedResource = t.Result;
                Value = (T) cachedResource.Value;
                ResourceCache.AddUsage(cachedResource);
                return cachedResource;
            });
        }

        public bool IsDisposed { get; private set; }

        public T Value { get; private set; }

        public async Task WaitForLoadingAsync()
        {
            await _loadingTask;
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;

            _loadingTask.ContinueWith(t => {
                ResourceCache.RemoveUsage(t.Result);
            });
        }
    }
}
