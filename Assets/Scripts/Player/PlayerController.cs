using Assets.Scripts.Input;
using Assets.Scripts.Interfaces;
using Assets.Scripts.InventorySystem;
using Assets.Scripts.Player.Actions;
using Assets.Scripts.Player.States;
using Assets.Scripts.UI;
using Assets.Scripts.Utilities;
using Assets.Scripts.Utilities.Comparers;
using System;
using UnityEngine;

public delegate void CollisionDelegate(Collision col, CollisionEventType interaction);
public delegate void TriggerDelegate(Collider col, CollisionEventType interaction);

public enum CollisionEventType
{
    Enter,
    Stay,
    Exit
}

namespace Assets.Scripts.Player
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class PlayerController : MonoBehaviour, IDestructible
    {
        private const float INVULNERABILITY_ON_DAMAGE_DURATION = 1f;

        [Header("General")]
        [SerializeField]
        private Rigidbody _rigidbody;
        [SerializeField]
        private CapsuleCollider _collider;
        [SerializeField]
        private PlayerInput _input;
        [SerializeField]
        private PlayerCameraController _cameraController;
        [SerializeField]
        private Inventory _inventory;
        [SerializeField]
        private Vector3 _gravity = Vector3.down * 9.81f;

        [Header("Attributes")]
        [SerializeField]
        private int _maxHealth = 10;

        [Space(10), SerializeField]
        private ActionContainer _actionContainer;

        [Header("States")]
        [SerializeField]
        private GroundMovementState _groundMovementState;
        [SerializeField]
        private AirMovementState _airMovementState;

        [Header("Ground Offset")]
        [SerializeField]
        private float _offset = 0.15f;
        [SerializeField]
        private float _spring = 1000f;
        [SerializeField]
        private float _damp = 200f;
        [SerializeField]
        private GroundEvaluator _groundEvaluator;

        private GameplayTags _tags = new GameplayTags();
        private Collider[] _overlappingColliders = new Collider[10];
        private IInteractable _interactionTarget;
        private int _health;
        private float _invulnerableTime;

        public Rigidbody RigidBody => _rigidbody;
        public CapsuleCollider Collider => _collider;
        public PlayerInput Input => _input;
        public PlayerCameraController CameraController => _cameraController;
        public Inventory Inventory => _inventory;
        public GameplayTags Tags => _tags;
        public Vector3 Position
        {
            get => transform.position;
            set
            {
                transform.position = value;
                if (_rigidbody)
                    _rigidbody.position = value;
            }
        }
        public Quaternion Rotation
        {
            get => transform.rotation;
            set
            {
                transform.rotation = value;
                if (_rigidbody)
                    _rigidbody.rotation = value;
            }
        }
        public Vector3 Velocity
        {
            get => _rigidbody ? _rigidbody.velocity : Vector3.zero;
            set
            {
                if (_rigidbody)
                    _rigidbody.velocity = value;
            }
        }
        public Vector3 AngularVelocity
        {
            get => _rigidbody ? _rigidbody.angularVelocity : Vector3.zero;
            set
            {
                if (_rigidbody)
                    _rigidbody.angularVelocity = value;
            }
        }
        public Vector3 Gravity => _gravity;
        public Vector3 Centroid
        {
            get => transform.localToWorldMatrix.MultiplyPoint(_collider.center);
        }
        public float Height
        {
            get => _collider.height;
            set => _collider.height = value;
        }
        public GroundData Ground => _groundEvaluator.Data;
        public PlayerState CurrentState { get; private set; }
        public int Health => _health;
        public int MaxHealth => _maxHealth;
        public bool IsInvulnerable
        {
            get => _tags.IsTagActive(TagType.Invulnerable);
            private set
            {
                if (value == IsInvulnerable)
                    return;

                _tags.SetTag(TagType.Invulnerable, value);

                if (value)
                    _invulnerableTime = INVULNERABILITY_ON_DAMAGE_DURATION;
            }
        }
        public bool IsDestroyed => _health <= 0;
        public bool IsGrounded
        {
            get => _tags.IsTagActive(TagType.Grounded);
            private set => _tags.SetTag(TagType.Grounded, value);
        }
        public bool IsFalling
        {
            get => _tags.IsTagActive(TagType.Falling);
            private set => _tags.SetTag(TagType.Falling, value);
        }


        public event Action<int> HealthChanged;
        public event CollisionDelegate Collision;
        public event DestructionEvent Destroyed;

        private void OnValidate()
        {
            if (!_rigidbody)
                _rigidbody = GetComponent<Rigidbody>();

            if (!_collider)
                _collider = GetComponent<CapsuleCollider>();

            if (!_input)
                _input = GetComponent<PlayerInput>();
        }

        private void Awake()
        {
            _health = _maxHealth;
        }

        private void Start()
        {
            _rigidbody.useGravity = false;

            _groundEvaluator.Initialize(this);

            var actions = _actionContainer.Actions;
            for(var i = 0; i < actions.Count; i++)
                actions[i].Initialize(this);

            _input.Player.Interact.Performed += OnInteract;

            _tags.AddTagChangedCallback(TagType.Grounded, OnGroundedChanged);

            SetState(_groundMovementState);
        }

        private void OnInteract()
        {
            _interactionTarget?.Interact(this);
        }

        private void OnGroundedChanged(TagData data)
        {
            SetDefaultState();
        }

        private void FixedUpdate()
        {
            var dt = Time.fixedDeltaTime;

            UpdateInteractionTarget();

            // ground check & offset
            IsGrounded = _groundEvaluator.Evaluate() && !_tags.IsTagActive(TagType.Jumping);

            IsFalling = !IsGrounded && Velocity.y < -0.1f && Ground.Distance > _groundEvaluator.GroundedDistance;

            // actions
            var actions = _actionContainer.Actions;
            for (var i = 0; i < actions.Count; i++)
                actions[i].Update(dt);

            ApplyGroundOffset();

            ApplyGravity();

            CurrentState?.Update(dt);
        }

        private void Update()
        {
            if (_invulnerableTime <= 0f)
                return;

            var dt = Time.deltaTime;

            _invulnerableTime -= dt;

            if (_invulnerableTime <= 0f)
                IsInvulnerable = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            Collision?.Invoke(collision, CollisionEventType.Enter);
        }

        private void OnCollisionStay(Collision collision)
        {
            Collision?.Invoke(collision, CollisionEventType.Stay);
        }

        private void OnCollisionExit(Collision collision)
        {
            Collision?.Invoke(collision, CollisionEventType.Exit);
        }

        private void ApplyGravity()
        {
            _rigidbody.AddForce(_gravity, ForceMode.Acceleration);
        }

        private void ApplyGroundOffset()
        {
            if (!IsGrounded)
                return;

            var groundVelocity = Ground.GetPointVelocity();
            var groundDistance = Ground.Distance;
            var groundDelta = groundDistance - (Height * 0.5f + _offset);

            var controllerRelativeVel = Vector3.Dot(Velocity, Vector3.up);
            var groundRelativeVel = Vector3.Dot(groundVelocity, Vector3.up);

            var relVel = controllerRelativeVel - groundRelativeVel;

            var springForce = (-groundDelta * _spring) - (relVel * _damp);

            RigidBody.AddForce(springForce * Vector3.up);
        }

        private void UpdateInteractionTarget()
        {
            var count = Physics.OverlapSphereNonAlloc(Position, 2f, _overlappingColliders);

            _interactionTarget = null;

            if (count <= 0)
                return;

            Array.Sort(_overlappingColliders, new DistanceToPointComparer(Position));

            for (var i = 0; i < count; i++)
            {
                var otherCollider = _overlappingColliders[i];

                if (otherCollider == _collider)
                    continue;

                if (otherCollider.TryGetComponentOnCollider(out _interactionTarget))
                {
                    break;
                }
            }
        }

        public void ResetObject()
        {
            _health = _maxHealth;
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }

        public void AddAction(IAction action)
        {
            _actionContainer.AddAction(action);

            action.Initialize(this);
        }

        public void RemoveAction(IAction action)
        {
            _actionContainer.RemoveAction(action);

            action.Dispose();
        }

        public void SetState(PlayerState nextState)
        {
            if (nextState == null)
                return;

            if (nextState == CurrentState)
                return;

            nextState.Controller = this;

            CurrentState?.End();
            CurrentState = nextState;
            CurrentState.Start();
        }

        public void SetDefaultState()
        {
            if (IsGrounded)
                SetState(_groundMovementState);
            else
                SetState(_airMovementState);
        }

        public void Heal(int healAmount)
        {
            _health = Mathf.Min(_health + healAmount, _maxHealth);

            HealthChanged?.Invoke(_health);
        }

        public void DealDamage(int damage, GameObject damageSource = null, Action onDestroyed = null)
        {
            if (IsDestroyed || IsInvulnerable)
                return;

            _health = Mathf.Max(_health - damage, 0);

            HealthChanged?.Invoke(_health);

            if (_health <= 0)
            {
                onDestroyed?.Invoke();

                Destroyed?.Invoke(this);
            }
            else
                IsInvulnerable = true;
        }

        public void Destroy()
        {
            if (IsDestroyed)
                return;

            _health = 0;

            HealthChanged?.Invoke(_health);

            Destroyed?.Invoke(this);
        }
    }
}