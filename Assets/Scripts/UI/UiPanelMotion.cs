using System.Collections;
using UnityEngine;

namespace DungeonOdyssey.UI
{
    /// <summary>패널 페이드·살짝 떠오르는 등장 연출.</summary>
    public static class UiPanelMotion
    {
        public static void EnsureGroup(GameObject panel, out CanvasGroup group, out RectTransform rect)
        {
            group = panel.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = panel.AddComponent<CanvasGroup>();
            }

            rect = panel.GetComponent<RectTransform>();
        }

        public static IEnumerator FadeIn(GameObject panel, float dur = 0.22f, float rise = 16f)
        {
            if (panel == null)
            {
                yield break;
            }

            panel.SetActive(true);
            EnsureGroup(panel, out var group, out var rect);
            var basePos = rect != null ? rect.anchoredPosition : Vector2.zero;
            group.alpha = 0f;
            var t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                var p = Mathf.Clamp01(t / dur);
                var e = 1f - (1f - p) * (1f - p);
                group.alpha = e;
                if (rect != null)
                {
                    rect.anchoredPosition = basePos + Vector2.up * (rise * (1f - e));
                }

                yield return null;
            }

            group.alpha = 1f;
            if (rect != null)
            {
                rect.anchoredPosition = basePos;
            }
        }

        public static IEnumerator FadeOut(GameObject panel, float dur = 0.16f)
        {
            if (panel == null)
            {
                yield break;
            }

            EnsureGroup(panel, out var group, out _);
            var start = group.alpha;
            var t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                var p = Mathf.Clamp01(t / dur);
                group.alpha = Mathf.Lerp(start, 0f, p);
                yield return null;
            }

            group.alpha = 0f;
            panel.SetActive(false);
        }
    }
}
