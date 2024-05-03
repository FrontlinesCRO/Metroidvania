using Assets.Scripts.Interfaces;
using Assets.Scripts.InventorySystem;
using Assets.Scripts.InventorySystem.Items;
using Assets.Scripts.Player;
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
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField]
        private Slider _healthSlider;
        [SerializeField]
        private TMPro.TMP_Text _healthText;
        [SerializeField]
        private InventoryRenderer _inventoryRenderer;

        private PlayerController _player;

        public void Initialize(PlayerController player)
        {
            _player = player;

            // health bar init
            _healthSlider.minValue = 0;
            _healthSlider.maxValue = _player.MaxHealth;

            // inventory init
            _inventoryRenderer.SetInventory(_player.Inventory);
            _inventoryRenderer.ItemClickAction = UseItem;

            UpdateHealthBar(_player.Health);

            if (_player)
            {
                _player.HealthChanged += UpdateHealthBar;
                _player.Input.Player.Inventory.Performed += ToggleInventory;
            }
        }

        private void UpdateHealthBar(int health)
        {
            _healthSlider.value = health;

            _healthText.text = $"HP: {health}/{_player.MaxHealth}";
        }

        private void ToggleInventory()
        {
            _inventoryRenderer.Toggle();
        }

        private void UseItem(ItemDefinition item, PointerEventData.InputButton button)
        {
            if (button != PointerEventData.InputButton.Right)
                return;

            if (item is IUseable useableItem && useableItem.CanUse(_player, out _))
            {
                useableItem.Use(_player);

                _player.Inventory.TryRemove(item);
            }
        }
    }
}
