using Assets.Scripts.Interfaces;
using Assets.Scripts.Player;
using Trisibo;
using UnityEngine;

namespace Assets.Scripts.Core
{
    public class LevelDoor : MonoBehaviour, IInteractable
    {
        private const float MIN_INTERACTION_DISTANCE = 2f;

        [SerializeField]
        private SceneField _nextLevel;
        [SerializeField]
        private int _locationId;
        [SerializeField]
        private Transform _spawnPoint;

        public SceneField NextLevel => _nextLevel;
        public int LocationId => _locationId;
        public Vector3 SpawnPoint => _spawnPoint ? _spawnPoint.position : transform.position;

        private void OnEnable()
        {
            GameManager.Instance.LevelManager.RegisterDoor(this);
        }

        private void OnDisable()
        {
            GameManager.Instance.LevelManager.UnregisterDoor(this);
        }

        public bool CanInteract(PlayerController player, out string response)
        {
            response = null;

            var distanceToPlayer = Vector3.Distance(player.Position, transform.position);
            if (distanceToPlayer > MIN_INTERACTION_DISTANCE)
            {
                response = "The Player is not within interaction distance.";
                return false;
            }
            
            return true;
        }

        public void Interact(PlayerController player)
        {
            GameManager.Instance.LevelManager.LoadNextLevel(_nextLevel, _locationId);
        }
    }
}
