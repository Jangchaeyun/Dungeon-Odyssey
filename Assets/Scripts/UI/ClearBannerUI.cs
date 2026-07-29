using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonOdyssey.UI
{
    public enum ToastKind
    {
        Info,
        Success,
        Threat,
        Achievement,
        Style,
        Danger
    }

    /// <summary>등급별 색상·큐잉 토스트 — 메시지가 겹쳐 사라지지 않음.</summary>
    public class ClearBannerUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text messageText;
        [SerializeField] private Image accentBar;

        private readonly Queue<(string msg, float dur, ToastKind kind)> _queue = new();
        private Coroutine _routine;
        private bool _showing;

        private void Awake()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        public void Bind(GameObject panelObject, Text message, Image accent = null)
        {
            panel = panelObject;
            messageText = message;
            accentBar = accent;
            EnsureFrontLayer();
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        public void Show(string message, float autoHideSeconds = 2.6f) =>
            Show(message, ToastKind.Info, autoHideSeconds);

        public void Show(string message, ToastKind kind, float autoHideSeconds = 2.6f)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            // 줄바꿈 유지 · 과장은 2줄로 접어 패널 안에 가둠
            var text = message.Replace("\r\n", "\n").Trim();
            text = System.Text.RegularExpressions.Regex.Replace(text, @"[ \t]+", " ");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\n{3,}", "\n\n");
            if (!text.Contains('\n') && text.Length > 42)
            {
                text = FoldToTwoLines(text, 40);
            }

            var lines = text.Split('\n');
            if (lines.Length > 2)
            {
                text = lines[0].Trim() + "\n" + string.Join(" · ", lines, 1, lines.Length - 1).Trim();
            }

            EnsureFrontLayer();
            _queue.Enqueue((text, autoHideSeconds, kind));
            if (!_showing)
            {
                _routine = StartCoroutine(DrainQueue());
            }
        }

        private static string FoldToTwoLines(string oneLine, int softLimit)
        {
            var parts = oneLine.Split(new[] { " · ", "·" }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                return oneLine.Length > 72 ? oneLine.Substring(0, 70) + "…" : oneLine;
            }

            var line1 = parts[0].Trim();
            var i = 1;
            while (i < parts.Length && (line1 + " · " + parts[i].Trim()).Length <= softLimit)
            {
                line1 += " · " + parts[i].Trim();
                i++;
            }

            var line2 = string.Join(" · ", parts, i, parts.Length - i).Trim();
            if (line2.Length > 56)
            {
                line2 = line2.Substring(0, 54) + "…";
            }

            return string.IsNullOrEmpty(line2) ? line1 : line1 + "\n" + line2;
        }

        private void FitPanelForLines(int lineCount)
        {
            if (panel == null || messageText == null)
            {
                return;
            }

            var panelRt = panel.GetComponent<RectTransform>();
            var textRt = messageText.rectTransform;
            var h = lineCount >= 2 ? 72f : 52f;
            if (panelRt != null)
            {
                panelRt.sizeDelta = new Vector2(Mathf.Max(panelRt.sizeDelta.x, 720f), h);
            }

            if (textRt != null)
            {
                textRt.sizeDelta = new Vector2(680f, lineCount >= 2 ? 52f : 36f);
            }

            messageText.horizontalOverflow = HorizontalWrapMode.Wrap;
            messageText.verticalOverflow = VerticalWrapMode.Truncate;
            messageText.resizeTextForBestFit = true;
            messageText.resizeTextMinSize = 11;
            messageText.resizeTextMaxSize = 17;
            messageText.alignment = TextAnchor.MiddleCenter;
        }

        /// <summary>가방·상점 등 나중에 SetAsLastSibling 된 UI 위에도 토스트가 보이게 함.</summary>
        private void EnsureFrontLayer()
        {
            if (panel == null)
            {
                return;
            }

            panel.transform.SetAsLastSibling();

            var img = panel.GetComponent<Image>();
            if (img != null)
            {
                img.raycastTarget = false;
            }

            if (messageText != null)
            {
                messageText.raycastTarget = false;
            }

            if (accentBar != null)
            {
                accentBar.raycastTarget = false;
            }

            var overlay = panel.GetComponent<Canvas>();
            if (overlay == null)
            {
                overlay = panel.AddComponent<Canvas>();
            }

            overlay.overrideSorting = true;
            overlay.sortingOrder = 32000;
        }

        public void Hide()
        {
            _queue.Clear();
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }

            _showing = false;
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private IEnumerator DrainQueue()
        {
            _showing = true;
            while (_queue.Count > 0)
            {
                var (msg, dur, kind) = _queue.Dequeue();
                EnsureFrontLayer();
                FitPanelForLines(msg.Contains('\n') ? 2 : 1);
                ApplyKind(kind);
                if (messageText != null)
                {
                    messageText.text = msg;
                }

                yield return UiPanelMotion.FadeIn(panel, 0.2f, 16f);
                if (dur > 0f)
                {
                    yield return new WaitForSecondsRealtime(dur);
                    yield return UiPanelMotion.FadeOut(panel, 0.16f);
                }
            }

            _showing = false;
            _routine = null;
        }

        private void ApplyKind(ToastKind kind)
        {
            var color = kind switch
            {
                ToastKind.Success => new Color(0.45f, 0.9f, 0.72f),
                ToastKind.Threat => new Color(0.5f, 0.78f, 0.95f),
                ToastKind.Achievement => new Color(0.55f, 0.92f, 0.88f),
                ToastKind.Style => new Color(0.95f, 0.72f, 0.4f),
                ToastKind.Danger => new Color(0.95f, 0.4f, 0.45f),
                _ => new Color(0.88f, 0.9f, 0.93f)
            };

            if (messageText != null)
            {
                messageText.color = color;
            }

            if (accentBar != null)
            {
                accentBar.color = new Color(color.r, color.g, color.b, 0.9f);
            }
        }
    }
}
