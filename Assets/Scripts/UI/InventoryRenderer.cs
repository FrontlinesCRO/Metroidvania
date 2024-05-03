using Assets.Scripts.InventorySystem;
using Assets.Scripts.InventorySystem.Items;
using FarrokhGames.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static FarrokhGames.Inventory.InventoryDraggedItem;
using static UnityEditor.Progress;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class InventoryRenderer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private static InventoryItemRenderer s_draggedItem;

        [SerializeField]
        private RectTransform _canvasRectTransform;
        [SerializeField]
        private RectTransform _rectTransform;
        [SerializeField, Tooltip("The size of the cells building up the inventory")]
        private Vector2Int _cellSize = new Vector2Int(32, 32);
        [SerializeField, Tooltip("The sprite to use for empty cells")]
        private Sprite _cellSpriteEmpty = null;
        [SerializeField, Tooltip("The sprite to use for selected cells")]
        private Sprite _cellSpriteSelected = null;
        [SerializeField, Tooltip("The sprite to use for blocked cells")]
        private Sprite _cellSpriteBlocked = null;
        
        private Inventory _inventory;
        private bool _haveListeners;
        private bool _amDraggingItem;
        private Pool<Image> _imagePool;
        private Pool<InventoryItemRenderer> _itemRendererPool;
        private Image[] _grids;
        private Dictionary<ItemDefinition, InventoryItemRenderer> _items = new Dictionary<ItemDefinition, InventoryItemRenderer>();

        public RectTransform canvasRectTransform => _canvasRectTransform;
        public RectTransform rectTransform => _rectTransform;
        public Inventory Inventory => _inventory;
        public Vector2 CellSize => _cellSize;

        public Action<ItemDefinition, PointerEventData.InputButton> ItemClickAction;

        private void OnValidate()
        {
            if (!_rectTransform)
                _rectTransform = GetComponent<RectTransform>();

            if (!_canvasRectTransform)
            {
                var canvases = GetComponentsInParent<Canvas>();

                if (canvases.Length == 0)
                    throw new NullReferenceException("Could not find a canvas.");

                _canvasRectTransform = canvases[canvases.Length - 1].transform as RectTransform;
            }
        }

        private void Awake()
        {
            // Get the rect transforms if not set
            if (!_canvasRectTransform)
            {
                var canvases = GetComponentsInParent<Canvas>();

                if (canvases.Length == 0)
                    throw new NullReferenceException("Could not find a canvas.");

                _canvasRectTransform = canvases[canvases.Length - 1].transform as RectTransform;
            }

            if (!_rectTransform)
                _rectTransform = GetComponent<RectTransform>();

            // Create the image container
            var imageContainer = new GameObject("Image Container").AddComponent<RectTransform>();
            imageContainer.transform.SetParent(transform);
            imageContainer.transform.localPosition = Vector3.zero;
            imageContainer.transform.localScale = Vector3.one;

            // Create pool of images
            _imagePool = new Pool<Image>(
                delegate
                {
                    var image = new GameObject("Image").AddComponent<Image>();
                    image.transform.SetParent(imageContainer);
                    image.transform.localScale = Vector3.one;
                    return image;
                });

            // Create pool of item renderers
            _itemRendererPool = new Pool<InventoryItemRenderer>(
                delegate
                {
                    var itemRendererImage = new GameObject("Item Renderer").AddComponent<Image>();
                    var itemRenderer = itemRendererImage.AddComponent<InventoryItemRenderer>();
                    itemRenderer.transform.SetParent(imageContainer);
                    itemRenderer.transform.localScale = Vector3.one;
                    return itemRenderer;
                });

            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (_inventory != null && !_haveListeners)
            {
                if (_cellSpriteEmpty == null) { throw new NullReferenceException("Sprite for empty cell is null"); }
                if (_cellSpriteSelected == null) { throw new NullReferenceException("Sprite for selected cells is null."); }
                if (_cellSpriteBlocked == null) { throw new NullReferenceException("Sprite for blocked cells is null."); }

                _inventory.onItemAdded += HandleItemAdded;
                _inventory.onItemRemoved += HandleItemRemoved;
                _inventory.onItemDropped += HandleItemRemoved;
                _inventory.onResized += HandleResized;
                _haveListeners = true;

                // Render inventory
                ReRenderGrid();
                ReRenderAllItems();
            }
        }

        private void OnDisable()
        {
            if (_inventory != null && _haveListeners)
            {
                _inventory.onItemAdded -= HandleItemAdded;
                _inventory.onItemRemoved -= HandleItemRemoved;
                _inventory.onItemDropped -= HandleItemRemoved;
                _inventory.onResized -= HandleResized;
                _haveListeners = false;
            }

            if (_amDraggingItem)
                s_draggedItem = null;

            _amDraggingItem = false;
        }

        public void SetInventory(Inventory inventory)
        {
            _inventory = inventory;
        }

        public void Toggle()
        {
            var isActive = !isActiveAndEnabled;
            gameObject.SetActive(isActive);
        }

        private void ReRenderGrid()
        {
            // Clear the grid
            if (_grids != null)
            {
                for (var i = 0; i < _grids.Length; i++)
                {
                    _grids[i].gameObject.SetActive(false);

                    RecycleImage(_grids[i]);

                    _grids[i].transform.SetSiblingIndex(i);
                }
            }

            _grids = null;

            // Render new grid
            var containerSize = new Vector2(CellSize.x * _inventory.Width, CellSize.y * _inventory.Height);
            Image grid;

            var topLeft = new Vector3(-containerSize.x / 2, -containerSize.y / 2, 0); // Calculate topleft corner
            var halfCellSize = new Vector3(CellSize.x / 2, CellSize.y / 2, 0); // Calulcate cells half-size
            _grids = new Image[_inventory.Width * _inventory.Height];
            var c = 0;
            for (int y = 0; y < _inventory.Height; y++)
            {
                for (int x = 0; x < _inventory.Width; x++)
                {
                    grid = CreateImage(_cellSpriteEmpty, true);
                    grid.gameObject.name = "Grid " + c;
                    grid.rectTransform.SetAsFirstSibling();
                    grid.type = Image.Type.Sliced;
                    grid.rectTransform.localPosition = topLeft + new Vector3(CellSize.x * ((_inventory.Width - 1) - x), CellSize.y * y, 0) + halfCellSize;
                    grid.rectTransform.sizeDelta = CellSize;
                    _grids[c] = grid;
                    c++;
                }
            }

            // Set the size of the main RectTransform
            // This is useful as it allowes custom graphical elements
            // suchs as a border to mimic the size of the inventory.
            rectTransform.sizeDelta = containerSize;
        }

        private void ReRenderAllItems()
        {
            // Clear all items
            foreach (var itemRenderer in _items.Values)
            {
                itemRenderer.gameObject.SetActive(false);
                RecycleItemRenderer(itemRenderer);
            }
            _items.Clear();

            // Add all items
            foreach (var item in _inventory.Items)
            {
                HandleItemAdded(item);
            }
        }

        private void HandleItemAdded(ItemDefinition item)
        {
            var itemRenderer = CreateItemRenderer(item, false);

            itemRenderer.RectTransform.localPosition = GetItemOffset(item);

            _items.Add(item, itemRenderer);
        }

        private void HandleItemRemoved(ItemDefinition item)
        {
            if (_items.ContainsKey(item))
            {
                var itemRenderer = _items[item];

                itemRenderer.gameObject.SetActive(false);

                RecycleItemRenderer(itemRenderer);

                _items.Remove(item);
            }
        }

        private void HandleResized()
        {
            ReRenderGrid();
            ReRenderAllItems();
        }

        private Image CreateImage(Sprite sprite, bool raycastTarget)
        {
            var img = _imagePool.Take();
            img.gameObject.SetActive(true);
            img.sprite = sprite;
            img.rectTransform.sizeDelta = new Vector2(img.sprite.rect.width, img.sprite.rect.height);
            img.transform.SetAsLastSibling();
            img.type = Image.Type.Simple;
            img.raycastTarget = raycastTarget;
            return img;
        }

        private InventoryItemRenderer CreateItemRenderer(ItemDefinition item, bool raycastTarget)
        {
            var itemRenderer = _itemRendererPool.Take();
            itemRenderer.gameObject.SetActive(true);
            itemRenderer.transform.SetAsLastSibling();
            itemRenderer.Setup(item, this);
            itemRenderer.ItemClicked = OnItemClicked;
            itemRenderer.ItemPickedUp = OnItemPickedUp;
            itemRenderer.ItemReleased = OnItemReleased;

            return itemRenderer;
        }

        private void OnItemClicked(InventoryItemRenderer itemRenderer, PointerEventData.InputButton button)
        {
            ItemClickAction?.Invoke(itemRenderer.Item, button);
        }

        private void OnItemPickedUp(InventoryItemRenderer itemRenderer)
        {
            _amDraggingItem = true;
            s_draggedItem = itemRenderer;
        }

        private void OnItemReleased(InventoryItemRenderer itemRenderer)
        {
            _amDraggingItem = false;
            s_draggedItem = null;
        }

        private void RecycleImage(Image image)
        {
            image.gameObject.name = "Image";
            image.gameObject.SetActive(false);

            _imagePool.Recycle(image);
        }

        private void RecycleItemRenderer(InventoryItemRenderer itemRenderer)
        {
            itemRenderer.gameObject.name = "Item Renderer";
            itemRenderer.gameObject.SetActive(false);
            itemRenderer.ItemPickedUp = null;
            itemRenderer.ItemReleased = null;

            _itemRendererPool.Recycle(itemRenderer);
        }

        /// <summary>
        /// Selects a given item in the inventory.
        /// </summary>
        /// <param name="item">Item to select</param>
        /// <param name="blocked">Should the selection be rendered as blocked</param>
        /// <param name="color">The color of the selection</param>
        public void SelectItem(ItemDefinition item, bool blocked, Color color)
        {
            if (item == null)
                return;

            ClearSelection();

            for (var x = 0; x < item.Width; x++)
            {
                for (var y = 0; y < item.Height; y++)
                {
                    if (item.IsPartOfShape(new Vector2Int(x, y)))
                    {
                        var p = item.Position + new Vector2Int(x, y);
                        if (p.x >= 0 && p.x < _inventory.Width && p.y >= 0 && p.y < _inventory.Height)
                        {
                            var index = p.y * _inventory.Width + ((_inventory.Width - 1) - p.x);
                            _grids[index].sprite = blocked ? _cellSpriteBlocked : _cellSpriteSelected;
                            _grids[index].color = color;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clears all selections made in this inventory
        /// </summary>
        public void ClearSelection()
        {
            for (var i = 0; i < _grids.Length; i++)
            {
                _grids[i].sprite = _cellSpriteEmpty;
                _grids[i].color = Color.white;
            }
        }

        /// <summary>
        /// Returns the appropriate offset of an item to make it fit nicely in the grid
        /// </summary>
        public Vector2 GetItemOffset(ItemDefinition item)
        {
            var x = (-(_inventory.Width * 0.5f) + item.Position.x + item.Width * 0.5f) * CellSize.x;
            var y = (-(_inventory.Height * 0.5f) + item.Position.y + item.Height * 0.5f) * CellSize.y;

            return new Vector2(x, y);
        }

        public Vector2Int ScreenToGrid(Vector2 screenPoint)
        {
            var pos = ScreenToLocalPositionInRenderer(screenPoint);
            var sizeDelta = rectTransform.sizeDelta;

            pos.x += sizeDelta.x / 2;
            pos.y += sizeDelta.y / 2;

            return new Vector2Int(Mathf.FloorToInt(pos.x / CellSize.x), Mathf.FloorToInt(pos.y / CellSize.y));
        }

        public Vector2 ScreenToLocalPositionInRenderer(Vector2 screenPosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                screenPosition,
                null,
                out var localPosition
            );

            return localPosition;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("Pointer inside Inventory panel");
            if (s_draggedItem != null)
            {
                // Change which controller is in control of the dragged item
                s_draggedItem.CurrentInventoryRenderer = this;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("Pointer outside Inventory panel");
            if (s_draggedItem != null)
            {
                // Clear the item as it leaves its current controller
                s_draggedItem.CurrentInventoryRenderer = null;

                ClearSelection();
            }
        }
    }
}
