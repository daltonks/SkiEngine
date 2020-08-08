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
                child.HorizontalOptionsProp.ValueChanged += OnChildHorizontalOptionsChanged;
            }

            void OnChildRemoved(SkiView child)
            {
                child.Node?.Destroy();
                child.HorizontalOptionsProp.ValueChanged -= OnChildHorizontalOptionsChanged;
            }
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
            foreach (var child in Children)
            {
                child.Layout(_maxSize.Width, float.MaxValue);

                var childSize = child.Size;
                size.Height += childSize.Height;
                size.Width = Math.Max(size.Width, childSize.Width);
            }

            Size = size;

            var childY = 0f;
            foreach (var child in Children)
            {
                child.Node.RelativePoint = new SKPoint(GetChildX(child), childY);
                childY += child.Size.Height;
            }
        }

        private float GetChildX(SkiView child)
        {
            return child.HorizontalOptions switch
            {
                SkiLayoutOptions.Start => 0,
                SkiLayoutOptions.Center => _maxSize.Width / 2 - child.Size.Width / 2,
                SkiLayoutOptions.End => _maxSize.Width - child.Size.Width,
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
