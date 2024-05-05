using Assets.Scripts.Core;
using Assets.Scripts.Player;
using Assets.Scripts.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trisibo;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager s_instance;
        public static GameManager Instance => s_instance;

        public enum GameStates
        {
            None        = 0,
            MainMenu    = 1 << 0,
            PreLevel    = 1 << 1,
            Level       = 1 << 2,
            GameOver    = 1 << 3,
            All         = 0xFFFF
        }

        private GameStates _currentState = GameStates.MainMenu;
        public GameStates CurrentState
        {
            get => _currentState;
            private set
            {
                if (_currentState == value)
                    return;

                _currentState = value;

                GameStateChanged?.Invoke();
            }
        }

        [Header("Core Prefabs")]
        [SerializeField]
        private PlayerController _playerPrefab;
        [SerializeField]
        private CanvasController _canvasPrefab;

        [Header("Active References")]
        [SerializeField]
        private PlayerController _player;
        [SerializeField]
        private CanvasController _canvas;
        [SerializeField]
        private LevelManager _levelManager;

        public PlayerController Player => _player;
        public CanvasController Canvas => _canvas;
        public LevelManager LevelManager => _levelManager;

        public static event Action GameStateChanged;

        private void Awake()
        {
            if (s_instance)
            {
                if (_player)
                    Destroy(_player.gameObject);
                if (_canvas)
                    Destroy(_canvas.gameObject);

                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(this);

            s_instance = this;
        }

        private void Start()
        {
            if (!_player)
                _player = Instantiate(_playerPrefab);
            if (!_canvas)
                _canvas = Instantiate(_canvasPrefab);

            DontDestroyOnLoad(_player);
            DontDestroyOnLoad(_canvas);

            _canvas.Initialize(this);
            _levelManager.Initialize();

            _player.Destroyed += OnPlayerDied;
            _levelManager.NextLevel += OnNextLevel;
            _levelManager.NextLevelLoaded += OnNextLevelLoaded;

            CurrentState = SceneManager.GetActiveScene().buildIndex == 0 ? GameStates.MainMenu : GameStates.Level;
        }

        private void OnNextLevel()
        {
            CurrentState = GameStates.PreLevel;

            Player.RigidBody.isKinematic = true;
        }

        private void OnNextLevelLoaded()
        {
            CurrentState = GameStates.Level;

            Player.RigidBody.isKinematic = false;
        }

        private void ResetLevel()
        {
            CurrentState = GameStates.PreLevel;

            Player.ResetObject();
            Player.RigidBody.isKinematic = true;

            GameManager.Instance.Canvas.FadeInOut.FadeOut(FinishReset);
        }

        private void FinishReset()
        {
            CurrentState = GameStates.Level;

            Player.RigidBody.isKinematic = false;
        }

        private void OnPlayerDied(Interfaces.IDestructible destructible)
        {
            CurrentState = GameStates.GameOver;

            GameManager.Instance.Canvas.FadeInOut.FadeIn(ResetLevel);
        }
    }
}
