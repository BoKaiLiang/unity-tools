using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PKTools.Timer
{
    public enum Counting
    {
        CountDown,
        CountUp
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

        // From zero or duration
        private Counting counting = Counting.CountDown;

        private Action<float> onUpdate;     // pass elapsed time / remaining time
        private Action onPause;
        private Action onResume;
        private Action onComplete;
        private Action onCancel;

        private static TimerManager timerManager;

        private TimerEvent() { }

        /// <summary>
        /// Default timer event is set as "counting down" and "not auto-kill".
        /// After create, should invoke "Play()" to active the timer.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static TimerEvent Create(string tag, float duration)
        {
            if (tag == null || tag == string.Empty)
            {
                throw new ArgumentException("Tag can't be null or empty.");
            }

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

            TimerEvent timer = new TimerEvent();
            timer.Tag = tag;
            timer.Duration = duration;
            timer.RemainingTime = duration;
            timer.ElaspedTime = 0.0f;

            timerManager.AddTimerEvent(timer.Tag, timer);

            return timer;
        }

        public TimerEvent Play()
        {
            if (Duration <= 0.0f)
            {
                throw new InvalidOperationException("Setup Duration is: " + Duration.ToString());
            }

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
            if (!IsPause)
            {
                IsPause = true;
                onPause?.Invoke();
            }
            return this;
        }

        public TimerEvent Resume()
        {
            if (IsPause)
            {
                IsPause = false;
                onResume?.Invoke();
            }
            return this;
        }

        public TimerEvent Cancel()
        {
            if (!IsCancel)
            {
                IsCancel = true;
                IsActive = false;
                onCancel?.Invoke();
                RemainingTime = Duration;
            }
            return this;
        }

        public TimerEvent SetCountingWay(Counting counting)
        {
            this.counting = counting;

            return this;
        }

        public TimerEvent SetAutoKill(bool autoKill)
        {
            this.autoKill = autoKill;

            return this;
        }

        public static TimerEvent GetTimer(string tag)
        {
            TimerEvent timerEvent;
            bool tryToGet = timerManager.dictTimerEvents.TryGetValue(tag, out timerEvent);

            if (tryToGet == false)
            {
                throw new NullReferenceException("Can not find the register timer event: \"" + tag + "\".");
            }

            return timerEvent;
        }

        public static void Destroy(string tag)
        {
            timerManager.dictTimerEvents.Remove(tag);
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
                if (counting == Counting.CountDown)
                {
                    timerCountDown(Time.deltaTime);
                }
                else if (counting == Counting.CountUp)
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

            private void Start()
            {
                DontDestroyOnLoad(this);
            }

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
