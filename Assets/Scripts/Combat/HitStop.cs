using System.Collections;
using DungeonOdyssey.Core;
using UnityEngine;

namespace DungeonOdyssey.Combat
{
    /// <summary>
    /// 짧은 히트스톱 — 타격 순간에만 timeScale 을 깎아 묵직하게.
    /// </summary>
    public class HitStop : MonoBehaviour
    {
        private static HitStop _instance;
        private Coroutine _routine;
        private float _defaultFixed;

        public static void Pulse(float duration = 0.06f, float scale = 0.08f)
        {
            Ensure().DoPulse(duration, scale);
        }

        private static HitStop Ensure()
        {
            if (_instance != null)
            {
                return _instance;
            }

            var go = new GameObject("HitStop");
            Object.DontDestroyOnLoad(go);
            _instance = go.AddComponent<HitStop>();
            _instance._defaultFixed = Time.fixedDeltaTime;
            return _instance;
        }

        private void DoPulse(float duration, float scale)
        {
            if (GameUi.IsPaused || GameUi.IsBlocking)
            {
                return;
            }

            if (_routine != null)
            {
                StopCoroutine(_routine);
                RestoreTimeScale();
            }

            _routine = StartCoroutine(PulseRoutine(duration, scale));
        }

        private IEnumerator PulseRoutine(float duration, float scale)
        {
            Time.timeScale = Mathf.Clamp(scale, 0.01f, 1f);
            Time.fixedDeltaTime = _defaultFixed * Time.timeScale;
            yield return new WaitForSecondsRealtime(duration);
            RestoreTimeScale();
            _routine = null;
        }

        private void RestoreTimeScale()
        {
            if (GameUi.IsPaused || GameUi.IsBlocking)
            {
                Time.timeScale = 0f;
                Time.fixedDeltaTime = _defaultFixed;
                return;
            }

            Time.timeScale = 1f;
            Time.fixedDeltaTime = _defaultFixed;
        }
    }
}
