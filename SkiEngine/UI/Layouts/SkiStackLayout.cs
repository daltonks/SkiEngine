using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SkiaSharp;

namespace SkiEngine.UI.Layouts
{
    public class SkiStackLayout : SkiView
    {
        public SkiStackLayout()
        {
            Children.CollectionChanged += OnChildrenChanged;
        }

        public ObservableCollection<SkiView> Children { get; } = new ObservableCollection<SkiView>();
        public override IEnumerable<SkiView> ChildrenEnumerable => Children;

        public override bool ListensForPressedTouches => false;

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
                    if (args.NewStartingIndex == Children.Count - 1)
                    {
                        // Happy, easy path. Don't need full layout.
                        var childView = (SkiView)args.NewItems[0];
                        UpdateChildNode(childView, new InitialNodeTransform(new SKPoint(0, Size.Value.Height)));
                        childView.Layout(Size.Value.Width, float.MaxValue);
                        Size.Value = new SKSize(Size.Value.Width, Size.Value.Height + childView.Size.Value.Height);
                        InvalidateSurface();
                        return;
                    }
                    else
                    {
                        UpdateChildNode((SkiView)args.NewItems[0]);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    ((SkiView)args.OldItems[0]).Node?.Destroy();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    ((SkiView)args.OldItems[0]).Node?.Destroy();
                    UpdateChildNode((SkiView)args.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (args.OldItems != null)
                    {
                        foreach (var oldItem in args.OldItems)
                        {
                            ((SkiView) oldItem).Node?.Destroy();
                        }
                    }
                    
                    break;
            }

            UiComponent?.RequestFullLayout();
            InvalidateSurface();
        }

        public override void Layout(float maxWidth, float maxHeight)
        {
            var height = 0f;
            foreach (var child in ChildrenEnumerable)
            {
                child.Node.RelativePoint = new SKPoint(0, height);
                child.Layout(maxWidth, float.MaxValue);
                height += child.Size.Value.Height;
            }
            Size.Value = new SKSize(maxWidth, height);
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
