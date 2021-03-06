﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using SkiEngine.NCS;
using SkiEngine.NCS.Component;
using SkiEngine.NCS.Component.Camera;
using SkiEngine.NCS.Component.Sprite;
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

        private readonly CanvasComponent _canvasComponent1;
        private readonly CanvasComponent _canvasComponent2;
        private readonly CameraGroup _cameraGroup1;
        private readonly CameraGroup _cameraGroup2;
        private readonly CameraComponent _camera1;
        private readonly CameraComponent _camera2;

        public MainPage()
        {
            InitializeComponent();

            _scene.Start();

            // Canvas components
            _canvasComponent1 = _scene.RootNode.AddComponent(new CanvasComponent());
            _canvasComponent2 = _scene.RootNode.AddComponent(new CanvasComponent());

            // Camera groups
            _cameraGroup1 = _canvasComponent1.CreateCameraGroup();
            _cameraGroup2 = _canvasComponent2.CreateCameraGroup();

            // Cameras
            _camera1 = _scene.RootNode
                .CreateChild()
                .AddComponent(new CameraComponent(_cameraGroup1, 0));

            _camera2 = _scene.RootNode
                .CreateChild(new SKPoint(50, 200), 0, (float) Math.PI / 8, new SKPoint(3, 3))
                .AddComponent(new CameraComponent(_cameraGroup2, 0));

            // Scribble
            _scene.RootNode
                .CreateChild()
                .AddComponent(
                    new DrawableComponent(
                        (canvas, camera) =>
                        {
                            using (
                                var touchPathStroke = new SKPaint
                                {
                                    IsAntialias = true,
                                    Style = SKPaintStyle.Stroke,
                                    Color = SKColors.Purple,
                                    StrokeWidth = 1
                                }
                            )
                            {
                                foreach (var touchPath in _temporaryPaths)
                                {
                                    canvas.DrawPath(touchPath.Value, touchPathStroke);
                                }
                                foreach (var touchPath in _paths)
                                {
                                    canvas.DrawPath(touchPath, touchPathStroke);
                                }
                            }
                        }
                    )
                )
                .AddToCamera(_camera1)
                .AddToCamera(_camera2);

            //Sprites
            var zeroSprite = CreateTileSprite(_scene.RootNode, new InitialNodeTransform());
            var secondSprite = CreateTileSprite(
                zeroSprite.Node, 
                new InitialNodeTransform(new SKPoint(50, 0), 0, (float) Math.PI / 2, new SKPoint(2, 2))
            );
            var thirdSprite = CreateTileSprite(
                secondSprite.Node, 
                new InitialNodeTransform(new SKPoint(50, 0), 0, (float) Math.PI / 2, new SKPoint(2, 2))
            );

            var fourthSprite = CreateTileSprite(
                thirdSprite.Node, 
                new InitialNodeTransform(new SKPoint(50, 0), 0, (float) Math.PI / 2, new SKPoint(2, 2))
            );
        }

        private SpriteComponent CreateTileSprite(Node parent, InitialNodeTransform transform)
        {
            var sprite = parent
                .CreateChild(transform)
                .AddComponent(
                    new SpriteComponent(TilesetImage, new SpriteData(SKRectI.Create(0, 0, 8, 8)))
                )
                .AddToCamera(_camera1)
                .AddToCamera(_camera2);

            return sprite;
        }

        private void OnPaintSurface1(object sender, SKPaintGLSurfaceEventArgs args)
        {
            _scene.Update();

            var canvas = args.Surface.Canvas;
            
            canvas.Clear(SKColors.Black);
            _scene.Draw(() => {
                _canvasComponent1.StartDraw(canvas, SkGlView1.Width, SkGlView1.Height);
                _cameraGroup1.Draw(canvas);
            });
            canvas.Flush();
        }

        private void OnPaintSurface2(object sender, SKPaintGLSurfaceEventArgs args)
        {
            var canvas = args.Surface.Canvas;

            canvas.Clear(SKColors.Black);
            _scene.Draw(() => {
                _canvasComponent2.StartDraw(canvas, SkGlView2.Width, SkGlView2.Height);
                _cameraGroup2.Draw(canvas);
            });
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

            _scene.RunDuringUpdate(
                () =>
                {
                    var worldPoint = camera.PixelToWorldMatrix.MapPoint(argsLocation);

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
                            if (_temporaryPaths.TryGetValue(argsId, out var foundPath))
                            {
                                _paths.Add(foundPath);
                                _temporaryPaths.Remove(argsId);

                                _camera1.ZoomTo(_paths.SelectMany(path => path.Points));
                                _camera2.ZoomTo(_paths.SelectMany(path => path.Points));
                            }
                            
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
