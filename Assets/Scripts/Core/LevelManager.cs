using Assets.Scripts.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trisibo;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Core
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField]
        private SceneField _defaultLevel;

        private List<LevelDoor> _doors = new List<LevelDoor>(5);
        private SceneField _currentLevel;
        private SceneField _nextLevel;
        private int _nextLocationId;

        private void Start()
        {
            SceneManager.LoadScene(_defaultLevel.BuildIndex, LoadSceneMode.Single);

            _currentLevel = _defaultLevel;
        }

        private void OnFadeInComplete()
        {
            Debug.Log("Fade in complete");
            
            var asyncOp = SceneManager.LoadSceneAsync(_nextLevel.BuildIndex, LoadSceneMode.Single);

            if (asyncOp != null)
                OnLoadNextLevel(asyncOp);
            else
                asyncOp.completed += OnLoadNextLevel;
        }

        private void OnFadeOutComplete()
        {
            Debug.Log("Fade out complete");
            var player = GameManager.Instance.Player;
            player.Input.Player.Enable();
        }

        private void OnLoadNextLevel(AsyncOperation operation)
        {
            Debug.Log("Next level loaded");

            LevelDoor targetDoor = null;
            for (var i = 0; i< _doors.Count; i++)
            {
                var door = _doors[i];

                if (door.NextLevel.BuildIndex == _nextLevel.BuildIndex && door.LocationId == _nextLocationId)
                {
                    targetDoor = door;
                    break;
                }
            }

            if (targetDoor == null)
            {
                Debug.LogError("There is no door that links to the previous level in this level.");
            }
            else
            {
                // position player at the spawn location of this door
                var player = GameManager.Instance.Player;
                player.Position = targetDoor.SpawnPoint;
                player.CameraController.PositionToTarget();
            }

            _currentLevel = _nextLevel;
            _nextLevel = null;
            _nextLocationId = 0;

            CanvasController.Instance.FadeInOut.FadeOut(OnFadeOutComplete);
        }

        public void LoadNextLevel(SceneField nextScene, int locationId)
        {
            if (_nextLevel != null)
                return;

            _nextLevel = nextScene;
            _nextLocationId = locationId;

            var player = GameManager.Instance.Player;
            player.Input.Player.Disable();

            CanvasController.Instance.FadeInOut.FadeIn(OnFadeInComplete);
        }

        public void RegisterDoor(LevelDoor door)
        {
            Debug.Log("Door registered");

            if (_doors.Contains(door))
                return;

            _doors.Add(door);
        }

        public void UnregisterDoor(LevelDoor door)
        {
            Debug.Log("Door unregistered");
            if (!_doors.Contains(door))
                return;

            _doors.Remove(door);
        }
    }
}
