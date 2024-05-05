using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trisibo;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField]
        private SceneField _playLevel;

        private void Awake()
        {
            GameManager.GameStateChanged += OnGameStateChanged;
        }

        private void OnDestroy()
        {
            GameManager.GameStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged()
        {
            var currentState = GameManager.Instance.CurrentState;

            gameObject.SetActive(currentState == GameManager.GameStates.MainMenu);
        }

        public void Play()
        {
            GameManager.Instance.LevelManager.LoadNextLevel(_playLevel);
        }
    }
}
