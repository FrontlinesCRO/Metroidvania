using Assets.Scripts.InventorySystem;
using Assets.Scripts.InventorySystem.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(Image), typeof(RectTransform))]
    public class InventoryItemRenderer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerDownHandler, IPointerClickHandler
    {
        [SerializeField]
        private Image _image;

        private RectTransform _rectTransform;
        private ItemDefinition _item;
        private InventoryRenderer _originalInventoryRenderer;
        private Vector2Int _originPoint;
        private Vector2 _offset;

        public ItemDefinition Item => _item;
        public InventoryRenderer OriginalInventoryRenderer => _originalInventoryRenderer;
        public InventoryRenderer CurrentInventoryRenderer { get; set; }
        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();

                return _rectTransform;
            }
        }

        public Action<InventoryItemRenderer, PointerEventData.InputButton> ItemClicked;
        public Action<InventoryItemRenderer> ItemPickedUp;
        public Action<InventoryItemRenderer> ItemReleased;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _image = GetComponent<Image>();
        }

        public void Setup(ItemDefinition item, InventoryRenderer inventoryRenderer)
        {
            _item = item;
            _originPoint = item.Position;
            _originalInventoryRenderer = inventoryRenderer;
            CurrentInventoryRenderer = inventoryRenderer;

            if (!_rectTransform)
                _rectTransform = GetComponent<RectTransform>();

            if (!_image)
                _image = GetComponent<Image>();

            _image.sprite = item.Sprite;
            _image.rectTransform.sizeDelta = new Vector2(inventoryRenderer.CellSize.x * item.Width, inventoryRenderer.CellSize.y * item.Height) * 0.8f;
            _image.preserveAspect = true;
            _image.type = Image.Type.Simple;
            _image.raycastTarget = true;
        }

        private Vector2 GetDraggedItemOffset(InventoryRenderer renderer, ItemDefinition item)
        {
            var scale = new Vector2(
                Screen.width / renderer.canvasRectTransform.sizeDelta.x,
                Screen.height / renderer.canvasRectTransform.sizeDelta.y
            );

            var gx = -(item.Width * renderer.CellSize.x / 2f) + (renderer.CellSize.x / 2);
            var gy = -(item.Height * renderer.CellSize.y / 2f) + (renderer.CellSize.y / 2);

            return new Vector2(gx, gy) * scale;
        }

        private void UpdatePosition(Vector2 position)
        {
            var canvasRect = OriginalInventoryRenderer.canvasRectTransform;

            // Move the image
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, position + _offset, null, out var newValue);
            _image.rectTransform.localPosition = newValue;

            // Make selections
            if (CurrentInventoryRenderer != null)
            {
                Item.Position = CurrentInventoryRenderer.ScreenToGrid(position + _offset + GetDraggedItemOffset(CurrentInventoryRenderer, Item));

                var canAdd = CurrentInventoryRenderer.Inventory.CanAddAt(Item, Item.Position);

                CurrentInventoryRenderer.SelectItem(Item, !canAdd, Color.white);
            }

            // Slowly animate the item towards the center of the mouse pointer
            _offset = Vector2.Lerp(_offset, Vector2.zero, Time.deltaTime * 10f);
        }

        private void Drop(Vector2 position)
        {
            if (CurrentInventoryRenderer != null)
            {
                var originalInventory = OriginalInventoryRenderer.Inventory;
                var currentInventory = CurrentInventoryRenderer.Inventory;
                var grid = CurrentInventoryRenderer.ScreenToGrid(position + _offset + GetDraggedItemOffset(CurrentInventoryRenderer, _item));

                originalInventory.TryRemove(Item);

                // Try to add new item
                if (currentInventory.CanAddAt(_item, grid))
                {
                    // Place the item in a new location
                    currentInventory.TryAddAt(_item, grid); 
                }
                // Could not add, return the item 
                else
                {
                    // return the item to its previous location
                    originalInventory.TryAddAt(_item, _originPoint);
                }

                CurrentInventoryRenderer.ClearSelection();
            }
            else
            {
                var originalInventory = OriginalInventoryRenderer.Inventory;

                if (!originalInventory.TryDrop(_item)) // Drop the item on the ground
                {
                    originalInventory.TryAddAt(_item, _originPoint);
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            CurrentInventoryRenderer.ClearSelection();

            var localPosition = CurrentInventoryRenderer.ScreenToLocalPositionInRenderer(eventData.position);
            var itemOffest = CurrentInventoryRenderer.GetItemOffset(Item);
            _offset = itemOffest - localPosition;
            _originPoint = Item.Position;
            _image.raycastTarget = false;
            // Remove the item from inventory
            //CurrentInventoryRenderer.Inventory.TryRemove(Item);

            ItemPickedUp?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            var position = Vector2.zero;
            if (eventData != null)
                position = eventData.position;

            UpdatePosition(position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Drop(eventData.position);

            _image.raycastTarget = true;

            ItemReleased?.Invoke(this);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ItemClicked?.Invoke(this, eventData.button);
        }
    }
}
