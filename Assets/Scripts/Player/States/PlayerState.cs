using UnityEngine;

namespace Assets.Scripts.Player.States
{
    public abstract class PlayerState
    {
        public PlayerController Controller { get; set; }
        public Vector3 MovementVector { get; protected set; }
        public Vector3 ForwardVector { get; protected set; }
        public Vector3 UpVector { get; protected set; }

        public abstract void Start();
        public abstract void End();
        public abstract void Update(float dt);
    }
}
