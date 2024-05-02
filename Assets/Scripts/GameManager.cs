using Assets.Scripts.Core;
using Assets.Scripts.Player;
using Assets.Scripts.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager s_instance;
        public static GameManager Instance => s_instance;

        [SerializeField]
        private PlayerController _player;
        [SerializeField]
        private LevelManager _levelManager;
        //[SerializeField]
        //private CanvasController _canvasController;

        public PlayerController Player => _player;
        public LevelManager LevelManager => _levelManager;
        //public CanvasController Canvas => _canvasController;

        private void Awake()
        {
            if (s_instance)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(this);

            s_instance = this;
        }
    }
}
