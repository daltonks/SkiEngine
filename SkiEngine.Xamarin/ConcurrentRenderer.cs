using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using SkiEngine.Util;

namespace SkiEngine.Xamarin
{
    public delegate void OffUiThreadDrawDelegate(SKSurface surface, ConcurrentRenderer.SnapshotHandler snapshotHandler, double widthXamarinUnits, double heightXamarinUnits);
    public delegate void OnUiThreadDrawDelegate(SKSurface surface, IReadOnlyList<SnapshotImage> snapshots);

    public class ConcurrentRenderer : IDisposable
    {
        private readonly TaskQueue _taskQueue = new TaskQueue();
        private readonly OffUiThreadDrawDelegate _offUiThreadDrawAction;
        private readonly OnUiThreadDrawDelegate _onUiThreadDrawDelegate;
        private readonly Action _invalidateSurfaceAction;
        private readonly int _delayBetweenRendersMillis;

        private volatile bool _pendingDraw;
        private SKSurface _offUiThreadSurface;

        private readonly object _snapshotLock = new object();
        private readonly SnapshotHandler _snapshotHandler;
        private readonly List<SnapshotImage> _snapshotImages = new List<SnapshotImage>(1);
       
        private int _widthPixels;
        private int _heightPixels;
        private double _widthXamarinUnits;
        private double _heightXamarinUnits;

        public ConcurrentRenderer(
            OffUiThreadDrawDelegate offUiThreadDrawAction, 
            OnUiThreadDrawDelegate onUiThreadDrawDelegate,
            Action invalidateSurfaceAction,
            int delayBetweenRendersMillis = 0
        )
        {
            _offUiThreadDrawAction = offUiThreadDrawAction;
            _onUiThreadDrawDelegate = onUiThreadDrawDelegate;
            _invalidateSurfaceAction = invalidateSurfaceAction;
            _delayBetweenRendersMillis = delayBetweenRendersMillis;

            _snapshotHandler = new SnapshotHandler(this);
        }

        public SnapshotImage AddSnapshotImageUser(int index)
        {
            lock (_snapshotLock)
            {
                if (index < _snapshotImages.Count)
                {
                    var snapshotImage = _snapshotImages[index];
                    snapshotImage.AddUser();
                    return snapshotImage;
                }

                return new SnapshotImage(
                    SKImage.Create(new SKImageInfo(1, 1)),
                    new SKSizeI(0, 0)
                );
            }
        }

        private readonly object _pendingDrawLock = new object();
        public bool TryQueueDraw()
        {
            var shouldDraw = false;

            lock (_pendingDrawLock)
            {
                if (!_pendingDraw)
                {
                    _pendingDraw = true;
                    shouldDraw = true;
                }
            }

            if (shouldDraw)
            {
                _taskQueue.QueueAsync(ConcurrentDrawAndInvalidateSurfaceAsync);
            }

            return shouldDraw;
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public void OnPaintSurface(SKPaintSurfaceEventArgs e, double widthXamarinUnits, double heightXamarinUnits)
        {
            lock (_snapshotLock)
            {
                _onUiThreadDrawDelegate(e.Surface, _snapshotImages);
            }

            var imageInfo = e.Info;
            var widthPixels = imageInfo.Width;
            var heightPixels = imageInfo.Height;

            if (_widthPixels == widthPixels 
                && _heightPixels == heightPixels 
                && _widthXamarinUnits == widthXamarinUnits 
                && _heightXamarinUnits == heightXamarinUnits)
            {
                return;
            }

            // Canvas size has changed

            _widthPixels = widthPixels;
            _heightPixels = heightPixels;
            _widthXamarinUnits = widthXamarinUnits;
            _heightXamarinUnits = heightXamarinUnits;
            
            _taskQueue.QueueAsync(() => {
                // Recreate _offUiThreadSurface
                _offUiThreadSurface?.Dispose();
                _offUiThreadSurface = SKSurface.Create(
                    new SKImageInfo(
                        _widthPixels,
                        _heightPixels,
                        SKImageInfo.PlatformColorType,
                        SKAlphaType.Premul
                    )
                );

                // Redraw
                return ConcurrentDrawAndInvalidateSurfaceAsync();
            });
        }

        private async Task ConcurrentDrawAndInvalidateSurfaceAsync()
        {
            if (_offUiThreadSurface == null)
            {
                return;
            }

            await Task.Delay(_delayBetweenRendersMillis);

            _pendingDraw = false;

            _snapshotHandler.Reset();
            _offUiThreadDrawAction(_offUiThreadSurface, _snapshotHandler, _widthXamarinUnits, _heightXamarinUnits);
            _snapshotHandler.DisposeExtraSnapshots();

            _invalidateSurfaceAction.Invoke();
        }

        public async void Dispose()
        {
            await _taskQueue.ShutdownAsync();

            _offUiThreadSurface?.Dispose();

            foreach (var snapshotImage in _snapshotImages)
            {
                snapshotImage.RemoveUser();
            }
        }

        public class SnapshotHandler
        {
            private int _index;

            private readonly ConcurrentRenderer _renderer;

            public SnapshotHandler(ConcurrentRenderer renderer)
            {
                _renderer = renderer;
            }

            internal void Reset()
            {
                _index = 0;
            }

            public void Snapshot()
            {
                var skImage = _renderer._offUiThreadSurface.Snapshot();

                lock (_renderer._snapshotLock)
                {
                    var snapshotImage = new SnapshotImage(
                        skImage, 
                        new SKSizeI(_renderer._widthPixels, _renderer._heightPixels)
                    );

                    if (_index < _renderer._snapshotImages.Count)
                    {
                        _renderer._snapshotImages[_index].RemoveUser();
                        _renderer._snapshotImages[_index] = snapshotImage;
                    }
                    else
                    {
                        _renderer._snapshotImages.Add(snapshotImage);
                    }
                }

                _index++;
            }

            internal void DisposeExtraSnapshots()
            {
                lock (_renderer._snapshotLock)
                {
                    for (var i = _index; i < _renderer._snapshotImages.Count;)
                    {
                        var snapshot = _renderer._snapshotImages[i];
                        snapshot.RemoveUser();
                        _renderer._snapshotImages.RemoveAt(i);
                    }
                }
            }
        }
    }
}
