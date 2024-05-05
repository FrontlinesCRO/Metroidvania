using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class CanvasController : MonoBehaviour
    {
        [SerializeField]
        private PlayerUI _playerUI;
        [SerializeField]
        private FadeInOutManager _fadeInOut;

        private GameManager _gameManager;

        public PlayerUI PlayerUI => _playerUI;
        public FadeInOutManager FadeInOut => _fadeInOut;

        public void Initialize(GameManager gameManager)
        {
            _gameManager = gameManager;

            _playerUI.Initialize(gameManager.Player);
        }

    }
}
