using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SkiaSharp;
using SkiEngine.UI.Gestures;

namespace SkiEngine.UI.Views.Base
{
    public abstract class SkiView
    {
        private bool _allowViewPreferredWidth = true;
        private bool _allowViewPreferredHeight = true;
        private bool _updatingViewPreferredSize;

        public SkiView()
        {
            WidthRequestProp = new LinkedProperty<float?>(
                this, 
                valueChanged: (sender, oldValue, newValue) =>
                {
                    if (!_updatingViewPreferredSize)
                    {
                        _allowViewPreferredWidth = false;
                    }
                }
            );
            HeightRequestProp = new LinkedProperty<float?>(
                this, 
                valueChanged: (sender, oldValue, newValue) =>
                {
                    if (!_updatingViewPreferredSize)
                    {
                        _allowViewPreferredHeight = false;
                    }
                }
            );
            SizeProp = new LinkedProperty<SKSize>(this);
            HorizontalOptionsProp = new LinkedProperty<SkiLayoutOptions>(this);
            VerticalOptionsProp = new LinkedProperty<SkiLayoutOptions>(this);
            IsFocusedProp = new LinkedProperty<bool>(
                this, 
                valueChanged: (sender, oldValue, newValue) =>
                {
                    var previousFocusedView = UiComponent.FocusedView;
                    if (newValue)
                    {
                        if (previousFocusedView != null)
                        {
                            previousFocusedView.IsFocused = false;
                        }
                        UiComponent.FocusedView = this;
                    }
                    else if (previousFocusedView == this)
                    {
                        UiComponent.FocusedView = null;
                    }
                }
            );
        }

        public SkiUiComponent UiComponent { get; internal set; }

        private Node _node;
        public Node Node
        {
            get => _node;
            internal set
            {
                if (_node == value)
                {
                    return;
                }

                _node?.Destroy();
                _node = value;
                OnNodeChanged();
            }
        }

        public LinkedProperty<float?> WidthRequestProp { get; }
        public float? WidthRequest
        {
            get => WidthRequestProp.Value;
            set => WidthRequestProp.Value = value;
        }

        public LinkedProperty<float?> HeightRequestProp { get; }
        public float? HeightRequest
        {
            get => HeightRequestProp.Value;
            set => HeightRequestProp.Value = value;
        }

        protected float? ViewPreferredWidth
        {
            set
            {
                if (!_allowViewPreferredWidth)
                {
                    return;
                }

                _updatingViewPreferredSize = true;
                WidthRequest = value;
                _updatingViewPreferredSize = false;
            }
        }

        protected float? ViewPreferredHeight
        {
            set
            {
                if (!_allowViewPreferredHeight)
                {
                    return;
                }

                _updatingViewPreferredSize = true;
                HeightRequest = value;
                _updatingViewPreferredSize = false;
            }
        }

        public LinkedProperty<SKSize> SizeProp { get; }
        public SKSize Size
        {
            get => SizeProp.Value;
            protected set => SizeProp.Value = value;
        }

        public SKRect BoundsLocal => new SKRect(0, 0, Size.Width, Size.Height);
        public SKRect BoundsWorld => Node.LocalToWorldMatrix.MapRect(BoundsLocal);

        public LinkedProperty<SkiLayoutOptions> HorizontalOptionsProp { get; }
        public SkiLayoutOptions HorizontalOptions
        {
            get => HorizontalOptionsProp.Value;
            set => HorizontalOptionsProp.Value = value;
        }

        public LinkedProperty<SkiLayoutOptions> VerticalOptionsProp { get; }
        public SkiLayoutOptions VerticalOptions
        {
            get => VerticalOptionsProp.Value;
            set => VerticalOptionsProp.Value = value;
        }

        public LinkedProperty<bool> IsFocusedProp { get; }
        public bool IsFocused
        {
            get => IsFocusedProp.Value;
            set => IsFocusedProp.Value = value;
        }

        public abstract IEnumerable<SkiView> ChildrenEnumerable { get; }

        public List<SkiGestureRecognizer> GestureRecognizers { get; } = new List<SkiGestureRecognizer>();

        public SKMatrix PixelToLocalMatrix => UiComponent.Camera.PixelToWorldMatrix.PostConcat(Node.WorldToLocalMatrix);

        public void UpdateChildNode(SkiView child, InitialNodeTransform transform = null)
        {
            if (Node != null)
            {
                child.UiComponent = UiComponent;
                child.Node = Node.CreateChild(transform ?? new InitialNodeTransform());
            }
        }

        protected abstract void OnNodeChanged();
        public abstract void Layout(float maxWidth, float maxHeight);
        protected abstract void DrawInternal(SKCanvas canvas);

        public void Draw(SKCanvas canvas)
        {
            var drawMatrix = Node.LocalToWorldMatrix.PostConcat(UiComponent.Camera.WorldToPixelMatrix);
            canvas.SetMatrix(drawMatrix);

            if (canvas.QuickReject(BoundsLocal))
            {
                return;
            }

            DrawInternal(canvas);
        }

        public void InvalidateSurface()
        {
            UiComponent?.InvalidateSurface();
        }

        public bool HitTest(SKPoint pointWorld)
        {
            var localPoint = Node.WorldToLocalMatrix.MapPoint(pointWorld);
            return BoundsLocal.Contains(localPoint);
        }
    }

    public static class SkiViewExtensions
    {
        public static T Run<T>(this T view, Action<T> action) where T : SkiView
        {
            action(view);
            return view;
        }
    }
}
