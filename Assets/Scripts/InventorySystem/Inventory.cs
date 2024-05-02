using Assets.Scripts.InventorySystem.Items;
using Assets.Scripts.Utilities;
using FarrokhGames.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.InventorySystem
{
    public class Inventory : MonoBehaviour
    {
        public class ItemSlot
        {
            public ItemDefinition Item;
            public Vector2Int Position;
        }

        [SerializeField]
        private Vector2Int _size;

        private List<ItemDefinition> _items = new List<ItemDefinition>();
        private Rect _fullRect;

        public int Width => _size.x;
        public int Height => _size.y;
        public IReadOnlyList<ItemDefinition> Items => _items;
        public int ItemCount => _items.Count;

        public Action<ItemDefinition> onItemDropped { get; set; }
        public Action<ItemDefinition> onItemAdded { get; set; }
        public Action<ItemDefinition> onItemRemoved { get; set; }
        public Action onResized { get; set; }

        //private Grid<ItemSlot> _grid;

        private void Start()
        {
            //_grid = new Grid<ItemSlot>(_size.x, _size.y);

            Resize(Width, Height);
        }

        public void Resize(int newWidth, int newHeight)
        {
            _size.x = newWidth;
            _size.y = newHeight;

            RebuildRect();
        }

        private void HandleSizeChanged()
        {
            // Drop all items that no longer fit the inventory
            for (int i = 0; i < ItemCount;)
            {
                var item = _items[i];
                var shouldBeDropped = false;
                var padding = Vector2.one * 0.01f;

                if (!_fullRect.Contains(item.GetMinPoint() + padding) || !_fullRect.Contains(item.GetMaxPoint() - padding))
                {
                    shouldBeDropped = true;
                }

                if (shouldBeDropped)
                {
                    TryDrop(item);
                }
                else
                {
                    i++;
                }
            }
        }

        private void RebuildRect()
        {
            _fullRect = new Rect(0, 0, _size.x, _size.y);

            HandleSizeChanged();

            onResized?.Invoke();
        }

        public bool IsFull
        {
            get
            {
                for (var x = 0; x < Width; x++)
                {
                    for (var y = 0; y < Height; y++)
                    {
                        if (GetAtPoint(new Vector2Int(x, y)) == null)
                            return false;
                    }
                }

                return true;
            }
        }

        public ItemDefinition GetAtPoint(Vector2Int point)
        {
            foreach (var item in _items)
            {
                if (item.Contains(point))
                    return item;
            }

            return null;
        }

        public ItemDefinition[] GetAtPoint(Vector2Int point, Vector2Int size)
        {
            var posibleItems = new ItemDefinition[size.x * size.y];
            var c = 0;
            for (var x = 0; x < size.x; x++)
            {
                for (var y = 0; y < size.y; y++)
                {
                    posibleItems[c] = GetAtPoint(point + new Vector2Int(x, y));
                    c++;
                }
            }
            return posibleItems.Distinct().Where(x => x != null).ToArray();
        }

        public bool TryRemove(ItemDefinition item)
        {
            if (!CanRemove(item)) 
                return false;

            _items.Remove(item);

            onItemRemoved?.Invoke(item);

            return true;
        }

        public bool TryDrop(ItemDefinition item)
        {
            if (!CanDrop(item))
                return false;

            _items.Remove(item);

            var collectible = Instantiate(item.Prefab);
            
            var hasHit = Physics.Raycast(transform.position, Vector3.down, out var hit, 5f, 1 << 0);
            if (hasHit)
            {
                var groundOffset = collectible.Collider.bounds.extents.y * collectible.transform.lossyScale.y + 0.25f;
                collectible.transform.position = hit.point + Vector3.up * groundOffset;
            }
            else
                collectible.transform.position = transform.position;

            onItemDropped?.Invoke(item);

            return true;
        }

        public bool CanAddAt(ItemDefinition item, Vector2Int point)
        {
            if (IsFull)
                return false;

            var previousPoint = item.Position;
            item.Position = point;
            var padding = Vector2.one * 0.01f;

            // check if item is outside of inventory
            if (!_fullRect.Contains(item.GetMinPoint() + padding) || !_fullRect.Contains(item.GetMaxPoint() - padding))
            {
                item.Position = previousPoint;
                return false;
            }

            // check if item overlaps another item already in the inventory
            if (!_items.Any(otherItem => item.Overlaps(otherItem)))
                return true; // Item can be added

            item.Position = previousPoint;

            return false;

        }

        public bool CanAdd(ItemDefinition item)
        {
            Vector2Int point;

            if (!Contains(item) && GetFirstPointThatFitsItem(item, out point))
            {
                return CanAddAt(item, point);
            }

            return false;
        }

        public bool TryAddAt(ItemDefinition item, Vector2Int point)
        {
            if (!CanAddAt(item, point))
                return false;

            item.Position = point;

            _items.Add(item);

            onItemAdded?.Invoke(item);
            return true;
        }

        public bool TryAdd(ItemDefinition item)
        {
            if (!CanAdd(item))
                return false;

            Vector2Int point;

            return GetFirstPointThatFitsItem(item, out point) && TryAddAt(item, point);
        }

        public void DropAll()
        {
            var itemsToDrop = _items.ToArray();
            foreach (var item in itemsToDrop)
            {
                TryDrop(item);
            }
        }

        public void Clear()
        {
            foreach (var item in _items)
            {
                TryRemove(item);
            }
        }

        public bool Contains(ItemDefinition item)
        {
            return _items.Contains(item);
        }

        public bool CanRemove(ItemDefinition item)
        {
            return Contains(item);
        }

        public bool CanDrop(ItemDefinition item)
        {
            return Contains(item) && item.CanDrop;
        }

        private bool GetFirstPointThatFitsItem(ItemDefinition item, out Vector2Int point)
        {
            if (DoesItemFit(item))
            {
                for (var x = 0; x < Width - (item.Width - 1); x++)
                {
                    for (var y = 0; y < Height - (item.Height - 1); y++)
                    {
                        point = new Vector2Int(x, y);

                        if (CanAddAt(item, point)) 
                            return true;
                    }
                }
            }

            point = Vector2Int.zero;
            return false;
        }

        private bool DoesItemFit(ItemDefinition item)
        {
            return item.Width <= Width && item.Height <= Height;
        }
    }
}
