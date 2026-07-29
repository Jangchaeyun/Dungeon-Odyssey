using System.Collections.Generic;
using DungeonOdyssey.Combat;
using DungeonOdyssey.Core;
using DungeonOdyssey.Player;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonOdyssey.UI
{
    /// <summary>
    /// 하단 무기 핫바 — 클릭/휠/Shift+숫자로 즉시 교체.
    /// </summary>
    public class WeaponHotbarUI : MonoBehaviour
    {
        private RectTransform _root;
        private PlayerStats _stats;
        private readonly List<SlotView> _slots = new();
        private readonly List<WeaponId> _owned = new();
        private Text _hint;
        private int _lastMask = -1;
        private WeaponId _lastEquipped = (WeaponId)(-1);

        private sealed class SlotView
        {
            public GameObject Go;
            public Button Button;
            public Image Bg;
            public Image Accent;
            public Text Index;
            public Text Name;
            public WeaponId Id;
            public int SlotIndex;
        }

        public static WeaponHotbarUI Build(Transform canvasRoot)
        {
            var host = new GameObject("WeaponHotbar");
            host.transform.SetParent(canvasRoot, false);
            var ui = host.AddComponent<WeaponHotbarUI>();
            ui.Construct();
            return ui;
        }

        private void Construct()
        {
            var rt = gameObject.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 58f);
            rt.sizeDelta = new Vector2(720f, 56f);
            _root = rt;

            _hint = MakeText(transform, "HotbarHint", "상단 가방 아이콘  ·  휠 · V  ·  Shift+1~9", 10,
                new Vector2(0f, 28f), new Vector2(420f, 16f),
                new Color(0.55f, 0.6f, 0.68f, 0.85f));
            var hintRt = _hint.rectTransform;
            hintRt.anchorMin = new Vector2(0.5f, 0f);
            hintRt.anchorMax = new Vector2(0.5f, 0f);
            hintRt.pivot = new Vector2(0.5f, 0f);
            hintRt.anchoredPosition = new Vector2(0f, 52f);

            // 게임 화면에서는 숨김 — 무기는 상단 가방 / 휠·V 단축키로 교체
            gameObject.SetActive(false);
        }

        public void Bind(PlayerStats stats)
        {
            if (_stats != null)
            {
                _stats.OnMetaChanged -= HandleMeta;
            }

            _stats = stats;
            if (_stats != null)
            {
                _stats.OnMetaChanged += HandleMeta;
            }

            _lastMask = -1;
        }

        private void OnDestroy()
        {
            if (_stats != null)
            {
                _stats.OnMetaChanged -= HandleMeta;
            }
        }

        private void HandleMeta()
        {
        }

        private void Rebuild()
        {
            if (_stats == null || _root == null)
            {
                return;
            }

            _lastMask = _stats.OwnedWeaponsMask;
            _lastEquipped = _stats.EquippedWeapon;
            WeaponCatalog.FillOwnedList(_lastMask, _owned);

            while (_slots.Count > _owned.Count)
            {
                var last = _slots[_slots.Count - 1];
                _slots.RemoveAt(_slots.Count - 1);
                if (last.Go != null)
                {
                    Destroy(last.Go);
                }
            }

            var count = _owned.Count;
            var slotW = count > 10 ? 58f : 68f;
            var gap = 4f;
            var total = count * slotW + Mathf.Max(0, count - 1) * gap;
            var startX = -total * 0.5f + slotW * 0.5f;

            for (var i = 0; i < count; i++)
            {
                SlotView slot;
                if (i < _slots.Count)
                {
                    slot = _slots[i];
                }
                else
                {
                    slot = CreateSlot(i);
                    _slots.Add(slot);
                }

                var id = _owned[i];
                slot.Id = id;
                slot.SlotIndex = i + 1;
                var def = WeaponCatalog.Get(id);
                var up = _stats.UpgradeOf(id);
                var rarity = WeaponCatalog.GetRarity(id);
                var equipped = id == _lastEquipped;
                slot.Index.text = i < 9 ? $"{i + 1}" : "·";
                slot.Name.text = ShortName(def.Name, up);
                var bg = WeaponCatalog.RarityBg(rarity);
                slot.Bg.color = equipped
                    ? Color.Lerp(bg, new Color(0.25f, 0.45f, 0.5f), 0.35f)
                    : bg;
                slot.Accent.color = WeaponCatalog.RarityAccent(rarity);
                slot.Name.color = equipped
                    ? new Color(0.95f, 0.96f, 0.98f, 1f)
                    : new Color(0.85f, 0.88f, 0.9f, 1f);
                slot.Index.color = WeaponCatalog.RarityAccent(rarity);
                slot.Go.GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(startX + i * (slotW + gap), 0f);
                slot.Go.GetComponent<RectTransform>().sizeDelta = new Vector2(slotW, 48f);
            }

            if (_hint != null)
            {
                _hint.gameObject.SetActive(count > 1);
            }
        }

        private static string ShortName(string name, int upgrade)
        {
            var s = name;
            if (s.Length > 5)
            {
                s = s.Substring(0, 4) + "…";
            }

            return upgrade > 0 ? $"{s}+{upgrade}" : s;
        }

        private SlotView CreateSlot(int index)
        {
            var go = new GameObject($"WepSlot_{index}");
            go.transform.SetParent(_root, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(68f, 48f);

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.09f, 0.12f, 0.88f);

            var accentGo = new GameObject("Accent");
            accentGo.transform.SetParent(go.transform, false);
            var accentRt = accentGo.AddComponent<RectTransform>();
            accentRt.anchorMin = new Vector2(0f, 0f);
            accentRt.anchorMax = new Vector2(1f, 0f);
            accentRt.pivot = new Vector2(0.5f, 0f);
            accentRt.sizeDelta = new Vector2(0f, 3f);
            accentRt.anchoredPosition = Vector2.zero;
            var accent = accentGo.AddComponent<Image>();
            accent.color = new Color(0.35f, 0.85f, 0.95f, 0.9f);
            accent.raycastTarget = false;

            var idx = MakeText(go.transform, "Idx", $"{index + 1}", 10,
                new Vector2(0f, 14f), new Vector2(60f, 14f),
                new Color(0.55f, 0.85f, 0.95f, 1f));
            var name = MakeText(go.transform, "Name", "검", 11,
                new Vector2(0f, -2f), new Vector2(62f, 18f),
                new Color(0.9f, 0.9f, 0.92f, 1f));

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.25f, 0.35f, 0.42f, 1f);
            colors.pressedColor = new Color(0.2f, 0.45f, 0.5f, 1f);
            btn.colors = colors;

            var captured = index;
            btn.onClick.AddListener(() => OnClickSlot(captured));

            return new SlotView
            {
                Go = go,
                Button = btn,
                Bg = bg,
                Accent = accent,
                Index = idx,
                Name = name,
                SlotIndex = index + 1
            };
        }

        private void OnClickSlot(int listIndex)
        {
            if (_stats == null || GameUi.IsBlocking || GameUi.IsPaused)
            {
                return;
            }

            if (listIndex < 0 || listIndex >= _slots.Count)
            {
                return;
            }

            var id = _slots[listIndex].Id;
            if (_stats.TryEquipWeapon(id, out var msg))
            {
                CombatAudio.UiClick();
                FindFirstObjectByType<HudUI>()?.SetHint(msg);
            }
            else
            {
                FindFirstObjectByType<HudUI>()?.SetHint(msg);
            }
        }

        private static Color ElementColor(WeaponElement el) => el switch
        {
            WeaponElement.Flame => new Color(1f, 0.45f, 0.2f, 1f),
            WeaponElement.Frost => new Color(0.45f, 0.8f, 1f, 1f),
            WeaponElement.Storm => new Color(0.7f, 0.85f, 1f, 1f),
            WeaponElement.Void => new Color(0.7f, 0.4f, 0.9f, 1f),
            WeaponElement.Holy => new Color(1f, 0.9f, 0.55f, 1f),
            WeaponElement.Nature => new Color(0.45f, 0.85f, 0.5f, 1f),
            WeaponElement.Blood => new Color(0.9f, 0.25f, 0.35f, 1f),
            WeaponElement.Earth => new Color(0.75f, 0.6f, 0.4f, 1f),
            WeaponElement.Steel => new Color(0.75f, 0.8f, 0.9f, 1f),
            _ => new Color(0.35f, 0.85f, 0.95f, 1f)
        };

        private static Text MakeText(Transform parent, string name, string content, int size,
            Vector2 pos, Vector2 rect, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = rect;
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                        ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.text = content;
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }
    }
}
