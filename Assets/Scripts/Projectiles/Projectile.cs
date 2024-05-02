using Assets.Scripts.Interfaces;
using Assets.Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Projectiles
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody _rigidBody;
        [SerializeField]
        private float _lifetime = 5f;

        private float _remainingLifetime;
        private int _damage;
        private GameObject _source;

        public Rigidbody RigidBody => _rigidBody;

        private void Awake()
        {
            if (!_rigidBody)
                _rigidBody = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            _remainingLifetime = _lifetime;
            _rigidBody.velocity = Vector3.zero;
            _rigidBody.angularVelocity = Vector3.zero;
        }

        private void Update()
        {
            var dt = Time.deltaTime;

            _remainingLifetime -= dt;

            if (_remainingLifetime > 0)
                return;

            gameObject.SetActive(false);
        }

        public void Launch(Vector3 velocity, int damage, GameObject source)
        {
            _damage = damage;
            _source = source;

            _rigidBody.AddForce(velocity, ForceMode.VelocityChange);
        }

        private void DealDamage(IDestructible destructible)
        {
            if (destructible.IsInvulnerable || destructible.IsDestroyed)
                return;

            destructible.DealDamage(_damage, _source);

            gameObject.SetActive(false);
        }

        private void OnCollisionEnter(Collision collision)
        {
            var hitCollider = collision.collider;
            if (!hitCollider.TryGetComponentOnCollider<IDestructible>(out var destructible))
                return;

            DealDamage(destructible);
        }
    }
}
