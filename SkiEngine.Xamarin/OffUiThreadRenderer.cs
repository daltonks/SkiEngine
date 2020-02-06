using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using SkiEngine.Util;

namespace SkiEngine.Xamarin
{
    public delegate void DrawDelegate(SKSurface surface, OffUiThreadRenderer.SnapshotHandler snapshotHandler, double widthXamarinUnits, double heightXamarinUnits);

    public class OffUiThreadRenderer : IDisposable
    {
        private readonly TaskQueue _taskQueue = new TaskQueue();
        private readonly DrawDelegate _drawAction;
        private readonly Action _invalidateSurfaceAction;

        private readonly object _pendingDrawLock = new object();
        private volatile bool _pendingDraw;
        private SKSurface _offUiThreadSurface;

        private readonly object _snapshotLock = new object();
        private readonly List<SnapshotImage> _snapshotImages = new List<SnapshotImage>();
       
        private int _widthPixels;
        private int _heightPixels;
        private double _widthXamarinUnits;
        private double _heightXamarinUnits;

        public OffUiThreadRenderer(DrawDelegate drawAction, Action invalidateSurfaceAction)
        {
            _drawAction = drawAction;
            _invalidateSurfaceAction = invalidateSurfaceAction;
        }

        public SnapshotImage AddSnapshotImageUser(int index)
        {
            lock (_snapshotLock)
            {
                var snapshotImage = index < _snapshotImages.Count
                    ? _snapshotImages[index]
                    : new SnapshotImage(
                        SKImage.Create(new SKImageInfo(1, 1)),
                        new SKSizeI(0, 0)
                    );

                snapshotImage.AddUser();
                return snapshotImage;
            }
        }

        public void QueueExpensiveDraw()
        {
            var actuallyDraw = false;

            lock (_pendingDrawLock)
            {
                if (!_pendingDraw)
                {
                    _pendingDraw = true;
                    actuallyDraw = true;
                }
            }

            if (actuallyDraw)
            {
                _taskQueue.QueueAsync(DrawToSnapshotAndInvalidateSurface);
            }
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public void OnPaintSurface(SKPaintSurfaceEventArgs e, double widthXamarinUnits, double heightXamarinUnits)
        {
            lock (_snapshotLock)
            {
                foreach (var snapshotImage in _snapshotImages)
                {
                    e.Surface.Canvas.DrawImage(snapshotImage.SkImage, 0, 0);
                }
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
                DrawToSnapshotAndInvalidateSurface();
            });
        }

        private void DrawToSnapshotAndInvalidateSurface()
        {
            if (_offUiThreadSurface == null)
            {
                return;
            }

            _pendingDraw = false;

            using (var snapshotHandler = new SnapshotHandler(this))
            {
                _drawAction(_offUiThreadSurface, snapshotHandler, _widthXamarinUnits, _heightXamarinUnits);
            }
            
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

        public class SnapshotHandler : IDisposable
        {
            private int _index;

            private readonly OffUiThreadRenderer _renderer;

            public SnapshotHandler(OffUiThreadRenderer renderer)
            {
                _renderer = renderer;
            }

            public void Snapshot()
            {
                lock (_renderer._snapshotLock)
                {
                    var snapshotImage = new SnapshotImage(
                        _renderer._offUiThreadSurface.Snapshot(), 
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
                    
                    snapshotImage.AddUser();
                }

                _index++;
            }

            public void Dispose()
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
