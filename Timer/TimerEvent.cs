using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace PKTools.Timer
{
    public enum Counting
    {
        Down,
        Up
    };

    public class TimerEvent
    {
        public string Tag { get; private set; }

        public float Duration { get; private set; }

        public float ElaspedTime { get; private set; }

        public float RemainingTime { get; private set; }

        public bool IsComplete { get; private set; }
        public bool IsPause { get; private set; }
        public bool IsCancel { get; private set; }
        public bool IsActive { get; private set; }

        private bool autoKill { get; set; }
        private Counting counting { get; set; }

        private Action<float> onUpdate;     // pass elapsed time/remaining time
        private Action onPause;
        private Action onResume;
        private Action onComplete;
        private Action onCancel;

        private static TimerManager timerManager;

        private TimerEvent() { }

        public static TimerEvent Create(Counting counting, float duration, string tag = "UNNAMED TIMER")
        {
            // Create Timer Manager if there is not on scene.
            if (timerManager == null)
            {
                TimerManager managerInScene = UnityEngine.Object.FindObjectOfType<TimerManager>();
                if (managerInScene != null)
                {
                    timerManager = managerInScene;
                }
                else
                {
                    GameObject managerObject = new GameObject { name = "[TIMER MANAGER]" };
                    timerManager = managerObject.AddComponent<TimerManager>();
                }
            }

            int flag = 0;
            string modifyTag = tag;

            while (timerManager.dictTimerEvents.ContainsKey(modifyTag))
            {
                flag++;
                StringBuilder stringBuilder = new StringBuilder(tag);
                stringBuilder.Append("(" + flag + ")".ToString());
                modifyTag = stringBuilder.ToString();
                Debug.LogWarning("[TIMER EVENT] : \"" + tag + "\" is repeated.");
            }

            TimerEvent timer = new TimerEvent()
            {
                Tag = modifyTag,
                Duration = duration,
                RemainingTime = duration,
                ElaspedTime = 0.0f,
                IsComplete = false,
                IsActive = false,
                IsPause = false,
                IsCancel = false,
                
                autoKill = false,
                counting = counting,
                onUpdate = null,
                onPause = null,
                onResume = null,
                onComplete = null,
                onCancel = null
            };

            timerManager.AddTimerEvent(modifyTag, timer);

            return timer;
        }

        public static TimerEvent Get(string tag)
        {
            TimerEvent timerEvent;

            if (!timerManager.dictTimerEvents.TryGetValue(tag, out timerEvent)) {
                throw new ArgumentException("[TIMER EVENT] : \"" + tag + "\" not found.");
            }

            return timerEvent;
        }

        public TimerEvent Play()
        {
            RemainingTime = Duration;
            ElaspedTime = 0.0f;

            IsCancel = false;
            IsComplete = false;
            IsPause = false;
            IsActive = true;

            return this;
        }

        public TimerEvent Pause()
        {
            if (!IsPause && !IsCancel && IsActive)
            {
                IsPause = true;
                onPause?.Invoke();
            }
            return this;
        }

        public TimerEvent Resume()
        {
            if (IsPause && !IsCancel && IsActive)
            {
                IsPause = false;
                onResume?.Invoke();
            }
            return this;
        }

        public TimerEvent Cancel()
        {
            if (!IsCancel && IsActive)
            {
                IsCancel = true;
                IsActive = false;
                onCancel?.Invoke();
                RemainingTime = Duration;
            }
            return this;
        }

        public TimerEvent SetAutoKill(bool autoKill)
        {
            this.autoKill = autoKill;

            return this;
        }

        public TimerEvent OnUpdate(Action<float> updateAction)
        {
            onUpdate = updateAction;

            return this;
        }

        public TimerEvent OnPause(Action pauseAction)
        {
            onPause = pauseAction;

            return this;
        }

        public TimerEvent OnResume(Action resumeAction)
        {
            onResume = resumeAction;

            return this;
        }

        public TimerEvent OnComplete(Action completeAction)
        {
            onComplete = completeAction;

            return this;
        }

        public TimerEvent OnCancel(Action cancelAction)
        {
            onCancel = cancelAction;

            return this;
        }

        private void updateTimer(float deltaTime)
        {
            if (IsCancel)
            {
                return;
            }

            if (!IsPause && IsActive)
            {
                if (counting == Counting.Down)
                {
                    timerCountDown(Time.deltaTime);
                }
                else if (counting == Counting.Up)
                {
                    timerCountUp(Time.deltaTime);
                }
            }
        }

        private void timerCountDown(float dt)
        {
            RemainingTime -= dt;
            ElaspedTime = Duration - RemainingTime;

            if (RemainingTime <= 0.0f && !IsComplete)
            {
                IsComplete = true;
                IsActive = false;

                onComplete?.Invoke();
            }
            else
            {
                onUpdate?.Invoke(RemainingTime);
            }
        }

        private void timerCountUp(float dt)
        {
            ElaspedTime += dt;
            RemainingTime = Duration - ElaspedTime;

            if (ElaspedTime >= Duration && !IsComplete)
            {
                IsComplete = true;
                IsActive = false;

                onComplete?.Invoke();
            }
            else
            {
                onUpdate?.Invoke(ElaspedTime);
            }
        }

        private class TimerManager : MonoBehaviour
        {
            private Queue<string> completeTimers = new Queue<string>();
            public Dictionary<string, TimerEvent> dictTimerEvents = new Dictionary<string, TimerEvent>();

            public void AddTimerEvent(string tag, TimerEvent timerEvent)
            {
                dictTimerEvents.Add(tag, timerEvent);
            }

            private void Update()
            {
                if (dictTimerEvents == null || dictTimerEvents.Count == 0)
                {
                    return;
                }

                foreach (var timer in dictTimerEvents)
                {
                    timer.Value.updateTimer(Time.deltaTime);
                    if (timer.Value.IsComplete && timer.Value.autoKill)
                    {
                        completeTimers.Enqueue(timer.Key);
                    }
                }

                while (completeTimers.Count > 0.0f)
                {
                    dictTimerEvents.Remove(completeTimers.Dequeue());
                }
            }
        }
    }
}
