using Assets.Scripts.Interfaces;
using Assets.Scripts.Player;
using UnityEngine;

namespace Assets.Scripts.InventorySystem.Items
{
    public class ItemHealthRestore : ItemDefinition, IUseable
    {
        [SerializeField]
        private int _healthRestoreAmount;

        public bool CanUse(PlayerController player, out string response)
        {
            response = string.Empty;

            if (player.Health >= player.MaxHealth)
            {
                response = "Player is at full health.";
                return false;
            }

            return true;
        }

        public void Use(PlayerController player)
        {
            player.Heal(_healthRestoreAmount);
        }
    }
}
