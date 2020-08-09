using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SkiaSharp;
using SkiEngine.UI.Views;
using SkiEngine.UI.Views.Base;

namespace SkiEngine.UI.Layouts.Base
{
    public abstract class SkiMultiChildLayout : SkiView
    {
        private bool _layoutChildrenQueued;

        public SkiMultiChildLayout()
        {
            Children.CollectionChanged += OnChildrenChanged;
        }

        protected SKSize MaxSize { get; private set; }

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
                child.VerticalOptionsProp.ValueChanged += OnChildVerticalOptionsChanged;
            }

            void OnChildRemoved(SkiView child)
            {
                child.Node?.Destroy();
                child.WidthRequestProp.ValueChanged -= OnChildSizeRequestChanged;
                child.HeightRequestProp.ValueChanged -= OnChildSizeRequestChanged;
                child.VerticalOptionsProp.ValueChanged -= OnChildVerticalOptionsChanged;
            }
        }

        private void OnChildSizeRequestChanged(object sender, float? oldValue, float? newValue)
        {
            QueueLayoutChildren();
        }

        protected abstract void OnChildHorizontalOptionsChanged(object sender, SkiLayoutOptions oldValue, SkiLayoutOptions newValue);
        protected abstract void OnChildVerticalOptionsChanged(object sender, SkiLayoutOptions oldValue, SkiLayoutOptions newValue);

        protected void QueueLayoutChildren()
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

        public override void Layout(float maxWidth, float maxHeight)
        {
            MaxSize = new SKSize(maxWidth, maxHeight);
            LayoutChildren();
        }

        protected abstract void LayoutChildren();

        protected override void DrawInternal(SKCanvas canvas)
        {
            foreach (var view in Children)
            {
                view.Draw(canvas);
            }
        }
    }
}
