using Assets.Scripts.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface IInteractable
    {
        bool CanInteract(PlayerController player, out string response);
        bool TryInteract(PlayerController player)
        {
            if (!CanInteract(player, out _))
                return false;

            Interact(player);
            return true;
        }
        void Interact(PlayerController player);
    }
}
