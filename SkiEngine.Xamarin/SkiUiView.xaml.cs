﻿using System;
using SkiaSharp.Views.Forms;
using SkiEngine.UI;
using SkiEngine.UI.Views.Base;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SkiEngine.Xamarin
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SkiUiView : AbsoluteLayout
    {
        private readonly SkiUiScene _skiUiScene;
        
        public SkiUiView()
        {
            InitializeComponent();

            // UWP immediately redraws when calling InvalidateSurface,
            // which breaks logical flow if you make multiple changes
            // that invalidate the surface.
            // Because of this, use the dispatcher.
            var invalidateSurface = Device.RuntimePlatform == Device.UWP
                ? () => Application.Current.Dispatcher.BeginInvokeOnMainThread(CanvasView.InvalidateSurface)
                : (Action) CanvasView.InvalidateSurface;

            _skiUiScene = new SkiUiScene(
                invalidateSurface, 
                (node, camera, invalidate) => UiComponent = new SkiXamarinUiComponent(
                    CanvasView, NativeEntry, NativeEntryLayout, node, camera, invalidate
                )
            );
        }

        private SkiXamarinUiComponent _uiComponent;
        public SkiXamarinUiComponent UiComponent
        {
            get => _uiComponent;
            private set
            {
                _uiComponent = value;
                OnPropertyChanged();
            }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            _skiUiScene.UiComponent.View = (SkiView) BindingContext;
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            _skiUiScene.OnPaintSurface(e.Surface.Canvas, Width, Height);
        }

        private void OnTouch(object sender, SKTouchEventArgs e)
        {
            e.Handled = true;

            _skiUiScene.OnTouch(e.ToSkiTouch());
        }

        private void OnCloseEntryTapped(object sender, EventArgs e)
        {
            UiComponent.HideNativeEntry();
        }
    }
}