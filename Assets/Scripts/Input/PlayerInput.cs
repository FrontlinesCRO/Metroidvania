using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Input
{
    public class PlayerInput : MonoBehaviour
    {
        [SerializeField]
        private InputActionAsset _actionAsset;

        private MetroidvaniaInputActions _inputActions;

        public PlayerInputActions Player;

        private void Start()
        {
            _inputActions = new MetroidvaniaInputActions(_actionAsset);

            Player.Initialize(_inputActions);
        }

        private void OnDestroy()
        {
            Player.Dispose();
        }
    }

    public struct Axis2DInputAction
    {
        private Vector2 _value;
        private bool _invert;

        public bool Enabled { get; private set; }
        public Vector2 Value => _invert ? -_value : _value;
        public InputActionPhase Phase { get; private set; }

        public event Action Started;
        public event Action<Vector2> Performed;
        public event Action Canceled;

        public void Dispose()
        {
            Started = null;
            Performed = null;
            Canceled = null;
        }

        public void Enable()
        {
            Enabled = true;
        }

        public void Disable()
        {
            Enabled = false;
            Reset();
        }

        public void Reset()
        {
            _value = default;

            if (Phase != InputActionPhase.Canceled)
            {
                Canceled?.Invoke();
                Phase = InputActionPhase.Canceled;
            }
        }

        public void Invert(bool invert)
        {
            _invert = invert;
        }

        public void Update(InputAction.CallbackContext context)
        {
            if (!Enabled)
                return;

            _value = context.ReadValue<Vector2>();
            Phase = context.phase;

            switch (context.phase)
            {
                case InputActionPhase.Started:
                    Started?.Invoke();
                    break;
                case InputActionPhase.Performed:
                    Performed?.Invoke(Value);
                    break;
                case InputActionPhase.Canceled:
                    Canceled?.Invoke();
                    break;
            }
        }
    }

    public struct ButtonInputAction : IDisposable
    {
        private int _enabledFrame;

        public bool Enabled { get; private set; }
        public bool Pressed { get; private set; }
        public InputActionPhase Phase { get; private set; }

        public event Action Started;
        public event Action Performed;
        public event Action Canceled;

        public void Dispose()
        {
            Started = null;
            Performed = null;
            Canceled = null;
        }

        public void Enable()
        {
            _enabledFrame = Time.frameCount;
            Enabled = true;
        }

        public void Disable()
        {
            Enabled = false;
            Reset();
        }

        public void Reset()
        {
            Pressed = false;

            if (Phase != InputActionPhase.Canceled)
            {
                Canceled?.Invoke();
                Phase = InputActionPhase.Canceled;
            }
        }

        public void Update(InputAction.CallbackContext context)
        {
            if (!Enabled || _enabledFrame == Time.frameCount)
                return;

            Pressed = context.ReadValueAsButton();
            Phase = context.phase;

            switch (context.phase)
            {
                case InputActionPhase.Started:
                    Started?.Invoke();
                    break;
                case InputActionPhase.Performed:
                    Performed?.Invoke();
                    break;
                case InputActionPhase.Canceled:
                    Canceled?.Invoke();
                    break;
            }
        }
    }

    public partial class @MetroidvaniaInputActions
    {
        public @MetroidvaniaInputActions(InputActionAsset inputAsset)
        {
            asset = inputAsset;

            // Player
            m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
            m_Player_Move = m_Player.FindAction("Move", throwIfNotFound: true);
            m_Player_Aim = m_Player.FindAction("Aim", throwIfNotFound: true);
            m_Player_Jump = m_Player.FindAction("Jump", throwIfNotFound: true);
            m_Player_Sprint = m_Player.FindAction("Sprint", throwIfNotFound: true);
            m_Player_Dash = m_Player.FindAction("Dash", throwIfNotFound: true);
            m_Player_Shoot = m_Player.FindAction("Shoot", throwIfNotFound: true);
            m_Player_Guard = m_Player.FindAction("Guard", throwIfNotFound: true);
            m_Player_Interact = m_Player.FindAction("Interact", throwIfNotFound: true);
            m_Player_Inventory = m_Player.FindAction("Inventory", throwIfNotFound: true);
        }
    }
}
