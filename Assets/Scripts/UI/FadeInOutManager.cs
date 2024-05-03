using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

delegate void UpdateDelegate(float dt);

namespace Assets.Scripts.UI
{
    public class FadeInOutManager : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup _fadeGroup;
        [SerializeField]
        private float _fadeInDuration = 1f;
        [SerializeField]
        private float _fadeOutDuration = 1f;

        private float _fadeTime;
        private UpdateDelegate _fadeUpdate;
        private Action _onComplete; 

        private void Start()
        {
            _fadeGroup.alpha = 0f;
            _fadeGroup.blocksRaycasts = false;
        }

        private void Update()
        {
            var dt = Time.deltaTime;

            _fadeUpdate?.Invoke(dt);
        }

        private void FadeInUpdate(float dt)
        {
            _fadeTime += dt;

            var t = Mathf.SmoothStep(0f, 1f, _fadeTime / _fadeInDuration);

            _fadeGroup.alpha = t;

            if (t >= 1f)
            {
                _fadeUpdate = null;
                _onComplete?.Invoke();
            }
        }

        private void FadeOutUpdate(float dt)
        {
            _fadeTime += dt;

            var t = 1f - Mathf.SmoothStep(0f, 1f, _fadeTime / _fadeOutDuration);

            _fadeGroup.alpha = t;

            if (t <= 0f)
            {
                _fadeUpdate = null;
                _onComplete?.Invoke();
            }
        }

        public void FadeIn(Action onComplete = null)
        {
            if (_fadeUpdate != null)
            {
                Debug.LogWarning("Fade action already in progress.");
                return;
            }

            _fadeTime = 0f;
            _fadeGroup.blocksRaycasts = true;
            _onComplete = onComplete;
            _fadeUpdate = FadeInUpdate;
        }

        public void FadeOut(Action onComplete = null)
        {
            if (_fadeUpdate != null)
            {
                Debug.LogWarning("Fade action already in progress.");
                return;
            }

            _fadeTime = 0f;
            _fadeGroup.blocksRaycasts = false;
            _onComplete = onComplete;
            _fadeUpdate = FadeOutUpdate;
        }
    }
}
