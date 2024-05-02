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
        private FadeInOutManager _fadeInOut;
        [SerializeField]
        private InventoryRenderer _inventoryRenderer;

        public FadeInOutManager FadeInOut => _fadeInOut;
        public InventoryRenderer InventoryRenderer => _inventoryRenderer;

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
    }
}
