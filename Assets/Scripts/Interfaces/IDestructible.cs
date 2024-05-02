using System;
using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public delegate void DestructionEvent(IDestructible destructible);

    public interface IDestructible
    {
        int Health { get; }
        bool IsInvulnerable { get; }
        bool IsDestroyed { get; }
        Vector3 Position { get; }

        void DealDamage(int damage, GameObject damageSource = null, Action onDestroyed = null);
        void Destroy();

        public event DestructionEvent Destroyed;
    }
}
