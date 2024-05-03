using Assets.Scripts.Interfaces;
using Assets.Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Volumes
{
    [RequireComponent(typeof(Collider))]
    public class DamageTriggerVolume : MonoBehaviour
    {
        [SerializeField]
        private Collider _triggerCollider;
        [SerializeField]
        private int _damage = 1;
        [SerializeField]
        private Vector3 _knockbackForce = Vector3.up * 300f;

        private void DoDamage(Collider other)
        {
            if (!other.TryGetComponentOnCollider(out IDestructible destructible))
                return;

            destructible.DealDamage(_damage, gameObject);

            var otherRigidBody = other.attachedRigidbody;
            if (!otherRigidBody)
                return;
            
            otherRigidBody.AddForce(_knockbackForce, ForceMode.Impulse);
        }

        private void OnTriggerEnter(Collider other)
        {
            DoDamage(other);
        }

        private void OnTriggerStay(Collider other)
        {
            DoDamage(other);
        }
    }
}
