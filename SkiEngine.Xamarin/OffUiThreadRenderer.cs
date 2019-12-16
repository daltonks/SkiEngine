using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using SkiEngine.Util;

namespace SkiEngine.Xamarin
{
    public delegate void DrawDelegate(SKSurface surface, double widthXamarinUnits, double heightXamarinUnits);

    public class OffUiThreadRenderer : IDisposable
    {
        private readonly TaskQueue _taskQueue = new TaskQueue();
        private readonly DrawDelegate _drawAction;
        private readonly Action _invalidateSurfaceAction;

        private readonly object _pendingDrawLock = new object();
        private volatile bool _pendingDraw;
        private SKSurface _offUiThreadSurface;

        private readonly object _snapshotLock = new object();
        private volatile SnapshotImage _snapshotImage;
       
        private int _widthPixels;
        private int _heightPixels;
        private double _widthXamarinUnits;
        private double _heightXamarinUnits;

        public OffUiThreadRenderer(DrawDelegate drawAction, Action invalidateSurfaceAction)
        {
            _drawAction = drawAction;
            _invalidateSurfaceAction = invalidateSurfaceAction;
            
            _snapshotImage = new SnapshotImage(
                SKImage.Create(new SKImageInfo(1, 1)), 
                new SKSizeI(0, 0)
            );
            _snapshotImage.AddUser();
        }

        public SnapshotImage AddSnapshotImageUser()
        {
            lock (_snapshotLock)
            {
                _snapshotImage.AddUser();
                return _snapshotImage;
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
                e.Surface.Canvas.DrawImage(_snapshotImage.SkImage, 0, 0);
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

            // Draw scene
            _drawAction.Invoke(_offUiThreadSurface, _widthXamarinUnits, _heightXamarinUnits);

            // Create snapshot
            lock (_snapshotLock)
            {
                _snapshotImage.RemoveUser();
                _snapshotImage = new SnapshotImage(_offUiThreadSurface.Snapshot(), new SKSizeI(_widthPixels, _heightPixels));
                _snapshotImage.AddUser();
            }
            
            // Invalidate surface to redraw snapshot
            _invalidateSurfaceAction.Invoke();
        }

        public async void Dispose()
        {
            await _taskQueue.ShutdownAsync();

            _offUiThreadSurface?.Dispose();

            _snapshotImage.RemoveUser();
        }
    }
}
