using System;
using Assets.Scripts.Utilities;
using UnityEngine;

namespace Assets.Scripts.Player.Actions
{
    [Serializable]
    public class JumpAction : IAction
    {
        private const float GROUNDED_JUMP_TIME_BUFFER = 0.1f;
        private const float PERFORM_JUMP_TIME_BUFFER = 0.2f;

        [SerializeField]
        private float _jumpVelocity = 16f;

        private float _jumpTime = -1f;
        private float _jumpPerformTime = -1f;

        public PlayerController Player { get; private set; }
        private bool IsGrounded => Player.Tags.IsTagActive(TagType.Grounded);
        private bool IsFalling => Player.Tags.IsTagActive(TagType.Falling);
        private bool IsJumping
        {
            get => Player.Tags.IsTagActive(TagType.Jumping);
            set => Player.Tags.SetTag(TagType.Jumping, value);
        }

        public void Initialize(PlayerController player)
        {
            Player = player;

            Player.Input.Player.Jump.Performed += Perform;
            Player.Input.Player.Jump.Canceled += Cancel;

            Player.Collision += OnCollision;
        }

        public void Dispose()
        {
            Player.Collision -= OnCollision;
        }

        public void Reset()
        {
            
        }

        public void Perform()
        {
            if (CanPerform())
                Jump();
            else
                _jumpPerformTime = PERFORM_JUMP_TIME_BUFFER;
        }

        public void Cancel()
        {
            
        }

        public void Update(float dt)
        {
            if (!IsJumping)
            {
                if (_jumpPerformTime < 0f)
                    return;
                else if (CanPerform())
                    Jump();
                else
                    _jumpPerformTime -= dt;

                return;
            }

            _jumpTime -= dt;

            // apply some offsetting upwards without adding force so that the height of the jump is consistent
            var groundYVelocity = Player.Ground.Velocity.y;
            groundYVelocity = groundYVelocity > 0f ? groundYVelocity : 0f;

            var delta = groundYVelocity * dt;
            Player.RigidBody.MovePosition(Player.Position + delta * Vector3.up);

            IsJumping = !IsGrounded && !IsFalling && _jumpTime > 0f;
        }

        public bool CanPerform()
        {
            if (IsJumping)
                return false;

            // query the status manage of the controller for grounded status data
            var groundedData = Player.Tags.GetTag(TagType.Grounded);

            if (!groundedData.IsActive)
            {
                var groundedEndedTimeDelta = Time.timeSinceLevelLoad - groundedData.Time;

                var withinJumpTimeBuffer = groundedEndedTimeDelta < GROUNDED_JUMP_TIME_BUFFER;

                return withinJumpTimeBuffer;
            }

            return true;
        }

        private void Jump()
        {
            var playerVelocity = Player.RigidBody.velocity;
            var groundVelocity = Player.Ground.GetPointVelocity();
            var currentState = Player.CurrentState;

            var movementVector = currentState != null ? currentState.MovementVector : Vector3.zero;
            var groundVelDot = Vector3.Dot(movementVector, groundVelocity.NormalizeXZ());

            var jumpVelocity = playerVelocity;
            jumpVelocity.y = _jumpVelocity;

            if (groundVelDot < 0f)
            {
                jumpVelocity.x += groundVelocity.x * groundVelDot;
                jumpVelocity.z += groundVelocity.z * groundVelDot;
            }

            // clear any y forces added onto the controller during this simulation step
            var currentForceY = Player.RigidBody.GetAccumulatedForce().y;
            Player.RigidBody.AddForce(-currentForceY * Vector3.up);
            Player.RigidBody.AddForce(jumpVelocity - playerVelocity, ForceMode.VelocityChange);

            _jumpTime = _jumpVelocity / Player.Gravity.magnitude;
            _jumpPerformTime = -1f;
            
            IsJumping = true;
        }

        private void OnCollision(Collision col, CollisionEventType collisionType)
        {
            if (!IsJumping || collisionType == CollisionEventType.Exit)
                return;

            var contactCount = col.contactCount;

            for (var i = 0; i < contactCount; i++)
            {
                var contact = col.GetContact(i);

                var directionToPoint = contact.point - Player.Position;

                var dot = Vector3.Dot(directionToPoint.normalized, Vector3.up);

                if (dot >= 0.95f)
                {
                    IsJumping = false;
                    return;
                }
            }
        }
    }
}
