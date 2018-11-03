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

        private void Init(Func<TItem, TLayer> getLayerFunc, IComparer<TLayer> layerComparer, IEqualityComparer<TItem> itemEqualityComparer)
        {
            _getLayerFunc = getLayerFunc;
            _layerComparer = layerComparer;
            _itemEqualityComparer = itemEqualityComparer;

            _layers = new Dictionary<TLayer, HashSet<TItem>>();
            _orderedLayers = new List<TLayer>();
        }

        public bool Add(TItem item)
        {
            var layer = _getLayerFunc.Invoke(item);
            if (!_layers.TryGetValue(layer, out var layerSet))
            {
                layerSet = _layers[layer] = new HashSet<TItem>(_itemEqualityComparer);

                var layerSetOrderIndex = _orderedLayers.BinarySearch(layer, _layerComparer);
                if (layerSetOrderIndex < 0)
                {
                    layerSetOrderIndex = ~layerSetOrderIndex;
                }
                _orderedLayers.Insert(layerSetOrderIndex, layer);
            }

            return layerSet.Add(item);
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

        public bool Remove(TItem item, TLayer previousLayer)
        {
            var removedItem = false;

            if (_layers.TryGetValue(previousLayer, out var layerSet))
            {
                removedItem = layerSet.Remove(item);

                if (layerSet.Count == 0)
                {
                    _layers.Remove(previousLayer);

                    var layerSetOrderIndex = _orderedLayers.BinarySearch(previousLayer, _layerComparer);
                    _orderedLayers.RemoveAt(layerSetOrderIndex);
                }
            }

            return removedItem;
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
