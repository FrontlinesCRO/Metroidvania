using FarrokhGames.Inventory;
using UnityEngine;

namespace Assets.Scripts.InventorySystem.Items
{
    [CreateAssetMenu(fileName = "Item", menuName = "Custom/Item", order = 0)]
    public class ItemDefinition : ScriptableObject
    {
        [SerializeField]
        private string _id;
        [SerializeField]
        private string _name;
        [SerializeField]
        private int _value;
        [SerializeField]
        private Sprite _sprite;
        [SerializeField]
        private Collectible _prefab;
        [SerializeField]
        private Vector2Int _size;
        [SerializeField]
        private bool _canDrop;

        public Vector2Int Position { get; set; }

        public string ID => _id;
        public string Name => _name;
        public int Value => _value;
        public Sprite Sprite => _sprite;
        public Collectible Prefab => _prefab;
        public int Width => _size.x;
        public int Height => _size.y;
        public bool CanDrop => _canDrop;

        public bool IsPartOfShape(Vector2Int localPosition)
        {
            if (localPosition.x < 0 || localPosition.x >= Width || localPosition.y < 0 || localPosition.y >= Height)
            {
                return false; // outside of shape width/height
            }

            return true;
        }

        /// <summary>
        /// Returns the lower left corner position of an item 
        /// within its inventory
        /// </summary>
        public Vector2Int GetMinPoint()
        {
            return Position;
        }

        /// <summary>
        /// Returns the top right corner position of an item 
        /// within its inventory
        /// </summary>
        public Vector2Int GetMaxPoint()
        {
            return Position + new Vector2Int(Width, Height);
        }

        /// <summary>
        /// Returns true if this item overlaps the given point within an inventory
        /// </summary>
        public bool Contains(Vector2Int inventoryPoint)
        {
            for (var iX = 0; iX < Width; iX++)
            {
                for (var iY = 0; iY < Height; iY++)
                {
                    var iPoint = Position + new Vector2Int(iX, iY);
                    if (iPoint == inventoryPoint) { return true; }
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true of this item overlaps a given item
        /// </summary>
        public bool Overlaps(ItemDefinition otherItem)
        {
            if (otherItem == this)
                return false;

            for (var iX = 0; iX < Width; iX++)
            {
                for (var iY = 0; iY < Height; iY++)
                {
                    if (IsPartOfShape(new Vector2Int(iX, iY)))
                    {
                        var iPoint = Position + new Vector2Int(iX, iY);
                        for (var oX = 0; oX < otherItem.Width; oX++)
                        {
                            for (var oY = 0; oY < otherItem.Height; oY++)
                            {
                                if (otherItem.IsPartOfShape(new Vector2Int(oX, oY)))
                                {
                                    var oPoint = otherItem.Position + new Vector2Int(oX, oY);
                                    if (oPoint == iPoint)
                                        return true;    // Hit! Items overlap
                                }
                            }
                        }
                    }
                }
            }

            return false; // Items does not overlap
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(_name))
                return base.ToString();

            return _name;
        }

        public ItemDefinition CreateInstance()
        {
            return ScriptableObject.Instantiate(this);
        }
    }
}
