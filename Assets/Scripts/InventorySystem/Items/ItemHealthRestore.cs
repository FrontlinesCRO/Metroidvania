using Assets.Scripts.Interfaces;
using Assets.Scripts.Player;
using UnityEngine;

namespace Assets.Scripts.InventorySystem.Items
{
    public class ItemHealthRestore : ItemDefinition, IUseable
    {
        [SerializeField]
        private int _healthRestoreAmount;

        public bool CanUse(PlayerController player, string response)
        {
            return player.Health < player.MaxHealth;
        }

        public void Use(PlayerController player)
        {
            player.Heal(_healthRestoreAmount);
        }
    }
}
