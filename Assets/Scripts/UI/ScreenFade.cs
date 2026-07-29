using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonOdyssey.UI
{
    /// <summary>방 이동·씬 전환용 전체 화면 암전.</summary>
    public static class ScreenFade
    {
        private static CanvasGroup _group;
        private static Image _veil;

        public static void Ensure()
        {
            if (_group != null)
            {
                return;
            }

            var canvas = RuntimeUiFactory.CreateCanvas("ScreenFadeCanvas");
            canvas.sortingOrder = 900;
            Object.DontDestroyOnLoad(canvas.gameObject);

            var go = new GameObject("Veil");
            go.transform.SetParent(canvas.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            _veil = go.AddComponent<Image>();
            _veil.color = new Color(0.02f, 0.02f, 0.03f, 1f);
            _veil.raycastTarget = false;

            _group = go.AddComponent<CanvasGroup>();
            _group.alpha = 0f;
            _group.blocksRaycasts = false;
            _group.interactable = false;
        }

        public static IEnumerator Out(float duration = 0.28f)
        {
            Ensure();
            _group.blocksRaycasts = true;
            yield return Fade(_group.alpha, 1f, duration);
        }

        public static IEnumerator In(float duration = 0.34f)
        {
            Ensure();
            yield return Fade(_group != null ? _group.alpha : 1f, 0f, duration);
            if (_group != null)
            {
                _group.blocksRaycasts = false;
            }
        }

        /// <summary>목표 알파까지 (걷기와 동시에 암전할 때).</summary>
        public static IEnumerator To(float alpha, float duration = 0.3f)
        {
            Ensure();
            if (alpha > 0.05f)
            {
                _group.blocksRaycasts = true;
            }

            yield return Fade(_group.alpha, Mathf.Clamp01(alpha), duration);
            if (alpha <= 0.05f && _group != null)
            {
                _group.blocksRaycasts = false;
            }
        }

        public static void Set(float alpha)
        {
            Ensure();
            _group.alpha = Mathf.Clamp01(alpha);
            _group.blocksRaycasts = alpha > 0.05f;
        }

        /// <summary>던전 전환용 — 살짝 차가운 틴트로 분위기 유지.</summary>
        public static void SetTheme(bool dungeon)
        {
            Ensure();
            if (_veil != null)
            {
                _veil.color = dungeon
                    ? new Color(0.04f, 0.05f, 0.09f, 1f)
                    : new Color(0.02f, 0.02f, 0.03f, 1f);
            }
        }

        private static IEnumerator Fade(float from, float to, float duration)
        {
            if (_group == null)
            {
                yield break;
            }

            duration = Mathf.Max(0.05f, duration);
            var t = 0f;
            _group.alpha = from;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                var u = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duration));
                _group.alpha = Mathf.Lerp(from, to, u);
                yield return null;
            }

            _group.alpha = to;
        }
    }
}
