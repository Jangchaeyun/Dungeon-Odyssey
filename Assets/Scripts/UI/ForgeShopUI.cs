using System.Collections;
using System.Collections.Generic;
using DungeonOdyssey.Combat;
using DungeonOdyssey.Core;
using DungeonOdyssey.Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DungeonOdyssey.UI
{
    /// <summary>마을 메뉴 공용 — 상점은 아이콘 그리드, 가이드는 텍스트 목록.</summary>
    public class ForgeShopUI : MonoBehaviour
    {
        private GameObject _root;
        private GameObject _dim;
        private Text _title;
        private Text _status;
        private Text _prompt;
        private Transform _list;
        private readonly List<GameObject> _rows = new();
        private bool _open;
        private bool _iconMode;
        private System.Action<int> _onSelect;
        private string _statusBaseline = "";
        private readonly bool[] _enabled = new bool[32];
        private readonly string[] _labels = new string[32];
        private readonly string[] _details = new string[32];

        public bool IsOpen => _open;

        public void Bind(GameObject root, GameObject dim, Text title, Text status, Transform list,
            Text prompt = null)
        {
            _root = root;
            _dim = dim;
            _title = title;
            _status = status;
            _list = list;
            _prompt = prompt;
            HideImmediate();
        }

        public void Show(string title, string statusBlock, IReadOnlyList<(string label, bool enabled)> actions,
            System.Action<int> onSelect)
        {
            var tinted =
                new List<(string label, bool enabled, Color? tint, WeaponRarity? rarity, string iconKind, WeaponId
                    weapon)>();
            foreach (var a in actions)
            {
                tinted.Add((a.label, a.enabled, null, null, null, default));
            }

            Show(title, statusBlock, tinted, onSelect);
        }

        public void Show(string title, string statusBlock,
            IReadOnlyList<(string label, bool enabled, Color? tint)> actions, System.Action<int> onSelect)
        {
            var withRarity =
                new List<(string label, bool enabled, Color? tint, WeaponRarity? rarity, string iconKind, WeaponId
                    weapon)>();
            foreach (var a in actions)
            {
                withRarity.Add((a.label, a.enabled, a.tint, null, null, default));
            }

            Show(title, statusBlock, withRarity, onSelect);
        }

        public void Show(string title, string statusBlock,
            IReadOnlyList<(string label, bool enabled, Color? tint, WeaponRarity? rarity)> actions,
            System.Action<int> onSelect)
        {
            var withIcon =
                new List<(string label, bool enabled, Color? tint, WeaponRarity? rarity, string iconKind, WeaponId
                    weapon)>();
            foreach (var a in actions)
            {
                withIcon.Add((a.label, a.enabled, a.tint, a.rarity, null, default));
            }

            Show(title, statusBlock, withIcon, onSelect);
        }

        public void Show(string title, string statusBlock,
            IReadOnlyList<(string label, bool enabled, Color? tint, WeaponRarity? rarity, string iconKind, WeaponId
                weapon)> actions,
            System.Action<int> onSelect)
        {
            ApplyContent(title, statusBlock, actions, onSelect);

            if (_dim != null)
            {
                _dim.SetActive(true);
                _dim.transform.SetAsLastSibling();
            }

            if (_root != null)
            {
                _root.transform.SetAsLastSibling();
            }

            var wasOpen = _open;
            _open = true;
            GameUi.IsBlocking = true;
            FindFirstObjectByType<HudUI>()?.RefreshHintVisibility();

            if (wasOpen)
            {
                if (_root != null && !_root.activeSelf)
                {
                    _root.SetActive(true);
                }

                return;
            }

            CombatAudio.ShopOpen();
            StartCoroutine(UiPanelMotion.FadeIn(_root, 0.16f, 12f));
        }

        private void ApplyContent(string title, string statusBlock,
            IReadOnlyList<(string label, bool enabled, Color? tint, WeaponRarity? rarity, string iconKind, WeaponId
                weapon)> actions,
            System.Action<int> onSelect)
        {
            _onSelect = onSelect;
            for (var i = 0; i < _enabled.Length; i++)
            {
                _enabled[i] = false;
                _labels[i] = null;
                _details[i] = null;
            }

            _statusBaseline = statusBlock ?? "";
            if (_title != null)
            {
                _title.text = title;
            }

            if (_status != null)
            {
                _status.text = _statusBaseline;
            }

            _iconMode = false;
            if (actions != null)
            {
                foreach (var a in actions)
                {
                    if (!string.IsNullOrEmpty(a.iconKind))
                    {
                        _iconMode = true;
                        break;
                    }
                }
            }

            EnsureListLayout(_iconMode);
            ClearRows();
            if (_list == null || actions == null)
            {
                return;
            }

            for (var i = 0; i < actions.Count && i < _enabled.Length - 1; i++)
            {
                var idx = i + 1;
                var (label, enabled, tint, rarity, iconKind, weapon) = actions[i];
                _enabled[idx] = enabled;
                _labels[idx] = label;
                _details[idx] = BuildDetail(iconKind, weapon, label, enabled);
                _rows.Add(_iconMode
                    ? CreateIconTile(idx, label, enabled, tint, rarity, iconKind, weapon, _details[idx])
                    : CreateTextRow(idx, label, enabled, tint));
            }

            if (_root != null && _root.transform is RectTransform panelRt)
            {
                panelRt.sizeDelta = new Vector2(720f, 640f);
            }

            if (_prompt != null)
            {
                _prompt.text = _iconMode
                    ? "아이콘에 올리면 상세 설명  ·  클릭으로 구매  ·  Space/Esc 닫기"
                    : "클릭으로 선택  ·  Space / Esc 닫기";
            }

            if (_list is RectTransform listRt)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(listRt);
                listRt.anchoredPosition = Vector2.zero;
            }
        }

        private void EnsureListLayout(bool iconGrid)
        {
            if (_list == null)
            {
                return;
            }

            var go = _list.gameObject;

            if (iconGrid)
            {
                // LayoutGroup은 오브젝트당 하나만 가능 — Vertical 제거 후 Grid 추가
                var vertical = go.GetComponent<VerticalLayoutGroup>();
                if (vertical != null)
                {
                    DestroyImmediate(vertical);
                }

                var grid = go.GetComponent<GridLayoutGroup>();
                if (grid == null)
                {
                    grid = go.AddComponent<GridLayoutGroup>();
                }

                if (grid == null)
                {
                    return;
                }

                grid.enabled = true;
                grid.cellSize = new Vector2(118f, 132f);
                grid.spacing = new Vector2(10f, 10f);
                grid.padding = new RectOffset(10, 10, 8, 8);
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 5;
                grid.childAlignment = TextAnchor.UpperCenter;
                grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
                grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            }
            else
            {
                var grid = go.GetComponent<GridLayoutGroup>();
                if (grid != null)
                {
                    DestroyImmediate(grid);
                }

                var vertical = go.GetComponent<VerticalLayoutGroup>();
                if (vertical == null)
                {
                    vertical = go.AddComponent<VerticalLayoutGroup>();
                }

                if (vertical == null)
                {
                    return;
                }

                vertical.enabled = true;
                vertical.spacing = 6f;
                vertical.childAlignment = TextAnchor.UpperCenter;
                vertical.childControlWidth = true;
                vertical.childControlHeight = true;
                vertical.childForceExpandWidth = true;
                vertical.childForceExpandHeight = false;
                vertical.padding = new RectOffset(8, 8, 8, 8);
            }

            var fitter = go.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = go.AddComponent<ContentSizeFitter>();
            }

            if (fitter != null)
            {
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
        }

        public void Hide()
        {
            if (!_open)
            {
                HideImmediate();
                return;
            }

            _open = false;
            _onSelect = null;
            if (!GameUi.IsPaused)
            {
                GameUi.IsBlocking = false;
            }

            if (_dim != null)
            {
                _dim.SetActive(false);
            }

            FindFirstObjectByType<HudUI>()?.RefreshHintVisibility();
            StartCoroutine(UiPanelMotion.FadeOut(_root, 0.12f));
        }

        private void HideImmediate()
        {
            ClearRows();
            if (_root != null)
            {
                _root.SetActive(false);
            }

            if (_dim != null)
            {
                _dim.SetActive(false);
            }

            _open = false;
            _onSelect = null;
        }

        private void ClearRows()
        {
            foreach (var go in _rows)
            {
                if (go != null)
                {
                    Destroy(go);
                }
            }

            _rows.Clear();
            if (_list == null)
            {
                return;
            }

            for (var i = _list.childCount - 1; i >= 0; i--)
            {
                Destroy(_list.GetChild(i).gameObject);
            }
        }

        /// <summary>그림만 — 이름 텍스트 없음. 가격·등급은 색/뱃지로.</summary>
        private GameObject CreateIconTile(int index, string label, bool enabled, Color? tint,
            WeaponRarity? rarity, string iconKind, WeaponId weapon, string detail)
        {
            SplitLabel(label, out _, out var pricePart);

            var go = new GameObject($"ShopTile{index}");
            go.transform.SetParent(_list, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(118f, 132f);

            var le = go.AddComponent<LayoutElement>();
            le.minWidth = 118f;
            le.minHeight = 132f;
            le.preferredWidth = 118f;
            le.preferredHeight = 132f;

            Color accent;
            Color bg;
            if (rarity.HasValue)
            {
                bg = WeaponCatalog.RarityBg(rarity.Value);
                accent = WeaponCatalog.RarityAccent(rarity.Value);
            }
            else if (tint.HasValue)
            {
                bg = Color.Lerp(new Color(0.12f, 0.1f, 0.08f), tint.Value, 0.65f);
                accent = new Color(0.95f, 0.65f, 0.35f);
            }
            else
            {
                bg = new Color(0.14f, 0.12f, 0.1f, 0.98f);
                accent = new Color(0.8f, 0.55f, 0.35f);
            }

            if (!enabled)
            {
                bg = new Color(bg.r * 0.4f, bg.g * 0.4f, bg.b * 0.4f, 0.75f);
            }

            var img = go.AddComponent<Image>();
            img.color = bg;

            var outline = go.AddComponent<Outline>();
            outline.effectColor = enabled
                ? new Color(accent.r, accent.g, accent.b, 0.9f)
                : new Color(0.3f, 0.28f, 0.25f, 0.4f);
            outline.effectDistance = new Vector2(2f, -2f);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.interactable = enabled;
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 1.1f, 0.95f, 1f);
            colors.pressedColor = new Color(0.75f, 0.7f, 0.6f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.55f);
            colors.fadeDuration = 0.06f;
            btn.colors = colors;

            // 큰 아이콘
            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(go.transform, false);
            var iconRt = iconGo.AddComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.5f, 0.55f);
            iconRt.anchorMax = new Vector2(0.5f, 0.55f);
            iconRt.pivot = new Vector2(0.5f, 0.5f);
            iconRt.anchoredPosition = Vector2.zero;
            iconRt.sizeDelta = new Vector2(78f, 78f);
            var iconBg = iconGo.AddComponent<Image>();
            iconBg.color = new Color(0.05f, 0.04f, 0.035f, 0.85f);
            iconBg.raycastTarget = false;

            var spriteGo = new GameObject("Sprite");
            spriteGo.transform.SetParent(iconGo.transform, false);
            var sRt = spriteGo.AddComponent<RectTransform>();
            sRt.anchorMin = Vector2.zero;
            sRt.anchorMax = Vector2.one;
            sRt.offsetMin = new Vector2(4f, 4f);
            sRt.offsetMax = new Vector2(-4f, -4f);
            var spriteImg = spriteGo.AddComponent<Image>();
            spriteImg.sprite = UiItemIcon.ForShopKind(iconKind ?? "box", weapon);
            spriteImg.preserveAspect = true;
            spriteImg.color = enabled ? Color.white : new Color(1f, 1f, 1f, 0.35f);
            spriteImg.raycastTarget = false;

            // 하단 가격만 (짧은 숫자)
            if (!string.IsNullOrEmpty(pricePart))
            {
                var priceBg = new GameObject("PriceBg");
                priceBg.transform.SetParent(go.transform, false);
                var pRt = priceBg.AddComponent<RectTransform>();
                pRt.anchorMin = new Vector2(0.5f, 0f);
                pRt.anchorMax = new Vector2(0.5f, 0f);
                pRt.pivot = new Vector2(0.5f, 0f);
                pRt.anchoredPosition = new Vector2(0f, 6f);
                pRt.sizeDelta = new Vector2(96f, 22f);
                var pImg = priceBg.AddComponent<Image>();
                pImg.color = new Color(0.08f, 0.06f, 0.04f, 0.92f);
                pImg.raycastTarget = false;
                var price = RuntimeUiFactory.CreateText(priceBg.transform, "Price", pricePart, 13,
                    Vector2.zero, new Vector2(90f, 20f), TextAnchor.MiddleCenter,
                    enabled ? new Color(1f, 0.85f, 0.45f) : new Color(0.45f, 0.4f, 0.35f));
                price.fontStyle = FontStyle.Bold;
                price.raycastTarget = false;
            }

            // 호버 시 상단에 상세 설명
            var hover = go.AddComponent<ShopTileHover>();
            hover.Setup(detail, _status, () => _statusBaseline, _prompt, enabled);

            var captured = index;
            if (enabled)
            {
                btn.onClick.AddListener(() => Select(captured));
            }

            return go;
        }

        private GameObject CreateTextRow(int index, string label, bool enabled, Color? tint)
        {
            var go = new GameObject($"ForgeRow{index}");
            go.transform.SetParent(_list, false);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 46f;
            le.preferredHeight = 46f;
            le.flexibleWidth = 1f;

            var img = go.AddComponent<Image>();
            if (tint.HasValue)
            {
                var c = tint.Value;
                img.color = enabled ? c : new Color(c.r * 0.5f, c.g * 0.5f, c.b * 0.5f, 0.7f);
            }
            else
            {
                img.color = enabled
                    ? new Color(0.13f, 0.11f, 0.09f, 0.98f)
                    : new Color(0.08f, 0.07f, 0.06f, 0.75f);
            }

            var outline = go.AddComponent<Outline>();
            outline.effectColor = enabled
                ? new Color(0.72f, 0.55f, 0.35f, 0.5f)
                : new Color(0.3f, 0.28f, 0.25f, 0.3f);
            outline.effectDistance = new Vector2(1f, -1f);

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.interactable = enabled;
            var colors = btn.colors;
            colors.highlightedColor = new Color(1.12f, 1.06f, 0.92f, 1f);
            colors.pressedColor = new Color(0.82f, 0.78f, 0.7f, 1f);
            colors.disabledColor = new Color(0.65f, 0.65f, 0.65f, 0.55f);
            btn.colors = colors;

            var body = RuntimeUiFactory.CreateText(go.transform, "Label", label, 15,
                new Vector2(20f, 0f), new Vector2(520f, 36f), TextAnchor.MiddleLeft,
                enabled ? new Color(0.93f, 0.9f, 0.85f) : new Color(0.48f, 0.46f, 0.43f));
            var bodyRt = body.rectTransform;
            bodyRt.anchorMin = new Vector2(0f, 0f);
            bodyRt.anchorMax = new Vector2(1f, 1f);
            bodyRt.offsetMin = new Vector2(16f, 4f);
            bodyRt.offsetMax = new Vector2(-16f, -4f);

            var captured = index;
            if (enabled)
            {
                btn.onClick.AddListener(() => Select(captured));
            }

            return go;
        }

        private static void SplitLabel(string label, out string name, out string price)
        {
            name = label ?? "";
            price = "";
            if (string.IsNullOrEmpty(label))
            {
                return;
            }

            var idx = label.LastIndexOf(" · ", System.StringComparison.Ordinal);
            if (idx < 0)
            {
                return;
            }

            var right = label.Substring(idx + 3).Trim();
            if (right.EndsWith("G") || right.StartsWith("Lv") || right.Contains("상한") || right.Contains("완료"))
            {
                name = label.Substring(0, idx).Trim();
                price = right;
            }
        }

        private void Select(int index)
        {
            if (!_open || index < 1 || index >= _enabled.Length || !_enabled[index])
            {
                return;
            }

            CombatAudio.UiClick();
            // 클릭 이벤트 처리 중에 타일을 Destroy하면 EventSystem NRE 발생 → 다음 프레임에 실행
            StartCoroutine(InvokeSelectNextFrame(index));
        }

        private IEnumerator InvokeSelectNextFrame(int index)
        {
            yield return null;
            if (!_open)
            {
                yield break;
            }

            _onSelect?.Invoke(index);
        }

        private void Update()
        {
            if (!_open)
            {
                return;
            }

            if (GameInput.AttackDown)
            {
                Hide();
            }
        }

        private static string BuildDetail(string iconKind, WeaponId weapon, string label, bool enabled)
        {
            var lockNote = enabled ? "" : "\n※ 지금은 구매할 수 없다 (골드·레벨 조건 확인).";
            switch (iconKind)
            {
                case "weapon":
                    return WeaponCatalog.FullDescription(weapon) + lockNote;
                case "heal":
                    return
                        "회복 키트  ·  15G\n" +
                        "대장간의 응급 연고와 붕대 세트. 사용 즉시 체력을 최대치까지 완전히 회복한다.\n" +
                        "던전 들어가기 전, 또는 큰 전투 후 상처를 달래 둘 때 요긴하다." + lockNote;
                case "potion":
                    return
                        "회복 포션  ·  20G\n" +
                        "휴대용 녹빛 포션. 가방에 넣어 두고 Q 키(또는 가방)로 언제든 마실 수 있다.\n" +
                        "최대 체력의 약 40%를 즉시 회복한다. 여러 개 소지해 두면 탐색이 한결 안전해진다." + lockNote;
                case "luck":
                    return
                        "행운 부적  ·  50G\n" +
                        "희미한 보라빛 부적. 탐험 중 자동으로 발동해 골드 획득 등 작은 행운을 더한다.\n" +
                        "최대 9개까지 충전할 수 있으며, 클리어와 보물 타이밍에 도움이 된다." + lockNote;
                case "forge":
                    return
                        "장착 연마 (대장간)\n" +
                        $"지금 장착한 무기만 연마한다. 단계(+N) 1마다 보너스 +{WeaponCatalog.UpgradeAtkPerLevel}.\n" +
                        "가방(B)에서는 인벤 무기를 골라 강화·합성할 수 있다.\n" +
                        $"보통 +1, 가끔 대성공(+2)·완벽(+3). 비용: {label}" + lockNote;
                case "precision":
                    return
                        "장착 정밀 (대장간)\n" +
                        $"장착 무기 전용. 비용 약 1.65배. 항상 +1 확정 + 템퍼 ⋆+1 (최대 ⋆{WeaponCatalog.MaxTemper}).\n" +
                        $"템퍼 1마다 보너스 +{WeaponCatalog.TemperAtkPerLevel}. 인벤 강화는 가방에서.\n" +
                        $"템퍼가 최대면 사용할 수 없다. 비용: {label}" + lockNote;
                case "sigil":
                    return
                        "무기 각인 (대장간 전용)\n" +
                        "장착 무기가 강화 상한일 때 고유 각인을 새긴다. 가방에서는 각인할 수 없다.\n" +
                        $"{label}\n각인은 그 무기의 개성을 완성하는 마지막 손질이다." + lockNote;
                case "skill":
                    return "스킬은 무기에 포함되어 있습니다.\n무기를 장착하면 그 무기의 스킬(R)이 자동으로 정해집니다." +
                           lockNote;
                default:
                    return (label ?? "") + lockNote;
            }
        }

        private sealed class ShopTileHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
        {
            private string _detail;
            private Text _status;
            private System.Func<string> _baseline;
            private Text _prompt;
            private bool _enabled;
            private string _promptFallback;

            public void Setup(string detail, Text status, System.Func<string> baseline, Text prompt, bool enabled)
            {
                _detail = detail;
                _status = status;
                _baseline = baseline;
                _prompt = prompt;
                _enabled = enabled;
                _promptFallback = prompt != null ? prompt.text : "";
            }

            public void OnPointerEnter(PointerEventData eventData)
            {
                if (_status != null && !string.IsNullOrEmpty(_detail))
                {
                    _status.text = _detail;
                }

                if (_prompt != null)
                {
                    _prompt.text = _enabled ? "클릭하여 구매" : "구매 불가";
                }
            }

            public void OnPointerExit(PointerEventData eventData)
            {
                if (_status != null)
                {
                    _status.text = _baseline?.Invoke() ?? "";
                }

                if (_prompt != null)
                {
                    _prompt.text = string.IsNullOrEmpty(_promptFallback)
                        ? "아이콘에 올리면 상세 설명  ·  클릭으로 구매  ·  Space/Esc 닫기"
                        : _promptFallback;
                }
            }
        }
    }
}
