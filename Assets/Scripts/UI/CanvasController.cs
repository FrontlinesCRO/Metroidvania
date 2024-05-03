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
        private static CanvasController s_instance;
        public static CanvasController Instance => s_instance;

        [SerializeField]
        private PlayerUI _playerUI;
        [SerializeField]
        private FadeInOutManager _fadeInOut;

        public PlayerUI PlayerUI => _playerUI;
        public FadeInOutManager FadeInOut => _fadeInOut;

        private void Awake()
        {
            if (s_instance)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);

            s_instance = this;
        }

        private void Start()
        {
            var player = GameManager.Instance.Player;

            _playerUI.Initialize(player);
        }
    }
}
