using Assets.Scripts.Interfaces;
using Assets.Scripts.Utilities;
using UnityEngine;

namespace Assets.Scripts.Volumes
{
    public class DeathTriggerVolume : MonoBehaviour
    {
        private void Destroy(Collider other)
        {
            if (!other.TryGetComponentOnCollider(out IDestructible destructible))
                return;

            destructible.Destroy();
        }

        private void OnTriggerEnter(Collider other)
        {
            Destroy(other);
        }
    }
}
