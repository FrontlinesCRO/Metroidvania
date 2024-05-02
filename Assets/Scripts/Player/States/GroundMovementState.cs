using Assets.Scripts.Utilities;
using System;
using UnityEngine;

namespace Assets.Scripts.Player.States
{
    [Serializable]
    public class GroundMovementState : PlayerState
    {
        [Header("Movement")]
        [SerializeField]
        private float _speed = 6f;
        [SerializeField]
        private float _sprintSpeed = 9f;
        [SerializeField]
        private float _acceleration = 20f;
        [SerializeField]
        private float _decceleration = 10f;
        [SerializeField]
        private float _maxAcceleration = 100f;
        [SerializeField]
        private float _slopeRaycastDistance = 3f;
        [SerializeField]
        private float _slopeCheckAngle = 55f;
        [SerializeField]
        private float _maxSlopeAngle = 45f;
        [SerializeField]
        private LayerMask _slopeCheckMask = 1;
        //[SerializeField, Tooltip("The maxium/idle height of the controller.")]
        //private float _maxHeight = 1.75f;
        //[SerializeField, Tooltip("The minimum/crouch height of the controller.")]
        //private float _minHeight = 0.5f;

        [Header("Rotation")]
        [SerializeField]
        private float _rotationSpeed = 360f;
        [SerializeField]
        private float _rotationSpringStrength = 100f;
        [SerializeField]
        private float _rotationSpringDamper = 10f;

        private Camera _camera;
        private Vector3 _currentVelocity = Vector3.zero;
        private Quaternion _currentRotation = Quaternion.identity;

        private bool IsMoving
        {
            get => Controller.Tags.IsTagActive(TagType.Moving);
            set => Controller.Tags.SetTag(TagType.Moving, value);
        }
        private bool IsSprinting => Controller.Input.Player.Sprint.Pressed;
        //private bool IsJumping => Controller.Tags.IsTagActive(TagType.Jumping);

        public override void Start()
        {
            _camera = Camera.main;
            _currentVelocity = Controller.Velocity;
            _currentRotation = Controller.Rotation;
        }

        public override void End()
        {
            
        }

        public override void Update(float dt)
        {
            MovementVector = GetMovementVector();

            IsMoving = !MovementVector.Approximately(Vector3.zero, 0.001f);

            var groundVelocity = Controller.Ground.GetPointVelocity();

            ApplyMovement(groundVelocity, dt);

            ApplyRotation(dt);
        }

        private Vector3 GetMovementVector()
        {
            var inputVector = Controller.Input.Player.Movement.Value;
            var cameraTransform = _camera.transform;

            var movementVector = cameraTransform.TransformDirection(new Vector3(inputVector.x, 0f, 0f));
            movementVector = movementVector.normalized * inputVector.magnitude;

            if (movementVector.Approximately(Vector3.zero))
                return movementVector;

            var slopeCheckDirection =
                Quaternion.AngleAxis(_slopeCheckAngle, Vector3.Cross(Vector3.up, movementVector.NormalizeXZ())) * movementVector;

            slopeCheckDirection.Normalize();

            Ray slopeCheckRay = new Ray(Controller.Centroid, slopeCheckDirection);
            var hasHit = Physics.Raycast(slopeCheckRay, out var slopeHit, _slopeRaycastDistance, _slopeCheckMask,
                QueryTriggerInteraction.Ignore);

            if (!hasHit) 
                return movementVector;

            // perform a wall check
            Ray wallCheckRay = new Ray(Controller.Centroid, movementVector);

            var wallCheckDistance = new Vector2(slopeCheckDirection.x, slopeCheckDirection.z).magnitude;

            var hasHitWall = Physics.Raycast(wallCheckRay, wallCheckDistance + 0.01f, _slopeCheckMask,
                QueryTriggerInteraction.Ignore);

            // if we did hit a wall then don't adjust slope movement
            if (hasHitWall) 
                return movementVector;

            var groundPoint = Controller.Ground.Point;
            var slopeDirection = (slopeHit.point - groundPoint).normalized;

            // we get the angle of the slope direction vector
            var slopeAngle = Mathf.Abs(Vector3.Angle(Vector3.up, slopeDirection) - 90f);
            // we determine if this is a valid slope (can the character move along it)
            var isValidSlope = (slopeAngle >= 0f && slopeAngle <= _maxSlopeAngle) || slopeAngle.Approximately(0f);

            return isValidSlope ? slopeDirection * movementVector.magnitude : movementVector;
        }

        private Vector3 GetForwardVector()
        {
            var forwardVector = Controller.transform.forward;

            if (IsMoving)
            {
                var inputVector = Controller.Input.Player.Movement.Value;
                var cameraTransform = _camera.transform;

                forwardVector = cameraTransform.TransformDirection(new Vector3(inputVector.x, 0f, 0f)); ;
            }

            forwardVector.y = 0f;
            forwardVector.Normalize();

            return forwardVector;
        }

        private void ApplyMovement(Vector3 groundVelocity, float deltaTime)
        {
            var targetSpeed = IsSprinting ? _sprintSpeed : _speed;
            
            var movementVelocity = MovementVector * targetSpeed;
            var acceleration = groundVelocity.magnitude / deltaTime;

            var movementDot = Vector3.Dot(Controller.Velocity.normalized, MovementVector);

            if (movementDot >= 0f)
                acceleration += _acceleration;
            else
                acceleration += _decceleration;

            _currentVelocity = CalculateVelocity(_currentVelocity, movementVelocity + groundVelocity, acceleration, deltaTime);

            _currentVelocity.y = Mathf.Lerp(Controller.Velocity.y, _currentVelocity.y, Mathf.Abs(MovementVector.y));
            //if (IsJumping)
            //    _currentVelocity.y = Controller.Velocity.y;

            var targetAcceleration = (_currentVelocity - Controller.Velocity) / deltaTime;
            targetAcceleration = Vector3.ClampMagnitude(targetAcceleration, _maxAcceleration);

            Controller.RigidBody.AddForce(targetAcceleration * Controller.RigidBody.mass);
        }

        private void ApplyRotation(float deltaTime)
        {
            ForwardVector = GetForwardVector();
            UpVector = Vector3.up;

            ForwardVector = MathExtensions.ProjectVectorOnPlane(ForwardVector, UpVector);

            var targetRotation = Quaternion.LookRotation(ForwardVector, UpVector);

            var rotationSpeed = _rotationSpeed;

            if (!IsMoving)
            {
                var groundAngularVelocity = Controller.Ground.AngularVelocity * Mathf.Rad2Deg * deltaTime;

                groundAngularVelocity.Scale(UpVector);

                targetRotation = Quaternion.Euler(groundAngularVelocity) * targetRotation;
                rotationSpeed += groundAngularVelocity.magnitude;
            }

            _currentRotation = Quaternion.RotateTowards(_currentRotation, targetRotation, rotationSpeed * deltaTime);

            var rotationToTarget = MathExtensions.ShortestRotation(_currentRotation, Controller.Rotation);

            rotationToTarget.ToAngleAxis(out var rotationAngle, out var rotationAxis);
            rotationAxis.Normalize();
            rotationAngle *= Mathf.Deg2Rad;

            var targetTorque = rotationAxis * (rotationAngle * _rotationSpringStrength) - (Controller.AngularVelocity * _rotationSpringDamper);

            Controller.RigidBody.AddTorque(targetTorque);
        }

        private Vector3 CalculateVelocity(Vector3 currentVelocity, Vector3 movementVelocity, float acceleration, float deltaTime)
        {
            var upDot = Mathf.Abs(Vector3.Dot(movementVelocity, Vector3.up));
            if (upDot < 0.05f)
                currentVelocity.y = 0f;

            currentVelocity = Vector3.MoveTowards(currentVelocity, movementVelocity, acceleration * deltaTime);

            return currentVelocity;
        }
    }
}
