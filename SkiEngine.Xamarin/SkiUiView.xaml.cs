﻿using System;
using SkiaSharp.Views.Forms;
using SkiEngine.UI;
using SkiEngine.UI.Views.Base;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SkiEngine.Xamarin
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SkiUiView : ScrollView
    {
        public static readonly BindableProperty SkiViewProperty = BindableProperty.Create(
            nameof(SkiView),
            typeof(SkiView),
            typeof(SkiUiView),
            propertyChanged: (bindable, oldValue, newValue) => ((SkiUiView) bindable).OnSkiViewChanged()
        );
        
        private readonly SkiUiScene _skiUiScene;
        private SkiXamarinUiComponent _uiComponent;
        
        public SkiUiView()
        {
            InitializeComponent();

            // UWP immediately redraws when calling InvalidateSurface,
            // which breaks logical flow if you make multiple changes
            // that invalidate the surface.
            // Because of this, use the dispatcher.
            var invalidateSurface = Device.RuntimePlatform == Device.UWP
                ? () => Application.Current.Dispatcher.BeginInvokeOnMainThread(SkiaView.InvalidateSurface)
                : (Action) (() => MainThread.InvokeOnMainThreadAsync(SkiaView.InvalidateSurface));

            _skiUiScene = new SkiUiScene(
                invalidateSurface, 
                (node, camera, invalidate) => _uiComponent = new SkiXamarinUiComponent(
                    SkiaView, NativeEntry, node, camera, invalidate
                )
            );

            // https://github.com/mono/SkiaSharp/issues/1377
            if (Device.RuntimePlatform == Device.UWP)
            {
                SkiaView.SizeChanged += (sender, args) =>
                {
                    _skiUiScene.InvalidateSurface();
                };
            }
        }

        public SkiView SkiView
        {
            get => (SkiView) GetValue(SkiViewProperty);
            set => SetValue(SkiViewProperty, value);
        }

        private void OnSkiViewChanged()
        {
            _skiUiScene.UiComponent.View = SkiView;
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs skPaintSurfaceEventArgs)
        {
            _skiUiScene.OnPaintSurface(skPaintSurfaceEventArgs.Surface.Canvas, Width, Height);
        }

        private void OnTouch(object sender, SKTouchEventArgs e)
        {
            e.Handled = true;

            _skiUiScene.OnTouch(e.ToSkiTouch());
        }
    }
}