using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Player
{
    public class PlayerRespawnPoint : MonoBehaviour
    {
        private bool _respawnPlayer;

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
            var gameState = GameManager.Instance.CurrentState;

            if (gameState == GameManager.GameStates.GameOver)
                _respawnPlayer = true;
            else if (gameState == GameManager.GameStates.PreLevel && _respawnPlayer)
            {
                var player = GameManager.Instance.Player;

                player.Position = transform.position;
                player.CameraController.PositionToTarget();

                _respawnPlayer = false;
            }
        }
    }
}
