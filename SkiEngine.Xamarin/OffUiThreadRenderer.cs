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

        private readonly object _snapshotImageLock = new object();
        private readonly object _pendingDrawLock = new object();
        private volatile bool _pendingDraw;
        private SKSurface _offUiThreadSurface;
        private bool _allowSnapshotDispose = true;
        
        private SKImage _snapshotImage;

        private int _widthPixels;
        private int _heightPixels;
        private double _widthXamarinUnits;
        private double _heightXamarinUnits;

        public OffUiThreadRenderer(DrawDelegate drawAction, Action invalidateSurfaceAction)
        {
            _drawAction = drawAction;
            _invalidateSurfaceAction = invalidateSurfaceAction;
        }

        public (SKImage Image, SKSizeI Size) GetSnapshotImageAndPreventDispose()
        {
            lock (_snapshotImageLock)
            {
                _allowSnapshotDispose = false;
                return (_snapshotImage, new SKSizeI(_widthPixels, _heightPixels));
            }
        }

        public SKColor? GetPixelColor(int pixelX, int pixelY)
        {
            lock (_snapshotImageLock)
            {
                if (pixelX < 0 || pixelX >= _widthPixels || pixelY < 0 || pixelY >= _heightPixels)
                {
                    return null;
                }

                try
                {
                    return _snapshotImage?.PeekPixels().GetPixelColor(pixelX, pixelY);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);

                    return null;
                }
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
                _taskQueue.QueueAsync(ExpensiveDraw);
            }
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public void OnPaintSurface(SKPaintSurfaceEventArgs e, double widthXamarinUnits, double heightXamarinUnits)
        {
            lock (_snapshotImageLock)
            {
                if (_snapshotImage != null)
                {
                    e.Surface.Canvas.DrawImage(_snapshotImage, 0, 0);
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
                ExpensiveDraw();
            });
        }

        private void ExpensiveDraw()
        {
            if (_offUiThreadSurface == null)
            {
                return;
            }

            _pendingDraw = false;

            // Draw scene
            _drawAction.Invoke(_offUiThreadSurface, _widthXamarinUnits, _heightXamarinUnits);

            // Create snapshot
            lock (_snapshotImageLock)
            {
                if (_allowSnapshotDispose)
                {
                    _snapshotImage?.Dispose();
                }
                else
                {
                    _allowSnapshotDispose = true;
                }
                
                _snapshotImage = _offUiThreadSurface.Snapshot();
            }

            // Invalidate surface to redraw snapshot
            _invalidateSurfaceAction.Invoke();
        }

        public async void Dispose()
        {
            await _taskQueue.ShutdownAsync();

            _offUiThreadSurface?.Dispose();

            lock (_snapshotImageLock)
            {
                if (_allowSnapshotDispose)
                {
                    _snapshotImage?.Dispose();
                }
            }
        }
    }
}
