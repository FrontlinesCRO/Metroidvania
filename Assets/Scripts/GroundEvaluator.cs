using System;
using System.Collections.Generic;
using Assets.Scripts.Player;
using Assets.Scripts.Utilities;
using Assets.Scripts.Utilities.Comparers;
using Fpx.Utilities;
using UnityEngine;

namespace Assets.Scripts
{
    [Serializable]
    public struct GroundEvaluator
    {
        #region Constants

        private const int GROUND_HIT_COUNT = 12;
        public static GroundEvaluator Default => new GroundEvaluator
        {
            RaycastDistance = 5f,
            RaycastRadius = 0.3f,
            MaxStepHeight = 0.3f,
            MaxGroundAngle = 45f,
            GroundedDistance = 0.15f,
            Mask = 1,
        };

        #endregion

        #region Structs

        private struct GroundCheckParams
        {
            public Vector3 CheckPosition;
            public Vector3 CheckDirection;
            public float Radius;
        }

        private struct GroundCheckProcessData
        {
            public bool IsGrounded;
            public float SmallestAngle;
            public float SmallestDistance;
            public Vector3 ValidHitNormals;
        }

        #endregion

        public float RaycastDistance;
        public float RaycastRadius;
        public float MaxStepHeight;
        public float MaxGroundAngle;
        public float GroundedDistance;
        public LayerMask Mask;

        // temporary values used when updating ground check
        private PlayerController _controller;
        private RaycastHit[] _groundHitBuffer;
        private GroundData _data;

        public GroundData Data => _data;

        public void Initialize(PlayerController controller)
        {
            _controller = controller;
            _groundHitBuffer = new RaycastHit[GROUND_HIT_COUNT];
            _data = new GroundData();
        }

        public bool Evaluate()
        {
            _data.Clear();

            var processData = new GroundCheckProcessData
            {
                IsGrounded = false,
                SmallestAngle = float.MaxValue,
                SmallestDistance = float.MaxValue,
                ValidHitNormals = Vector3.zero,
            };

            var groundCheckParams = new GroundCheckParams
            {
                CheckPosition = _controller.Centroid,
                CheckDirection = Vector3.down,
                Radius = RaycastRadius
            };

            EvaluateGround(groundCheckParams, ref processData);

            _data.TotalNormal = processData.ValidHitNormals.normalized;

            return processData.IsGrounded;
        }

        //private bool EvaluateFalling()
        //{
        //    return !IsGrounded && Controller.Velocity.y < 0f && Distance > GroundedDistance + 0.35f && !IsGliding;
        //}

        //private bool EvaluateFreeFall()
        //{
        //    return IsFalling && Controller.Velocity.y <= -FreeFallVelocity;
        //}

        private void EvaluateGround(GroundCheckParams groundCheckParams, ref GroundCheckProcessData processData)
        {
            var checkPosition = groundCheckParams.CheckPosition;
            var checkDirection = groundCheckParams.CheckDirection;
            var radius = groundCheckParams.Radius;

            var numberOfHits = Physics.SphereCastNonAlloc(
                checkPosition - (checkDirection * radius),
                radius,
                checkDirection,
                _groundHitBuffer,
                RaycastDistance,
                Mask,
                QueryTriggerInteraction.Collide
            );

            if (numberOfHits <= 0)
                return;

            Array.Sort(_groundHitBuffer, new RaycastHitDistanceComparer());

            for (var j = 0; j < numberOfHits; j++)
            {
                var groundHit = _groundHitBuffer[j];
                var hitCollider = _groundHitBuffer[j].collider;

                if (hitCollider.isTrigger || hitCollider.CompareTag("Unwalkable"))
                    continue;

                // check if the hit collider is part of the controller
                if (_controller.Collider == hitCollider)
                    continue;

                processData.ValidHitNormals += groundHit.normal;

                RaycastHit checkHit;
                float groundAngle;
                float heightDistance;
                var groundDirection = groundHit.point - checkPosition;

                var validGround = EvaluateGroundHitPoint(hitCollider, checkPosition, groundDirection, out groundAngle, out heightDistance, out checkHit);
                if (validGround)
                {
                    if (EvaluateGroundCandidate(groundHit, checkHit, groundAngle, heightDistance, ref processData))
                        continue;
                }

                var rotationAxis = Vector3.Cross(Vector3.up, groundDirection).normalized;
                groundDirection = Quaternion.AngleAxis(3f, rotationAxis) * groundDirection * 1.5f;

                validGround = EvaluateGroundHitPoint(hitCollider, checkPosition, groundDirection, out groundAngle, out heightDistance, out checkHit);
                if (validGround)
                {
                    if (EvaluateGroundCandidate(groundHit, checkHit, groundAngle, heightDistance, ref processData))
                        continue;
                }

                groundDirection = Quaternion.AngleAxis(-6f, rotationAxis) * groundDirection;

                validGround = EvaluateGroundHitPoint(hitCollider, checkPosition, groundDirection, out groundAngle, out heightDistance, out checkHit);
                if (validGround)
                {
                    if (EvaluateGroundCandidate(groundHit, checkHit, groundAngle, heightDistance, ref processData))
                        continue;
                }

                if (!processData.IsGrounded && !_data.Collider)
                {
                    _data.SetValues(groundHit.point, groundHit.normal, groundHit.normal, checkPosition.y - groundHit.point.y, groundAngle, groundHit);
                }
            }
        }

        private bool EvaluateGroundCandidate(RaycastHit groundHit, RaycastHit checkHit, float groundAngle, float heightDistance, ref GroundCheckProcessData processData)
        {
            if (heightDistance < processData.SmallestDistance)
            {
                if (heightDistance.Approximately(processData.SmallestDistance) && groundAngle > processData.SmallestAngle)
                    return false;

                processData.SmallestDistance = heightDistance;
                processData.SmallestAngle = groundAngle;
                processData.IsGrounded = true;

                _data.SetValues(groundHit.point, checkHit.normal, groundHit.normal, heightDistance, groundAngle, groundHit);
                return true;
            }

            return false;
        }

        private bool EvaluateGroundHitPoint(Collider col, Vector3 sourcePoint, Vector3 groundDirection, out float groundAngle, out float heightDistance, out RaycastHit hit, bool ignoreStep = false)
        {
            var validPoint = CheckGroundInDirection(col, sourcePoint, groundDirection, out hit, out groundAngle);

            heightDistance = _controller.Centroid.y - hit.point.y;

            if (!validPoint || heightDistance < 0f) return false;

            if (!ignoreStep)
            {
                // basically check if the hit is below the waist (the lowest collider)
                var stepDistance = heightDistance - _controller.Height * 0.5f;

                if (stepDistance < -MaxStepHeight)
                    return false;
            }

            var grounded = heightDistance < (GroundedDistance + _controller.Height * 0.5f);

            return grounded;
        }

        private bool CheckGroundInDirection(Collider col, Vector3 sourcePoint, Vector3 groundDirection, out RaycastHit hit, out float groundAngle)
        {
            groundAngle = 90f;
            var groundCheckRay = new Ray(sourcePoint, groundDirection);

            var hasHit = col.Raycast(groundCheckRay, out hit, groundDirection.magnitude + 0.01f);

            return hasHit && IsValidGroundNormal(col, hit.normal, out groundAngle);
        }

        private bool IsValidGroundNormal(Collider col, Vector3 groundNormal, out float groundAngle)
        {
            groundAngle = Vector3.Angle(groundNormal, Vector3.up);

            return groundAngle <= MaxGroundAngle;
        }
    }
}
