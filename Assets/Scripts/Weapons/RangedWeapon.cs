using Assets.Scripts.Projectiles;
using UnityEngine;

namespace Assets.Scripts.Weapons
{
    public class RangedWeapon : Weapon
    {
        [SerializeField]
        private Projectile _projectilePrefab;
        [SerializeField]
        private Transform _firePoint;
        [SerializeField]
        private int _damage = 1;
        [SerializeField]
        private float _fireVelocity = 30f;
        [SerializeField]
        private float _rateOfFire = 20f;
        [SerializeField]
        private bool _autoFire = false;

        private float _fireTime;
        private bool _fireing;

        public void Update()
        {
            var dt = Time.deltaTime;

            if (_fireTime > 0f)
            {
                _fireTime -= dt;
                return;
            }

            if (_fireing || _autoFire)
                Fire();
        }

        public override void Fire()
        {
            if (_fireTime > 0f)
                return;

            _fireTime = 1f / _rateOfFire;

            var projectile = Instantiate(_projectilePrefab, _firePoint.position, _firePoint.rotation);
            projectile.Launch(_firePoint.forward * _fireVelocity, _damage, gameObject);

            _fireing = true;
        }

        public override void Release()
        {
            _fireing = false;
        }

    }
}
