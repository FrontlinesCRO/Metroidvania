using Assets.Scripts.InventorySystem.Items;
using Assets.Scripts.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.InventorySystem
{
    [RequireComponent(typeof(Collider))]
    public class Collectible : MonoBehaviour
    {
        [SerializeField]
        private Collider _triggerCollider;
        [SerializeField]
        private List<Collider> _physicalColliders;
        [SerializeField]
        private ItemDefinition _item;

        private Collider[] _tempColliders = new Collider[10];
        private Collider _playerCollider;

        public Collider Collider => _triggerCollider;
        public ItemDefinition Item => _item;

        private void OnValidate()
        {
            if (_physicalColliders == null || _physicalColliders.Count <= 0)
            {
                var tempColliders = GetComponentsInChildren<Collider>();

                for (var i = 0; i < tempColliders.Length; i++)
                {
                    if (tempColliders[i].isTrigger)
                        continue;

                    _physicalColliders.Add(tempColliders[i]);
                }
            }
        }

        private void Awake()
        {
            if(!_triggerCollider)
                _triggerCollider = GetComponent<Collider>();
        }

        private void OnEnable()
        {
            _playerCollider = null;

            var count = Physics.OverlapSphereNonAlloc(transform.position, 0.5f, _tempColliders);

            if (count <= 0)
                return;

            for (var i = 0; i < count; i++)
            {
                var otherCollider = _tempColliders[i];

                if (!IsPlayer(otherCollider))
                    continue;

                _playerCollider = otherCollider;
                break;
            }

            if (_playerCollider == null)
                return;

            IgnoreCollisionWithPlayer(true);
        }

        private void OnDisable()
        {
            if (!_playerCollider)
                return;

            IgnoreCollisionWithPlayer(false);
        }

        private void IgnoreCollisionWithPlayer(bool ignore)
        {
            for (var i = 0; i < _physicalColliders.Count; i++)
                Physics.IgnoreCollision(_physicalColliders[i], _playerCollider, ignore);
        }

        private bool IsPlayer(Collider other)
        {
            return other.CompareTag("Player");
        }

        private void Collect(Collider other)
        {
            if (!other.TryGetComponentOnCollider(out Inventory inventory))
                return;

            if (inventory.TryAdd(_item.CreateInstance()))
                gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsPlayer(other) || _playerCollider)
                return;

            Collect(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsPlayer(other))
                return;

            IgnoreCollisionWithPlayer(false);
            _playerCollider = null;
        }

        private void OnCollisionEnter(Collision collision)
        {
            var collider = collision.collider;

            if (!IsPlayer(collider))
                return;

            Collect(collider);
        }
    }
}
