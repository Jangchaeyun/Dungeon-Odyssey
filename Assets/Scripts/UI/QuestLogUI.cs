using DungeonOdyssey.Combat;
using DungeonOdyssey.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonOdyssey.UI
{
    /// <summary>퀘스트 로그 — 진행 / 일일만 카드로 표시.</summary>
    public class QuestLogUI : MonoBehaviour
    {
        private const int CardCount = 2;
        private const float ActiveCardY = 56f;
        private const float DailyCardY = -74f;
        private const float SoloCardY = 0f;
        private GameObject _dim;
        private GameObject _panel;
        private readonly GameObject[] _cards = new GameObject[CardCount];
        private readonly RectTransform[] _cardRts = new RectTransform[CardCount];
        private readonly Text[] _tags = new Text[CardCount];
        private readonly Text[] _titles = new Text[CardCount];
        private readonly Text[] _details = new Text[CardCount];
        private readonly Text[] _progress = new Text[CardCount];
        private readonly Text[] _status = new Text[CardCount];
        private readonly Image[] _rails = new Image[CardCount];
        private Text _footer;
        private bool _open;

        public bool IsOpen => _open;

        public static QuestLogUI Build(Transform canvasRoot)
        {
            var host = canvasRoot.gameObject.GetComponent<QuestLogUI>();
            if (host == null)
            {
                host = canvasRoot.gameObject.AddComponent<QuestLogUI>();
            }

            host.Construct(canvasRoot);
            return host;
        }

        private void Construct(Transform canvasRoot)
        {
            if (_panel != null)
            {
                return;
            }

            _dim = new GameObject("QuestDim");
            _dim.transform.SetParent(canvasRoot, false);
            var dimRt = _dim.AddComponent<RectTransform>();
            dimRt.anchorMin = Vector2.zero;
            dimRt.anchorMax = Vector2.one;
            dimRt.offsetMin = Vector2.zero;
            dimRt.offsetMax = Vector2.zero;
            var dimImg = _dim.AddComponent<Image>();
            dimImg.color = new Color(0.02f, 0.03f, 0.05f, 0.62f);
            var dimBtn = _dim.AddComponent<Button>();
            dimBtn.targetGraphic = dimImg;
            dimBtn.onClick.AddListener(Close);
            _dim.SetActive(false);

            _panel = new GameObject("QuestLogPanel");
            _panel.transform.SetParent(canvasRoot, false);
            var panelRt = _panel.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.pivot = new Vector2(0.5f, 0.5f);
            panelRt.sizeDelta = new Vector2(620f, 440f);
            panelRt.anchoredPosition = Vector2.zero;
            var panelImg = _panel.AddComponent<Image>();
            panelImg.color = new Color(0.055f, 0.065f, 0.08f, 0.97f);
            var panelOutline = _panel.AddComponent<Outline>();
            panelOutline.effectColor = new Color(0.4f, 0.78f, 0.75f, 0.45f);
            panelOutline.effectDistance = new Vector2(1.2f, -1.2f);

            var title = RuntimeUiFactory.CreateText(_panel.transform, "Title", "퀘스트", 26,
                new Vector2(0f, 176f), new Vector2(540f, 34f), TextAnchor.MiddleCenter,
                new Color(0.92f, 0.95f, 0.96f));
            title.fontStyle = FontStyle.Bold;

            RuntimeUiFactory.CreateText(_panel.transform, "Sub", "지금 할 일과 보상을 한눈에", 13,
                new Vector2(0f, 146f), new Vector2(540f, 22f), TextAnchor.MiddleCenter,
                new Color(0.58f, 0.66f, 0.7f));

            // 진행 / 일일만 (진행은 수락 후에만 표시)
            BuildCard(_panel.transform, "ActiveCard", ActiveCardY, 0);
            BuildCard(_panel.transform, "DailyCard", DailyCardY, 1);

            _footer = RuntimeUiFactory.CreateText(_panel.transform, "Footer", "", 12,
                new Vector2(0f, -158f), new Vector2(540f, 28f), TextAnchor.MiddleCenter,
                new Color(0.55f, 0.62f, 0.66f));
            _footer.horizontalOverflow = HorizontalWrapMode.Wrap;
            _footer.verticalOverflow = VerticalWrapMode.Truncate;

            var close = RuntimeUiFactory.CreateButton(_panel.transform, "CloseBtn", "닫기",
                new Vector2(0f, -190f), new Vector2(160f, 40f), 18);
            close.onClick.AddListener(() =>
            {
                CombatAudio.UiClick();
                Close();
            });

            _panel.SetActive(false);
        }

        private void BuildCard(Transform parent, string name, float y, int index)
        {
            var card = new GameObject(name);
            card.transform.SetParent(parent, false);
            var rt = card.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0f, y);
            rt.sizeDelta = new Vector2(492f, 118f);
            _cards[index] = card;
            _cardRts[index] = rt;
            var bg = card.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.1f, 0.12f, 0.92f);
            var outline = card.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.08f);
            outline.effectDistance = new Vector2(1f, -1f);

            var rail = new GameObject("Rail");
            rail.transform.SetParent(card.transform, false);
            var railRt = rail.AddComponent<RectTransform>();
            railRt.anchorMin = new Vector2(0f, 0.1f);
            railRt.anchorMax = new Vector2(0f, 0.9f);
            railRt.pivot = new Vector2(0f, 0.5f);
            railRt.sizeDelta = new Vector2(4f, 0f);
            _rails[index] = rail.AddComponent<Image>();
            _rails[index].color = new Color(0.45f, 0.85f, 0.82f);

            // 위에서부터: 태그 · 진행도 → 제목 → 설명 → 상태 (여유 간격)
            _tags[index] = MakeCardText(card.transform, "Tag", "태그", 11,
                new Vector2(16f, -10f), new Vector2(200f, 18f), TextAnchor.UpperLeft,
                new Color(0.55f, 0.85f, 0.82f));
            _tags[index].fontStyle = FontStyle.Bold;

            _progress[index] = MakeCardText(card.transform, "Progress", "", 15,
                new Vector2(-16f, -8f), new Vector2(160f, 22f), TextAnchor.UpperRight,
                new Color(1f, 0.86f, 0.45f));
            _progress[index].fontStyle = FontStyle.Bold;
            _progress[index].alignment = TextAnchor.MiddleRight;

            _titles[index] = MakeCardText(card.transform, "Title", "제목", 18,
                new Vector2(16f, -32f), new Vector2(460f, 26f), TextAnchor.UpperLeft,
                new Color(0.92f, 0.94f, 0.96f));
            _titles[index].fontStyle = FontStyle.Bold;
            _titles[index].horizontalOverflow = HorizontalWrapMode.Overflow;
            _titles[index].verticalOverflow = VerticalWrapMode.Truncate;

            _details[index] = MakeCardText(card.transform, "Detail", "설명", 13,
                new Vector2(16f, -60f), new Vector2(460f, 22f), TextAnchor.UpperLeft,
                new Color(0.68f, 0.74f, 0.78f));
            _details[index].horizontalOverflow = HorizontalWrapMode.Overflow;
            _details[index].verticalOverflow = VerticalWrapMode.Truncate;

            _status[index] = MakeCardText(card.transform, "Status", "", 12,
                new Vector2(16f, -88f), new Vector2(460f, 20f), TextAnchor.UpperLeft,
                new Color(0.7f, 0.88f, 0.82f));
            _status[index].horizontalOverflow = HorizontalWrapMode.Overflow;
            _status[index].verticalOverflow = VerticalWrapMode.Truncate;
        }

        private static Text MakeCardText(Transform parent, string name, string content, int size,
            Vector2 anchoredPos, Vector2 sizeDelta, TextAnchor align, Color color)
        {
            var text = RuntimeUiFactory.CreateText(parent, name, content, size, anchoredPos, sizeDelta, align, color);
            var rt = text.rectTransform;
            if (align == TextAnchor.UpperLeft || align == TextAnchor.MiddleLeft)
            {
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
            }
            else if (align == TextAnchor.UpperRight || align == TextAnchor.MiddleRight)
            {
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
            }

            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = sizeDelta;
            return text;
        }

        public void Toggle()
        {
            if (_open)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        public void Open()
        {
            Refresh();
            _open = true;
            if (_dim != null)
            {
                _dim.SetActive(true);
                _dim.transform.SetAsLastSibling();
            }

            if (_panel != null)
            {
                _panel.SetActive(true);
                _panel.transform.SetAsLastSibling();
            }

            GameUi.IsBlocking = true;
        }

        public void Close()
        {
            _open = false;
            if (_dim != null)
            {
                _dim.SetActive(false);
            }

            if (_panel != null)
            {
                _panel.SetActive(false);
            }

            if (!FindAnyOtherBlocking())
            {
                GameUi.IsBlocking = false;
            }
        }

        public void Refresh()
        {
            if (_panel == null)
            {
                return;
            }

            var save = GameManager.Instance?.CurrentSave;
            if (save == null)
            {
                ApplyCard(0, default);
                ApplyCard(1, default);
                LayoutCards(false);
                if (_footer != null)
                {
                    _footer.text = "";
                }

                return;
            }

            QuestCatalog.BuildPlayerLog(save, out var active, out var daily, out _, out var footer);
            var hasActive = !string.IsNullOrEmpty(active.Title);
            ApplyCard(0, active);
            ApplyCard(1, daily);
            LayoutCards(hasActive);
            if (_footer != null)
            {
                _footer.text = footer;
            }
        }

        private void ApplyCard(int i, QuestCatalog.LogCard card)
        {
            if (i < 0 || i >= CardCount || _tags[i] == null)
            {
                return;
            }

            var visible = !string.IsNullOrEmpty(card.Title);
            if (_cards[i] != null)
            {
                _cards[i].SetActive(visible);
            }

            if (!visible)
            {
                return;
            }

            _tags[i].text = card.Tag ?? "";
            _tags[i].color = card.Accent;
            _titles[i].text = card.Title ?? "";
            _details[i].text = card.Detail ?? "";
            _progress[i].text = card.Progress ?? "";
            _status[i].text = card.Status ?? "";
            _status[i].color = card.Accent;
            if (_rails[i] != null)
            {
                _rails[i].color = card.Accent;
            }
        }

        private void LayoutCards(bool hasActive)
        {
            if (_cardRts[0] != null)
            {
                _cardRts[0].anchoredPosition = new Vector2(0f, ActiveCardY);
            }

            if (_cardRts[1] != null)
            {
                _cardRts[1].anchoredPosition = new Vector2(0f, hasActive ? DailyCardY : SoloCardY);
            }
        }

        private static bool FindAnyOtherBlocking()
        {
            var bag = Object.FindFirstObjectByType<BagUI>();
            if (bag != null && bag.IsOpen)
            {
                return true;
            }

            var forge = Object.FindFirstObjectByType<ForgeShopUI>();
            return forge != null && forge.IsOpen;
        }
    }
}
