using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Player
{
    public class PlayerAnimationController : MonoBehaviour
    {
        [SerializeField]
        private PlayerController _player;
        [SerializeField]
        private Animator _animator;

        private int _speedHash;
        private int _jumpHash;
        private int _groundedHash;
        private int _fallingHash;

        private void Awake()
        {
            _speedHash = Animator.StringToHash("Speed");
            _jumpHash = Animator.StringToHash("Jump");
            _groundedHash = Animator.StringToHash("Grounded");
            _fallingHash = Animator.StringToHash("Falling");
        }

        private void Start()
        {
            _player.Tags.AddTagChangedCallback(TagType.Jumping, OnJumpChanged);
            _player.Tags.AddTagChangedCallback(TagType.Grounded, OnGroundedChanged);
            _player.Tags.AddTagChangedCallback(TagType.Falling, OnFallingChanged);
        }

        private void OnJumpChanged(TagData data)
        {
            _animator.SetBool(_jumpHash, data.IsActive);
        }

        private void OnGroundedChanged(TagData data)
        {
            _animator.SetBool(_groundedHash, data.IsActive);
        }

        private void OnFallingChanged(TagData data)
        {
            _animator.SetBool(_fallingHash, data.IsActive);
        }

        private void Update()
        {
            var currentSpeed = _player.Velocity.magnitude;

            _animator.SetFloat(_speedHash, currentSpeed);
        }

        public void OnFootstep()
        {

        }

        public void OnLand()
        {

        }
    }
}
