using Assets.Scripts.Utilities;
using System;
using UnityEngine;

namespace Assets.Scripts.Player.Actions
{
    [Serializable]
    public class DashAction : IAction
    {
        [SerializeField]
        private float _groundDashForce = 50f;
        [SerializeField]
        private float _airDashForce = 20f;
        [SerializeField]
        private float _cooldownTime = 0.5f;

        private Timer _cooldownTimer;

        public PlayerController Player { get; private set; }
        private bool IsGrounded => Player.Tags.IsTagActive(TagType.Grounded);
        private bool IsDashing
        {
            get => Player.Tags.IsTagActive(TagType.Dashing);
            set => Player.Tags.SetTag(TagType.Dashing, value);
        }

        public void Initialize(PlayerController player)
        {
            Player = player;

            Player.Input.Player.Dash.Performed += Perform;
            Player.Input.Player.Dash.Canceled += Cancel;

            _cooldownTimer.SetDuration(_cooldownTime);
        }

        public void Dispose()
        {
            
        }

        public void Reset()
        {
            
        }

        public void Perform()
        {
            if (!CanPerform())
                return;

            var dashForce = IsGrounded ? _groundDashForce : _airDashForce;

            var dashDirection = GetDashDirection() * dashForce;

            var dashVelocityChange = dashDirection - Player.Velocity;

            Player.RigidBody.AddForce(dashVelocityChange, ForceMode.VelocityChange);
            //Player.AnimationController.Dash(dashDirection);

            IsDashing = true;

            _cooldownTimer.SetDuration(_cooldownTime);
            _cooldownTimer.Restart();
        }

        public void Cancel()
        {
            
        }

        public void Update(float dt)
        {
            if (!IsDashing)
                return;

            _cooldownTimer.Tick(dt);

            if (!_cooldownTimer.IsRunning)
                IsDashing = false;
        }

        public bool CanPerform()
        {
            return !_cooldownTimer.IsRunning;
        }

        private Vector3 GetDashDirection()
        {
            var movementState = Player.CurrentState;
            var dashDirection = movementState != null ? movementState.MovementVector : -Player.transform.forward;

            if (dashDirection.Approximately(Vector3.zero, 0.01f))
                dashDirection = -Player.transform.forward;

            return dashDirection;
        }

    }
}
