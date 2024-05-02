using System;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Input
{
    [Serializable]
    public class PlayerInputActions : MetroidvaniaInputActions.IPlayerActions
    {
        private MetroidvaniaInputActions _inputActions;

        public Axis2DInputAction Movement;
        public Axis2DInputAction Aim;
        public ButtonInputAction Jump;
        public ButtonInputAction Sprint;
        public ButtonInputAction Dash;
        public ButtonInputAction Shoot;
        public ButtonInputAction Guard;
        public ButtonInputAction Interact;
        public ButtonInputAction Inventory;

        public void Initialize(MetroidvaniaInputActions inputActions)
        {
            _inputActions = inputActions;
            _inputActions.Player.AddCallbacks(this);

            Enable();
        }

        public void Dispose()
        {
            Movement.Dispose();
            Aim.Dispose();
            Jump.Dispose();
            Sprint.Dispose();
            Dash.Dispose();
            Shoot.Dispose();
            Guard.Dispose();
            Interact.Dispose();
            Inventory.Dispose();

            _inputActions?.Player.RemoveCallbacks(this);
        }

        public void Enable()
        {
            Movement.Enable();
            Aim.Enable();
            Jump.Enable();
            Sprint.Enable();
            Dash.Enable();
            Shoot.Enable();
            Guard.Enable();
            Interact.Enable();
            Inventory.Enable();

            _inputActions.Player.Enable();
        }

        public void Disable()
        {
            Movement.Disable();
            Aim.Disable();
            Jump.Disable();
            Sprint.Disable();
            Dash.Disable();
            Shoot.Disable();
            Guard.Disable();
            Interact.Disable();
            Inventory.Disable();

            _inputActions.Player.Disable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            Movement.Update(context);
        }

        public void OnAim(InputAction.CallbackContext context)
        {
            Aim.Update(context);
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            Jump.Update(context);
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            Sprint.Update(context);
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            Dash.Update(context);
        }

        public void OnShoot(InputAction.CallbackContext context)
        {
            Shoot.Update(context);
        }

        public void OnGuard(InputAction.CallbackContext context)
        {
            Guard.Update(context);
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            Interact.Update(context);
        }

        public void OnInventory(InputAction.CallbackContext context)
        {
            Inventory.Update(context);
        }
    }
}
