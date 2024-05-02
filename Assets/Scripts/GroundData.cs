using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public struct GroundData
    {
        private RaycastHit _groundHit;
        private Collider _colliderCache;
        private Transform _transformCache;
        private Rigidbody _rigidbodyCache;
        private Vector3 _point;
        private Vector3 _normal;
        private Vector3 _interpolatedNormal;
        private Vector3 _totalNormal;
        private float _distance;
        private float _angle;

        public bool HasGround => _colliderCache; // collider will always be set if there is a ground

        /// <summary>
        /// The ground collider.
        /// </summary>
        public Collider Collider => _colliderCache;

        /// <summary>
        /// The ground transform.
        /// </summary>
        public Transform Transform => _transformCache;

        /// <summary>
        /// The attached rigidbody of the ground collider.
        /// </summary>
        public Rigidbody RigidBody => _rigidbodyCache;

        /// <summary>
        /// The ground point where the object is standing on.
        /// </summary>
        public Vector3 Point => _point;

        /// <summary>
        /// The ground normal.
        /// </summary>
        /// <remarks>If not grounded this property returns the world up vector (0,1,0).</remarks>
        public Vector3 Normal => _colliderCache ? _normal : Vector3.up;

        /// <summary>
        /// The interpolated ground normal returned by the sphere cast.
        /// </summary>
        /// <remarks>If not grounded this property returns the world up vector (0,1,0).</remarks>
        public Vector3 InterpolatedNormal => _colliderCache ? _interpolatedNormal : Vector3.up;

        /// <summary>
        /// The total normal from all valid hits.
        /// </summary>
        public Vector3 TotalNormal
        {
            get => _totalNormal;
            set => _totalNormal = value;
        }

        /// <summary>
        /// The height distance between the hit ground point and the position of the controller.
        /// </summary>
        /// <remarks>If not grounded it returns the maximum float value.</remarks>
        public float Distance => _colliderCache ? _distance : float.MaxValue;

        /// <summary>
        /// The angle of the ground. (in degrees)
        /// </summary>
        public float Angle => _angle;

        /// <summary>
        /// The velocity of the ground.
        /// </summary>
        /// <remarks>Returns Vector3.zero if no GroundData component on ground object.</remarks>
        public Vector3 Velocity
        {
            get
            {
                if (_rigidbodyCache)
                    return _rigidbodyCache.velocity;

                return Vector3.zero;
            }
        }

        /// <summary>
        /// The angular velocity of the ground.
        /// </summary>
        /// <remarks>Returns Vector3.zero if no GroundData component on ground object.</remarks>
        public Vector3 AngularVelocity
        {
            get
            {
                if (_rigidbodyCache)
                    return _rigidbodyCache.angularVelocity;

                return Vector3.zero;
            }
        }

        /// <summary>
        /// The friction of the ground.
        /// </summary>
        /// <remarks>Returns 1f if no GroundData component on ground object.</remarks>
        public float StaticFriction
        {
            get
            {
                if (_colliderCache && _colliderCache.sharedMaterial)
                {
                    return _colliderCache.sharedMaterial.staticFriction;
                }

                return 1f;
            }
        }

        /// <summary>
        /// The dynamic friction of 
        /// </summary>
        public float DynamicFriction
        {
            get
            {
                if (_colliderCache && _colliderCache.sharedMaterial)
                {
                    return _colliderCache.sharedMaterial.dynamicFriction;
                }

                return 1f;
            }
        }

        /// <summary>
        /// The friction combination to use.
        /// </summary>
        public PhysicMaterialCombine FrictionCombine
        {
            get
            {
                if (_colliderCache && _colliderCache.sharedMaterial)
                {
                    return _colliderCache.sharedMaterial.frictionCombine;
                }

                return PhysicMaterialCombine.Average;
            }
        }

        /// <summary>
        /// The bounciness of the ground.
        /// </summary>
        /// <remarks>Returns 0f if no GroundData component on ground object.</remarks>
        public float Bounciness
        {
            get
            {
                if (_colliderCache && _colliderCache.sharedMaterial)
                {
                    return _colliderCache.sharedMaterial.bounciness;
                }

                return 0f;
            }
        }

        //public GroundData()
        //{
        //    _groundHit = default;
        //    _transformCache = null;
        //    _colliderCache = null;
        //    _rigidbodyCache = null;
        //    _point = default;
        //    _normal = default;
        //    _interpolatedNormal = default;
        //    _totalNormal = default;
        //    _distance = 0f;
        //    _angle = 0f;
        //}

        public void SetValues(Vector3 point, Vector3 normal, Vector3 interpolatedNormal, float distance, float angle, RaycastHit groundHit)
        {
            _groundHit = groundHit;
            _transformCache = _groundHit.transform;
            _colliderCache = _groundHit.collider;

            if (_colliderCache)
            {
                _rigidbodyCache = _colliderCache.attachedRigidbody;
            }
            else
            {
                _rigidbodyCache = null;
            }

            _point = point;
            _normal = normal;
            _interpolatedNormal = interpolatedNormal;
            _distance = distance;
            _angle = angle;
        }

        public void Clear()
        {
            _groundHit = default;
            _transformCache = null;
            _colliderCache = null;
            _rigidbodyCache = null;
            _point = default;
            _normal = default;
            _interpolatedNormal = default;
            _distance = 0f;
            _angle = 0f;
        }

        /// <summary>
        /// Check if the ground is tagged with a certain tag.
        /// </summary>
        /// <param name="tag">The tag to evaluate.</param>
        /// <returns>True if it is tagged with the given tag, else False.</returns>
        public bool IsTaggedWith(string tag)
        {
            if (_colliderCache && _colliderCache.CompareTag(tag))
                return true;

            if (_rigidbodyCache && _rigidbodyCache.CompareTag(tag))
                return true;

            return false;
        }

        /// <summary>
        /// Checks if any of the given colliders are the ground collider.
        /// </summary>
        /// <param name="colliders">The colliders we are evaluating.</param>
        /// <returns>True if any of the given colliders is the ground collider, else False.</returns>
        public bool IsGroundCollider(params Collider[] colliders)
        {
            if (!_colliderCache) return false;

            for (var i = 0; i < colliders.Length; i++)
            {
                var otherCollider = colliders[i];

                if (!otherCollider) continue;
                else if (otherCollider == _colliderCache)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the velocity at the point of contact with the ground.
        /// </summary>
        /// <returns>The velocity at the ground point.</returns>
        public Vector3 GetPointVelocity()
        {
            if (_rigidbodyCache)
            {
                return _rigidbodyCache.GetPointVelocity(Point);
            }

            return Vector3.zero;
        }
    }
}
