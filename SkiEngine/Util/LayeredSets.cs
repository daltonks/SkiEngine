using System;
using System.Collections;
using System.Collections.Generic;

namespace SkiEngine.Util
{
    public class LayeredSets<TLayer, TItem> : IEnumerable<TItem>
    {
        private Dictionary<TLayer, HashSet<TItem>> _layers;
        private List<TLayer> _orderedLayers;

        private Func<TItem, TLayer> _getLayerFunc;
        private IComparer<TLayer> _layerComparer;
        private IEqualityComparer<TItem> _itemEqualityComparer;

        public LayeredSets(Func<TItem, TLayer> getLayerFunc)
        {
            Init(getLayerFunc, Comparer<TLayer>.Default, ReferenceEqualityComparer<TItem>.Default);
        }

        public LayeredSets(Func<TItem, TLayer> getLayerFunc, IComparer<TLayer> layerComparer, IEqualityComparer<TItem> itemEqualityComparer)
        {
            Init(getLayerFunc, layerComparer, itemEqualityComparer);
        }

        public IReadOnlyList<TLayer> OrderedLayers => _orderedLayers;

        public IEnumerable<TItem> ReversedItems
        {
            get
            {
                for (var i = _orderedLayers.Count - 1; i >= 0; i--)
                {
                    var layer = _orderedLayers[i];
                    foreach (var item in _layers[layer])
                    {
                        yield return item;
                    }
                }
            }
        }

        private void Init(Func<TItem, TLayer> getLayerFunc, IComparer<TLayer> layerComparer, IEqualityComparer<TItem> itemEqualityComparer)
        {
            _getLayerFunc = getLayerFunc;
            _layerComparer = layerComparer;
            _itemEqualityComparer = itemEqualityComparer;

            _layers = new Dictionary<TLayer, HashSet<TItem>>();
            _orderedLayers = new List<TLayer>();
        }

        public IReadOnlyCollection<TItem> GetItems(TLayer layer)
        {
            return _layers.TryGetValue(layer, out var items)
                ? items
                : (IReadOnlyCollection<TItem>) new List<TItem>(0);
        }

        public bool Add(TItem item)
        {
            var layer = _getLayerFunc.Invoke(item);
            if (_layers.TryGetValue(layer, out var layerSet))
            {
                return layerSet.Add(item);
            }

            // Initialize with an enumerable to ensure the HashSet starts with a capacity of 1
            _layers[layer] = new HashSet<TItem>(new[] { item }, _itemEqualityComparer);

            var layerSetOrderIndex = _orderedLayers.BinarySearch(layer, _layerComparer);
            if (layerSetOrderIndex < 0)
            {
                layerSetOrderIndex = ~layerSetOrderIndex;
            }
            _orderedLayers.Insert(layerSetOrderIndex, layer);

            return true;
        }

        public void Update(TItem item, TLayer previousLayer)
        {
            Remove(item, previousLayer);
            Add(item);
        }

        public bool Remove(TItem item)
        {
            return Remove(item, _getLayerFunc.Invoke(item));
        }

        public bool Remove(TItem item, TLayer layer)
        {
            var removedItem = false;

            if (_layers.TryGetValue(layer, out var layerSet))
            {
                removedItem = layerSet.Remove(item);

                if (layerSet.Count == 0)
                {
                    // Remove layer entirely
                    _layers.Remove(layer);

                    var layerSetOrderIndex = _orderedLayers.BinarySearch(layer, _layerComparer);
                    _orderedLayers.RemoveAt(layerSetOrderIndex);
                }
                else if (layerSet.Count == 1)
                {
                    // Memory optimization for many layers with usually 1 item in each layer.
                    layerSet.TrimExcess();
                }
            }

            return removedItem;
        }

        public void Clear()
        {
            _layers.Clear();
            _orderedLayers.Clear();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            foreach (var layer in _orderedLayers)
            foreach (var item in _layers[layer])
            {
                yield return item;
            }
        }
    }
}
