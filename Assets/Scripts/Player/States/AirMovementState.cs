using Assets.Scripts.Utilities;
using System;
using UnityEngine;

namespace Assets.Scripts.Player.States
{
    [Serializable]
    public class AirMovementState : PlayerState
    {
        [Header("Movement")]
        [SerializeField]
        private float _speed = 6f;
        [SerializeField]
        private float _acceleration = 20f;
        [SerializeField]
        private float _decceleration = 10f;
        [SerializeField]
        private float _maxAcceleration = 100f;
        [SerializeField]
        private float _drag = 1f;
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

            ApplyMovement(dt);

            ApplyRotation(dt);
        }

        private Vector3 GetMovementVector()
        {
            var inputVector = Controller.Input.Player.Movement.Value;
            var cameraTransform = _camera.transform;

            var movementVector = cameraTransform.TransformDirection(new Vector3(inputVector.x, 0f, 0f));
            movementVector = movementVector.normalized * inputVector.magnitude;

            return movementVector;
        }

        private Vector3 GetForwardVector()
        {
            var forwardVector = Controller.transform.forward;

            if (IsMoving)
            {
                forwardVector = MovementVector;
            }

            forwardVector.y = 0f;
            forwardVector.Normalize();

            return forwardVector;
        }

        private void ApplyMovement(float deltaTime)
        {
            var velDot = Vector3.Dot(Controller.Velocity.normalized, MovementVector);

            var acceleration = 0f;
            if (IsMoving)
                acceleration = _acceleration;

            var movementVelocity = MovementVector * _speed;

            _currentVelocity = CalculateVelocity(Controller.Velocity, movementVelocity, acceleration, deltaTime);            

            var targetAcceleration = (_currentVelocity - Controller.Velocity) / deltaTime;
            targetAcceleration.y = 0f; // ignore y acceleration cuz gravity will handle that part for us
            targetAcceleration = Vector3.ClampMagnitude(targetAcceleration, _maxAcceleration);

            var targetForce = targetAcceleration * Controller.RigidBody.mass;
            Controller.RigidBody.AddForce(targetForce);

            var velocityXZ = new Vector3(Controller.Velocity.x, 0f, Controller.Velocity.z);
            var speedFactor = Mathf.Max((velocityXZ.magnitude / _speed) - 1f, 0f);
            Controller.RigidBody.AddForce(-velocityXZ * _drag * speedFactor);
        }

        private void ApplyRotation(float deltaTime)
        {
            ForwardVector = GetForwardVector();
            UpVector = Vector3.up;

            var targetRotation = Quaternion.LookRotation(ForwardVector, UpVector);         

            _currentRotation = Quaternion.RotateTowards(_currentRotation, targetRotation, _rotationSpeed * deltaTime);

            var rotationToTarget = MathExtensions.ShortestRotation(_currentRotation, Controller.Rotation);

            rotationToTarget.ToAngleAxis(out var rotationAngle, out var rotationAxis);
            rotationAxis.Normalize();
            rotationAngle *= Mathf.Deg2Rad;

            var targetTorque = rotationAxis * (rotationAngle * _rotationSpringStrength) - (Controller.RigidBody.angularVelocity * _rotationSpringDamper);

            Controller.RigidBody.AddTorque(targetTorque);
        }

        private Vector3 CalculateVelocity(Vector3 currentVelocity, Vector3 movementVelocity, float acceleration, float deltaTime)
        {
            currentVelocity = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
            var currentSpeedSqr = currentVelocity.sqrMagnitude;

            if (currentSpeedSqr > movementVelocity.sqrMagnitude)
                movementVelocity = movementVelocity.normalized * Mathf.Sqrt(currentSpeedSqr);

            currentVelocity = Vector3.MoveTowards(currentVelocity, movementVelocity, acceleration * deltaTime);

            return currentVelocity;
        }
    }
}
