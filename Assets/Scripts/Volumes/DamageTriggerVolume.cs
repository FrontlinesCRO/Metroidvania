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
    public class DamageTriggerVolume : MonoBehaviour
    {
        [SerializeField]
        private int _damage = 1;
        [SerializeField]
        private float _knockbackForce = 100f;

        private void DoDamage(Collider other)
        {
            if (!other.TryGetComponentOnCollider(out IDestructible destructible))
                return;

            destructible.DealDamage(_damage, gameObject);

            var otherRigidBody = other.attachedRigidbody;
            if (!otherRigidBody)
                return;

            var knockbackDirection = other.transform.position - transform.position;
            otherRigidBody.AddForce(knockbackDirection.normalized * _knockbackForce, ForceMode.Impulse);
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
