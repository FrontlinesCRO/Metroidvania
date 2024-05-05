using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Player
{
    public class PlayerCameraController : MonoBehaviour
    {
        [SerializeField]
        private Transform _target;
        [SerializeField]
        private Vector3 _offset;
        [SerializeField]
        private float _minSmoothTime = 0.1f;
        [SerializeField]
        private float _maxSmoothTime = 0.3f;
        [SerializeField]
        private float _maxSpeed = 30f;
        [SerializeField]
        private bool _setCurrentOffsetFromTargetAsOffset;

        private float _desiredSqrDistanceToTarget;
        private Vector3 _velocity = Vector3.zero;

        private void OnValidate()
        {
            if (_setCurrentOffsetFromTargetAsOffset)
            {
                _offset = transform.position - _target.position;
                _setCurrentOffsetFromTargetAsOffset = false;
            }
        }

        private void Start()
        {
            if (transform.parent)
                transform.SetParent(null, true);

            transform.position = _target.TransformPoint(_offset);

            _desiredSqrDistanceToTarget = (_target.position - transform.position).sqrMagnitude;
        }

        private void LateUpdate()
        {
            var dt = Time.deltaTime;

            var followPosition = _target.position + _offset;
            var sqrDistanceToTarget = (_target.position - transform.position).sqrMagnitude;

            var t = sqrDistanceToTarget / (_desiredSqrDistanceToTarget * 2f);
            var smoothTime = Mathf.Lerp(_minSmoothTime, _maxSmoothTime, t);

            transform.position = Vector3.SmoothDamp(transform.position, followPosition, ref _velocity, smoothTime, _maxSpeed, dt);
        }

        public void PositionToTarget()
        {
            transform.position = _target.TransformPoint(_offset);
        }
    }
}
