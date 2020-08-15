using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SkiaSharp;
using SkiEngine.UI.Views.Base;

namespace SkiEngine.UI.Layouts.Base
{
    public abstract class SkiMultiChildLayout : SkiLayout
    {
        protected SkiMultiChildLayout()
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

            InvalidateLayout();

            void OnChildAdded(SkiView child)
            {
                UpdateChildNode(child);
                child.WidthRequestProp.ValueChanged += OnChildSizeRequestChanged;
                child.HeightRequestProp.ValueChanged += OnChildSizeRequestChanged;
                child.HorizontalOptionsProp.ValueChanged += OnChildLayoutOptionsChanged;
                child.VerticalOptionsProp.ValueChanged += OnChildLayoutOptionsChanged;
            }

            void OnChildRemoved(SkiView child)
            {
                child.Node?.Destroy();
                child.WidthRequestProp.ValueChanged -= OnChildSizeRequestChanged;
                child.HeightRequestProp.ValueChanged -= OnChildSizeRequestChanged;
                child.HorizontalOptionsProp.ValueChanged -= OnChildLayoutOptionsChanged;
                child.VerticalOptionsProp.ValueChanged -= OnChildLayoutOptionsChanged;
            }
        }

        private void OnChildSizeRequestChanged(object sender, float? oldValue, float? newValue)
        {
            InvalidateLayout();
        }

        private void OnChildLayoutOptionsChanged(object sender, SkiLayoutOptions oldValue, SkiLayoutOptions newValue)
        {
            InvalidateLayout();
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
