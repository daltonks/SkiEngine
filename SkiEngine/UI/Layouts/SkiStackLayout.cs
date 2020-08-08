using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SkiaSharp;
using SkiEngine.UI.Views;
using SkiEngine.UI.Views.Base;

namespace SkiEngine.UI.Layouts
{
    public class SkiStackLayout : SkiView
    {
        private SKSize _maxSize;

        public SkiStackLayout()
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

            UiComponent?.RunNextUpdate(LayoutChildren);
            InvalidateSurface();

            void OnChildAdded(SkiView child)
            {
                UpdateChildNode(child);
                child.SizeRequestProp.ValueChanged += OnChildSizeRequestChanged;
                child.HorizontalOptionsProp.ValueChanged += OnChildHorizontalOptionsChanged;
            }

            void OnChildRemoved(SkiView child)
            {
                child.Node?.Destroy();
                child.SizeRequestProp.ValueChanged -= OnChildSizeRequestChanged;
                child.HorizontalOptionsProp.ValueChanged -= OnChildHorizontalOptionsChanged;
            }
        }

        private void OnChildSizeRequestChanged(object sender, SKSize oldValue, SKSize newValue)
        {
            UiComponent?.RunNextUpdate(LayoutChildren);
            InvalidateSurface();
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

        private void LayoutChildren()
        {
            var size = new SKSize();

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (_maxSize.Height == float.MaxValue)
            {
                foreach (var child in Children)
                {
                    child.Layout(_maxSize.Width, float.MaxValue);
                    child.Node.RelativePoint = new SKPoint(0, size.Height);
                    size.Height += child.Size.Height;
                    size.Width = Math.Max(size.Width, child.Size.Width);
                }
            }
            else
            {
                var totalHeightRequests = 0f;
                var numNoHeightRequest = 0;
                foreach (var child in Children)
                {
                    var childHeightRequest = child.SizeRequest.Height;
                    if (childHeightRequest == -1)
                    {
                        numNoHeightRequest++;
                    }
                    else
                    {
                        totalHeightRequests += childHeightRequest;
                    }
                }

                var childrenMaxSizes = new SKSize[Children.Count];

                for (var i = 0; i < Children.Count; i++)
                {
                    var child = Children[i];
                    var childMaxSize = childrenMaxSizes[i];
                    child.Layout(childMaxSize.Width, childMaxSize.Height);
                    size.Height += childMaxSize.Height;
                    size.Width = Math.Max(size.Width, childMaxSize.Width);
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
