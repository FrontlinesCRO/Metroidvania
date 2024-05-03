using Assets.Scripts.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trisibo;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Core
{
    public class LevelManager : MonoBehaviour
    {
        private List<LevelDoor> _doors = new List<LevelDoor>(5);
        private SceneField _currentLevel;
        private SceneField _nextLevel;
        private int _nextLocationId;

        public event Action NextLevel;
        public event Action NextLevelLoaded;

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene nextScene, LoadSceneMode loadArgs)
        {
            if (_currentLevel == null)
            {
                _currentLevel = _nextLevel;
                _nextLevel = null;
                _nextLocationId = 0;
                return;
            }

            LevelDoor targetDoor = null;
            for (var i = 0; i < _doors.Count; i++)
            {
                var door = _doors[i];

                if (door.NextLevel.BuildIndex == _currentLevel.BuildIndex && door.LocationId == _nextLocationId)
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
        }

        private void OnFadeInComplete()
        {
            var asyncOp = SceneManager.LoadSceneAsync(_nextLevel.BuildIndex, LoadSceneMode.Single);

            if (asyncOp != null)
                OnLoadNextLevel(asyncOp);
            else
                asyncOp.completed += OnLoadNextLevel;
        }

        private void OnFadeOutComplete()
        {
            NextLevelLoaded?.Invoke();
        }

        private void OnLoadNextLevel(AsyncOperation operation)
        {         
            CanvasController.Instance.FadeInOut.FadeOut(OnFadeOutComplete);
        }

        public void LoadNextLevel(SceneField nextScene, int locationId = -1)
        {
            if (_nextLevel != null)
                return;

            _nextLevel = nextScene;
            _nextLocationId = locationId;

            NextLevel?.Invoke();

            CanvasController.Instance.FadeInOut.FadeIn(OnFadeInComplete);
        }

        public void RegisterDoor(LevelDoor door)
        {
            if (_doors.Contains(door))
                return;

            _doors.Add(door);
        }

        public void UnregisterDoor(LevelDoor door)
        {
            if (!_doors.Contains(door))
                return;

            _doors.Remove(door);
        }
    }
}
