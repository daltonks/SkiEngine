using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using SkiEngine.NCS;
using SkiEngine.NCS.Component;
using SkiEngine.Sprite;
using Xamarin.Forms;

namespace SkiEngine.TestApp
{
    public partial class MainPage : ContentPage
    {
        private static readonly SKImage TilesetImage;
        
        static MainPage()
        {
            // Get tilesetImage
            var assembly = typeof(MainPage).GetTypeInfo().Assembly;
            SKBitmap bitmap;
            using (var stream = assembly.GetManifestResourceStream("SkiEngine.TestApp.PrototypeMap.png"))
            {
                bitmap = SKBitmap.Decode(stream);
            }

            TilesetImage = SKImage.FromBitmap(bitmap);
        }

        private readonly Scene _scene = new Scene();
        private readonly Dictionary<long, SKPath> _temporaryPaths = new Dictionary<long, SKPath>();
        private readonly List<SKPath> _paths = new List<SKPath>();
        private readonly CameraComponent _camera1;
        private readonly CameraComponent _camera2;
        private readonly SemaphoreSlim _updateSemaphore = new SemaphoreSlim(1);

        public MainPage()
        {
            InitializeComponent();

            var camera1Node = _scene.RootNode.CreateChild();
            _camera1 = new CameraComponent(0, 0);
            camera1Node.AddComponent(_camera1);

            var camera2Node = _scene.RootNode.CreateChild(new SKPoint(0, 0), 0, new SKPoint(4f, 4f));
            _camera2 = new CameraComponent(0, 1);
            camera2Node.AddComponent(_camera2);

            var scribbleNode = _scene.RootNode.CreateChild();
            var scribbleDrawingComponent = new DrawableComponent(
                (canvas, transform) =>
                {
                    var touchPathStroke = new SKPaint
                    {
                        IsAntialias = true,
                        Style = SKPaintStyle.Stroke,
                        Color = SKColors.Purple,
                        StrokeWidth = 1
                    };

                    foreach (var touchPath in _temporaryPaths)
                    {
                        canvas.DrawPath(touchPath.Value, touchPathStroke);
                    }
                    foreach (var touchPath in _paths)
                    {
                        canvas.DrawPath(touchPath, touchPathStroke);
                    }
                }
            );
            scribbleNode.AddComponent(scribbleDrawingComponent);

            _camera1.AddDrawable(scribbleDrawingComponent, 0);
            _camera2.AddDrawable(scribbleDrawingComponent, 0);

            for (var x = -500; x <= 500; x += 25)
            for (var y = -500; y <= 500; y += 25)
            {
                var scale = x == 0 && y == 0 ? new SKPoint(3, 3) : new SKPoint(1, 1);
                var node = _scene.RootNode.CreateChild(new SKPoint(x, y), 0, scale);
                var sprite = new SpriteComponent(TilesetImage, new SpriteData(SKRectI.Create(0, 0, 4, 4)));
                node.AddComponent(sprite);
                _camera1.AddDrawable(sprite, 1);
                _camera2.AddDrawable(sprite, 1);
            }
        }

        private void OnPaintSurface1(object sender, SKPaintGLSurfaceEventArgs args)
        {
            _updateSemaphore.Wait();
            _scene.Update();
            _updateSemaphore.Release();

            var canvas = args.Surface.Canvas;
            canvas.Clear(SKColors.Black);

            _scene.Draw(canvas, 0);

            canvas.Flush();
        }

        private void OnPaintSurface2(object sender, SKPaintGLSurfaceEventArgs args)
        {
            var canvas = args.Surface.Canvas;
            canvas.Clear(SKColors.Black);

            _updateSemaphore.Wait();
            _scene.Draw(canvas, 1);
            _updateSemaphore.Release();

            canvas.Flush();
        }

        private void OnTouch1(object sender, SKTouchEventArgs args)
        {
            OnTouch(args, _camera1);
        }

        private void OnTouch2(object sender, SKTouchEventArgs args)
        {
            OnTouch(args, _camera2);
        }
        
        private void OnTouch(SKTouchEventArgs args, CameraComponent camera)
        {
            var argsActionType = args.ActionType;
            var argsLocation = args.Location;
            var argsId = args.Id;
            var argsInContact = args.InContact;

            _scene.RunNextUpdate(
                () =>
                {
                    var worldPoint = camera.PixelToWorldMatrix.MapPoint(argsLocation);
                    Debug.WriteLine(worldPoint);

                    switch (argsActionType)
                    {
                        case SKTouchAction.Pressed:
                        {
                            // start of a stroke
                            var p = new SKPath();
                            p.MoveTo(worldPoint);
                        
                            _temporaryPaths[argsId] = p;
                    
                            break;
                        }
                        case SKTouchAction.Moved:
                        {
                            // the stroke, while pressed
                            if (argsInContact && _temporaryPaths.TryGetValue(argsId, out var foundPath))
                            {
                                foundPath.LineTo(worldPoint);
                            }
                            break;
                        }
                        case SKTouchAction.Released:
                        {
                            // end of a stroke
                            _paths.Add(_temporaryPaths[argsId]);
                            _temporaryPaths.Remove(argsId);
                            break;
                        }
                        case SKTouchAction.Cancelled:
                        {
                            // we don't want that stroke
                            _temporaryPaths.Remove(argsId);
                            break;
                        }
                    }
                }
            );
            
            // we have handled these events
            args.Handled = true;
        }
    }
}
