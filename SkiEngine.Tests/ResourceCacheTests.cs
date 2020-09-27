using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SkiEngine.Tests
{
    public class ResourceCacheTests
    {
        [Test]
        public async Task Use_and_unuse()
        {
            ResourceCache.ClearAllUnusedResources();

            ResourceCache.UnusedBytesLimit = 1000;
            
            var resource1Usage = GetTestResourceUsage("resource1");
            await resource1Usage.WaitForLoadingAsync();

            Assert.AreEqual(0, ResourceCache.UnusedBytes);

            // Unused but value not disposed yet because we are under the bytes limit
            resource1Usage.Dispose();

            await Task.Delay(500);
            Assert.AreEqual(100, ResourceCache.UnusedBytes);
            Assert.IsFalse(resource1Usage.Value.IsDisposed);

            resource1Usage = GetTestResourceUsage("resource1");
            await resource1Usage.WaitForLoadingAsync();
            // Used again, so it's removed from the unused collection
            Assert.AreEqual(0, ResourceCache.UnusedBytes);

            // Unused but value not disposed because we are under the bytes limit
            resource1Usage.Dispose();
            await Task.Delay(500);
            Assert.AreEqual(100, ResourceCache.UnusedBytes);
            Assert.IsFalse(resource1Usage.Value.IsDisposed);

            // Add and unuse more resources to put us over the UnusedByteLimit
            // (number doesn't matter as long as it's big enough)
            for (var i = 0; i < 31; i++)
            {
                var resourceUsage = GetTestResourceUsage($"loopResource{i}");
                await resourceUsage.WaitForLoadingAsync();
                resourceUsage.Dispose();
            }

            await Task.Delay(500);

            // resource1 was evicted and its value was disposed
            // since we went over the UnusedBytesLimit
            Assert.IsTrue(resource1Usage.Value.IsDisposed);
            Assert.AreEqual(1000, ResourceCache.UnusedBytes);
        }

        private CachedResourceUsage<TestResource> GetTestResourceUsage(string name)
        {
            return ResourceCache.Get(
                "TEST",
                name,
                () => Task.FromResult(new MemoryStream(new byte[100])),
                stream => Task.FromResult(new TestResource())
            );
        }

        public class TestResource : IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }
    }
}