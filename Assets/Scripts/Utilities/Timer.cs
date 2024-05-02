using System;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    /// <summary>
    /// A countdown timer utility.
    /// </summary>
    public struct Timer
    {
        public float Time { get; private set; }
        public float NormalizedTime => Mathf.Clamp01(1f - (Time / Duration));
        public float Duration { get; private set; }
        public bool IsRunning { get; private set; }

        private bool _started;
        private Action _onStart;
        private Action _onEnd;

        public Timer(float duration, Action onStart = null, Action onEnd = null)
        {
            Time = duration;
            Duration = duration;
            IsRunning = false;

            _started = false;
            _onStart = onStart;
            _onEnd = onEnd;
        }

        public Timer SetTime(float time)
        {
            Time = Mathf.Clamp(time, 0f, Duration);
            return this;
        }

        public Timer SetDuration(float duration)
        {
            Duration = duration;
            return this;
        }

        public Timer AssingStart(Action onStart)
        {
            _onStart = onStart;
            return this;
        }

        public Timer AssignEnd(Action onEnd)
        {
            _onEnd = onEnd;
            return this;
        }

        public void Tick(float dt)
        {
            if (!IsRunning)
                return;

            Time -= dt;

            if (Time <= 0f)
                End();    
        }

        public void Reset()
        {
            Time = Duration;
            IsRunning = false;
            _started = false;
        }

        public void Restart()
        {
            Reset();
            Start();
        }

        public void Start()
        {
            IsRunning = true;

            if (_started)
                return;

            _started = true;
            _onStart?.Invoke();
        }

        public void Stop()
        {
            IsRunning = false;
        }

        private void End()
        {
            IsRunning = false;
            _onEnd?.Invoke();
        }
    }
}
