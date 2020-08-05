using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SkiEngine.UI
{
    public static class LinkedCollection
    {
        public static void ForwardUpdatesTo<TFromItem, TToItem>(
            this ObservableCollection<TFromItem> collection, 
            IList<TToItem> other,
            Func<TFromItem, TToItem> convert
        )
        {
            other.Clear();
            foreach (var item in collection)
            {
                other.Add(convert(item));
            }
            
            collection.CollectionChanged += (sender, args) =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        other.Insert(args.NewStartingIndex, convert((TFromItem) args.NewItems[0]));
                        break;
                    case NotifyCollectionChangedAction.Move:
                        var otherItem = other[args.OldStartingIndex];
                        other.RemoveAt(args.OldStartingIndex);
                        other.Insert(args.NewStartingIndex, otherItem);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        other.RemoveAt(args.OldStartingIndex);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        other[args.NewStartingIndex] = convert((TFromItem) args.NewItems[0]);
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        other.Clear();
                        break;
                }
            };
        }
    }
}