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
        private int _currentLevelIndex;
        private SceneField _nextLevel;
        private int _nextLocationId;

        public event Action NextLevel;
        public event Action NextLevelLoaded;

        public void Initialize()
        {
            _currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene nextScene, LoadSceneMode loadArgs)
        {
            if (_currentLevelIndex < 0)
            {
                _currentLevelIndex = _nextLevel != null ? _nextLevel.BuildIndex : SceneManager.GetActiveScene().buildIndex;
                _nextLevel = null;
                _nextLocationId = 0;
                return;
            }

            LevelDoor targetDoor = null;
            for (var i = 0; i < _doors.Count; i++)
            {
                var door = _doors[i];

                if (door.NextLevel.BuildIndex == _currentLevelIndex && door.LocationId == _nextLocationId)
                {
                    targetDoor = door;
                    break;
                }
            }

            if (targetDoor == null)
            {
                Debug.LogWarning("There is no door that links to the previous level in this level.");
            }
            else
            {
                // position player at the spawn location of this door
                var player = GameManager.Instance.Player;
                player.Position = targetDoor.SpawnPoint;
                player.CameraController.PositionToTarget();
            }

            _currentLevelIndex = _nextLevel.BuildIndex;
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
            GameManager.Instance.Canvas.FadeInOut.FadeOut(OnFadeOutComplete);
        }

        public void LoadNextLevel(SceneField nextScene, int locationId = -1)
        {
            if (_nextLevel != null)
                return;

            _nextLevel = nextScene;
            _nextLocationId = locationId;

            NextLevel?.Invoke();

            GameManager.Instance.Canvas.FadeInOut.FadeIn(OnFadeInComplete);
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
