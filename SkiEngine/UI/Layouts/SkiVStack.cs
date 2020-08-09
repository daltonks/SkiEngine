using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using SkiaSharp;
using SkiEngine.UI.Views;
using SkiEngine.UI.Views.Base;

namespace SkiEngine.UI.Layouts
{
    public class SkiVStack : SkiView
    {
        private SKSize _maxSize;
        private bool _layoutChildrenQueued;

        public SkiVStack()
        {
            Children.CollectionChanged += OnChildrenChanged;
        }

        public ObservableCollection<SkiView> Children { get; } = new ObservableCollection<SkiView>();
        public override IEnumerable<SkiView> ChildrenEnumerable => Children;

        protected override void OnNodeChanged()
        {
            foreach (var child in Children)
            {
                UpdateChildNode(child);
            }
        }

        private void OnChildrenChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    OnChildAdded((SkiView)args.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OnChildRemoved((SkiView)args.OldItems[0]);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    OnChildRemoved((SkiView)args.OldItems[0]);
                    OnChildAdded((SkiView)args.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (args.OldItems != null)
                    {
                        foreach (var oldItem in args.OldItems)
                        {
                            OnChildRemoved((SkiView)oldItem);
                        }
                    }
                    break;
            }

            QueueLayoutChildren();

            void OnChildAdded(SkiView child)
            {
                UpdateChildNode(child);
                child.WidthRequestProp.ValueChanged += OnChildSizeRequestChanged;
                child.HeightRequestProp.ValueChanged += OnChildSizeRequestChanged;
                child.HorizontalOptionsProp.ValueChanged += OnChildHorizontalOptionsChanged;
            }

            void OnChildRemoved(SkiView child)
            {
                child.Node?.Destroy();
                child.WidthRequestProp.ValueChanged -= OnChildSizeRequestChanged;
                child.HeightRequestProp.ValueChanged -= OnChildSizeRequestChanged;
                child.HorizontalOptionsProp.ValueChanged -= OnChildHorizontalOptionsChanged;
            }
        }

        private void OnChildSizeRequestChanged(object sender, float? oldValue, float? newValue)
        {
            QueueLayoutChildren();
        }

        private void OnChildHorizontalOptionsChanged(object sender, SkiLayoutOptions oldValue, SkiLayoutOptions newValue)
        {
            var child = (SkiView) sender;
            child.Node.RelativePoint = new SKPoint(GetChildX(child), child.Node.RelativePoint.Y);
        }

        public override void Layout(float maxWidth, float maxHeight)
        {
            _maxSize = new SKSize(maxWidth, maxHeight);
            LayoutChildren();
        }

        private void QueueLayoutChildren()
        {
            if (_layoutChildrenQueued || UiComponent == null)
            {
                return;
            }

            _layoutChildrenQueued = true;
            UiComponent.RunNextUpdate(() => {
                LayoutChildren();
                _layoutChildrenQueued = false;
            });

            InvalidateSurface();
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        private void LayoutChildren()
        {
            var size = new SKSize();

            if (_maxSize.Height == float.MaxValue)
            {
                // There is no height limit

                foreach (var child in Children)
                {
                    var width = child.WidthRequest == null
                        ? _maxSize.Width
                        : Math.Min(child.WidthRequest.Value, _maxSize.Width);
                    var height = child.HeightRequest ?? _maxSize.Height;

                    child.Layout(width, height);
                    child.Node.RelativePoint = new SKPoint(0, size.Height);
                    size.Height += child.Size.Height;
                    size.Width = Math.Max(size.Width, child.Size.Width);
                }
            }
            else
            {
                // There is a height limit

                var totalHeightRequests = 0f;
                var numNoHeightRequest = 0;
                foreach (var child in Children)
                {
                    if (child.HeightRequest == null)
                    {
                        numNoHeightRequest++;
                    }
                    else
                    {
                        totalHeightRequests += child.HeightRequest.Value;
                    }
                }

                float heightOfNoHeightRequestChildren;
                float scaleOfHeightRequestChildren;

                if (totalHeightRequests < _maxSize.Height)
                {
                    // All height requests can be honored
                    heightOfNoHeightRequestChildren = (_maxSize.Height - totalHeightRequests) / numNoHeightRequest;
                    scaleOfHeightRequestChildren = 1;
                }
                else
                {
                    // Height requests are out-of-bounds, so they need to be shrunk
                    heightOfNoHeightRequestChildren = 0;
                    scaleOfHeightRequestChildren = _maxSize.Height / totalHeightRequests;
                }

                foreach (var child in Children)
                {
                    var width = child.WidthRequest == null
                        ? _maxSize.Width
                        : Math.Min(child.WidthRequest.Value, _maxSize.Width);
                    var height = child.HeightRequest * scaleOfHeightRequestChildren ?? heightOfNoHeightRequestChildren;

                    child.Layout(width, height);
                    child.Node.RelativePoint = new SKPoint(0, size.Height);
                    size.Height += height;
                    size.Width = Math.Max(size.Width, child.Size.Width);
                }
            }

            Size = size;

            // Update children X values
            foreach (var child in Children)
            {
                child.Node.RelativePoint = new SKPoint(GetChildX(child), child.Node.RelativePoint.Y);
            }
        }

        private float GetChildX(SkiView child)
        {
            return child.HorizontalOptions switch
            {
                SkiLayoutOptions.Start => 0,
                SkiLayoutOptions.Center => Size.Width / 2 - child.Size.Width / 2,
                SkiLayoutOptions.End => Size.Width - child.Size.Width,
                SkiLayoutOptions.Fill => 0,
                _ => 0f
            };
        }

        protected override void DrawInternal(SKCanvas canvas)
        {
            foreach (var view in Children)
            {
                view.Draw(canvas);
            }
        }
    }
}
