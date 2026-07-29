using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DungeonOdyssey.Combat;
using DungeonOdyssey.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonOdyssey.UI
{
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private GameObject dim;
        [SerializeField] private Text nameText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Text promptText;
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private Transform choiceRoot;

        private bool _visible;
        private float _openGrace;
        private string _fullBody = "";
        private Coroutine _typeRoutine;
        private Coroutine _fadeRoutine;
        private bool _typing;
        private bool _isMenu;
        private readonly List<GameObject> _choiceButtons = new();
        private Vector2 _scrollDefaultMin = new(28f, 48f);
        private Vector2 _scrollDefaultMax = new(-28f, -60f);

        private void Awake()
        {
            HideImmediate();
        }

        public void Bind(GameObject panelObject, Text name, Text body, Text prompt = null,
            GameObject dimObject = null, ScrollRect scrollRect = null, Transform choices = null)
        {
            panel = panelObject;
            nameText = name;
            bodyText = body;
            promptText = prompt;
            dim = dimObject;
            scroll = scrollRect;
            choiceRoot = choices;
            if (scroll != null && scroll.transform is RectTransform srt)
            {
                _scrollDefaultMin = srt.offsetMin;
                _scrollDefaultMax = srt.offsetMax;
            }

            HideImmediate();
        }

        public void Show(string speaker, string body)
        {
            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
            }

            if (_typeRoutine != null)
            {
                StopCoroutine(_typeRoutine);
                _typeRoutine = null;
            }

            ClearChoices();
            RestoreNormalLayout();

            if (nameText != null)
            {
                nameText.text = speaker ?? "";
            }

            _fullBody = body ?? "";
            _isMenu = IsMenuBody(_fullBody);

            if (promptText != null)
            {
                promptText.text = _isMenu ? "숫자 키 또는 버튼  ·  Space 닫기" : "Space  /  클릭으로 닫기";
            }

            if (dim != null)
            {
                dim.SetActive(true);
                dim.transform.SetAsLastSibling();
            }

            if (panel != null)
            {
                panel.transform.SetAsLastSibling();
            }

            _visible = true;
            _openGrace = 0.12f;
            GameUi.IsBlocking = true;
            FindFirstObjectByType<HudUI>()?.RefreshHintVisibility();
            CombatAudio.UiClick();
            _fadeRoutine = StartCoroutine(UiPanelMotion.FadeIn(panel, 0.18f, 10f));

            if (_isMenu)
            {
                var intro = ExtractIntro(_fullBody);
                if (bodyText != null)
                {
                    bodyText.text = intro;
                    bodyText.fontSize = 14;
                    bodyText.lineSpacing = 1.05f;
                }

                _typing = false;
                RefreshBodyLayout();
                BuildChoiceButtons(_fullBody);
                ApplyMenuLayout(_choiceButtons.Count);
            }
            else if (_fullBody.Length > 100)
            {
                if (bodyText != null)
                {
                    bodyText.text = _fullBody;
                    bodyText.fontSize = 16;
                    bodyText.lineSpacing = 1.18f;
                }

                _typing = false;
                RefreshBodyLayout();
            }
            else
            {
                if (bodyText != null)
                {
                    bodyText.text = "";
                    bodyText.fontSize = 16;
                    bodyText.lineSpacing = 1.18f;
                }

                _typeRoutine = StartCoroutine(TypeBody());
            }
        }

        private void ApplyMenuLayout(int choiceCount)
        {
            // 상태: 상단 고정 영역 (이름 아래)
            if (scroll != null && scroll.transform is RectTransform scrollRt)
            {
                scrollRt.anchorMin = new Vector2(0f, 1f);
                scrollRt.anchorMax = new Vector2(1f, 1f);
                scrollRt.pivot = new Vector2(0.5f, 1f);
                scrollRt.offsetMin = new Vector2(28f, -168f);
                scrollRt.offsetMax = new Vector2(-28f, -56f);
            }

            // 선택지: 패널 하단, 안내문 바로 위
            if (choiceRoot is RectTransform crt)
            {
                var row = 44f;
                var gap = 6f;
                var h = Mathf.Max(row, choiceCount * (row + gap));
                crt.anchorMin = new Vector2(0.5f, 0f);
                crt.anchorMax = new Vector2(0.5f, 0f);
                crt.pivot = new Vector2(0.5f, 0f);
                crt.sizeDelta = new Vector2(660f, h);
                crt.anchoredPosition = new Vector2(0f, 40f);
                crt.SetAsLastSibling();
            }

            if (promptText != null)
            {
                var pr = promptText.rectTransform;
                pr.SetAsLastSibling();
                pr.anchorMin = new Vector2(0f, 0f);
                pr.anchorMax = new Vector2(1f, 0f);
                pr.pivot = new Vector2(0.5f, 0f);
                pr.offsetMin = new Vector2(24f, 6f);
                pr.offsetMax = new Vector2(-24f, 32f);
            }

            if (panel != null && panel.transform is RectTransform panelRt)
            {
                // 상단(이름+상태) + 선택지 + 하단 안내
                var h = 56f + 120f + 20f + choiceCount * 50f + 40f;
                panelRt.sizeDelta = new Vector2(700f, Mathf.Clamp(h, 400f, 620f));
            }
        }

        private void RestoreNormalLayout()
        {
            if (scroll != null && scroll.transform is RectTransform scrollRt)
            {
                scrollRt.anchorMin = new Vector2(0f, 0f);
                scrollRt.anchorMax = new Vector2(1f, 1f);
                scrollRt.pivot = new Vector2(0.5f, 0.5f);
                scrollRt.anchoredPosition = Vector2.zero;
                scrollRt.offsetMin = _scrollDefaultMin;
                scrollRt.offsetMax = _scrollDefaultMax;
            }

            if (panel != null && panel.transform is RectTransform panelRt)
            {
                panelRt.sizeDelta = new Vector2(720f, 480f);
            }

            if (bodyText != null)
            {
                bodyText.fontSize = 16;
                bodyText.lineSpacing = 1.18f;
            }
        }

        private void BuildChoiceButtons(string body)
        {
            if (choiceRoot == null)
            {
                return;
            }

            // 이전 자식 정리
            for (var i = choiceRoot.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(choiceRoot.GetChild(i).gameObject);
            }

            _choiceButtons.Clear();
            choiceRoot.gameObject.SetActive(true);

            // 공백 필수 — "[1]로 수락" 같은 본문 문구를 버튼으로 오인하지 않음
            var matches = Regex.Matches(body, @"\[(\d)\]\s+([^\[\n\r]+)");
            var items = new List<(int idx, string label)>();
            foreach (Match m in matches)
            {
                if (!m.Success || !int.TryParse(m.Groups[1].Value, out var idx))
                {
                    continue;
                }

                var label = Regex.Replace(m.Groups[2].Value.Trim(), @"·{2,}", "·");
                if (label.Length > 44)
                {
                    label = label.Substring(0, 42) + "…";
                }

                items.Add((idx, label));
            }

            // 아래에서 위로: 첫 옵션이 위
            var row = 44f;
            var gap = 6f;
            var total = items.Count;
            for (var i = 0; i < total; i++)
            {
                var (idx, label) = items[i];
                // pivot bottom: y = (total-1-i)*(row+gap) + row/2 … simpler: top of stack
                var y = (total - 1 - i) * (row + gap) + row * 0.5f;
                var btn = RuntimeUiFactory.CreateButton(choiceRoot, $"Choice{idx}",
                    $"[{idx}]  {label}", new Vector2(0f, y), new Vector2(640f, row), 15);
                var captured = idx;
                btn.onClick.AddListener(() =>
                {
                    CombatAudio.UiClick();
                    GameInput.PressMenu(captured);
                });
                _choiceButtons.Add(btn.gameObject);
            }
        }

        private void ClearChoices()
        {
            foreach (var go in _choiceButtons)
            {
                if (go != null)
                {
                    Object.Destroy(go);
                }
            }

            _choiceButtons.Clear();
            if (choiceRoot != null)
            {
                for (var i = choiceRoot.childCount - 1; i >= 0; i--)
                {
                    Object.Destroy(choiceRoot.GetChild(i).gameObject);
                }

                choiceRoot.gameObject.SetActive(false);
            }
        }

        private static string ExtractIntro(string body)
        {
            if (string.IsNullOrEmpty(body))
            {
                return "";
            }

            var idx = body.IndexOf("[1]");
            if (idx <= 0)
            {
                idx = body.IndexOf("──");
            }

            if (idx <= 0)
            {
                return body;
            }

            return body.Substring(0, idx).TrimEnd();
        }

        private void RefreshBodyLayout()
        {
            if (bodyText == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(bodyText.rectTransform);
            if (bodyText.rectTransform.parent is RectTransform content)
            {
                var h = Mathf.Max(40f, bodyText.preferredHeight + 8f);
                content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            }

            if (scroll != null)
            {
                scroll.verticalNormalizedPosition = 1f;
            }
        }

        private static bool IsMenuBody(string body) =>
            !string.IsNullOrEmpty(body) && (body.Contains("[1]") || body.Contains("[2]") || body.Contains("──"));

        public void Hide()
        {
            if (!_visible)
            {
                HideImmediate();
                return;
            }

            if (_typeRoutine != null)
            {
                StopCoroutine(_typeRoutine);
                _typeRoutine = null;
            }

            ClearChoices();
            RestoreNormalLayout();
            _visible = false;
            _typing = false;
            if (!GameUi.IsPaused)
            {
                GameUi.IsBlocking = false;
            }

            if (dim != null)
            {
                dim.SetActive(false);
            }

            FindFirstObjectByType<HudUI>()?.RefreshHintVisibility();

            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
            }

            _fadeRoutine = StartCoroutine(UiPanelMotion.FadeOut(panel, 0.14f));
        }

        private void HideImmediate()
        {
            ClearChoices();
            RestoreNormalLayout();
            if (panel != null)
            {
                panel.SetActive(false);
            }

            if (dim != null)
            {
                dim.SetActive(false);
            }

            _visible = false;
            _typing = false;
        }

        public bool IsVisible => _visible;

        private IEnumerator TypeBody()
        {
            _typing = true;
            if (bodyText == null)
            {
                _typing = false;
                yield break;
            }

            for (var i = 0; i <= _fullBody.Length; i++)
            {
                bodyText.text = _fullBody.Substring(0, i);
                var delay = 0.014f;
                if (i > 0 && i <= _fullBody.Length)
                {
                    var ch = _fullBody[i - 1];
                    if (ch == '\n')
                    {
                        delay = 0.04f;
                    }
                    else if (ch is '.' or '!' or '?')
                    {
                        delay = 0.05f;
                    }
                }

                yield return new WaitForSecondsRealtime(delay);
            }

            _typing = false;
            _typeRoutine = null;
            RefreshBodyLayout();
        }

        private void Update()
        {
            if (!_visible)
            {
                return;
            }

            if (_openGrace > 0f)
            {
                _openGrace -= Time.unscaledDeltaTime;
                return;
            }

            var wantClose = GameInput.AttackDown;
            if (!_isMenu && Input.GetMouseButtonDown(0))
            {
                wantClose = true;
            }

            if (!wantClose)
            {
                return;
            }

            if (_typing)
            {
                if (_typeRoutine != null)
                {
                    StopCoroutine(_typeRoutine);
                    _typeRoutine = null;
                }

                if (bodyText != null)
                {
                    bodyText.text = _fullBody;
                }

                _typing = false;
                RefreshBodyLayout();
                return;
            }

            if (_isMenu && !GameInput.AttackDown)
            {
                return;
            }

            Hide();
        }
    }
}
