using DungeonOdyssey.Combat;
using DungeonOdyssey.Core;
using DungeonOdyssey.Town;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DungeonOdyssey.UI
{
    /// <summary>화면 상단 아이콘 — 클릭으로 가방·상점 패널 표시.</summary>
    public class TopMenuIconsUI : MonoBehaviour
    {
        private RectTransform _root;
        private GameObject _shopGo;
        private Image _bagBg;
        private Image _shopBg;
        private Image _questBg;
        private Image _bagAccent;
        private Image _shopAccent;
        private Image _questAccent;
        private Outline _bagOutline;
        private Outline _shopOutline;
        private Outline _questOutline;
        private Color _bagIdle;
        private Color _bagActive;
        private Color _shopIdle;
        private Color _shopActiveCol;
        private Color _questIdle;
        private Color _questActive;
        private Color _bagAccentIdle;
        private Color _bagAccentActive;
        private Color _shopAccentIdle;
        private Color _shopAccentActive;
        private Color _questAccentIdle;
        private Color _questAccentActive;
        private bool _bagOpen;
        private bool _shopOpen;
        private bool _questOpen;

        public static TopMenuIconsUI Build(Transform canvasRoot)
        {
            var host = new GameObject("TopMenuIcons");
            host.transform.SetParent(canvasRoot, false);
            var ui = host.AddComponent<TopMenuIconsUI>();
            ui.Construct();
            return ui;
        }

        private void Construct()
        {
            _root = gameObject.GetComponent<RectTransform>();
            if (_root == null)
            {
                _root = gameObject.AddComponent<RectTransform>();
            }

            _root.anchorMin = new Vector2(1f, 1f);
            _root.anchorMax = new Vector2(1f, 1f);
            _root.pivot = new Vector2(1f, 1f);
            _root.anchoredPosition = new Vector2(-16f, -12f);
            _root.sizeDelta = new Vector2(280f, 72f);

            // 쿨 글래스 타일 — 아이스 악센트
            _shopIdle = new Color(0.12f, 0.13f, 0.16f, 0.9f);
            _shopActiveCol = new Color(0.18f, 0.28f, 0.3f, 0.96f);
            _shopAccentIdle = new Color(0.45f, 0.78f, 0.75f, 0.75f);
            _shopAccentActive = new Color(0.55f, 0.95f, 0.9f, 1f);

            _bagIdle = new Color(0.1f, 0.12f, 0.15f, 0.9f);
            _bagActive = new Color(0.14f, 0.26f, 0.3f, 0.96f);
            _bagAccentIdle = new Color(0.4f, 0.75f, 0.8f, 0.75f);
            _bagAccentActive = new Color(0.55f, 0.92f, 0.95f, 1f);

            _questIdle = new Color(0.1f, 0.12f, 0.14f, 0.9f);
            _questActive = new Color(0.16f, 0.24f, 0.28f, 0.96f);
            _questAccentIdle = new Color(0.55f, 0.78f, 0.7f, 0.75f);
            _questAccentActive = new Color(0.65f, 0.95f, 0.85f, 1f);

            _shopGo = CreateMenuTile(transform, "ShopIcon", "상점", "shop",
                new Vector2(-188f, 0f),
                _shopIdle, _shopAccentIdle,
                OnShopClicked,
                out _shopBg, out _shopAccent, out _shopOutline);

            CreateMenuTile(transform, "QuestIcon", "퀘스트", "quest",
                new Vector2(-94f, 0f),
                _questIdle, _questAccentIdle,
                OnQuestClicked,
                out _questBg, out _questAccent, out _questOutline);

            CreateMenuTile(transform, "BagIcon", "가방", "bag",
                new Vector2(0f, 0f),
                _bagIdle, _bagAccentIdle,
                OnBagClicked,
                out _bagBg, out _bagAccent, out _bagOutline);
        }

        private void LateUpdate()
        {
            if (_root != null)
            {
                _root.SetAsLastSibling();
            }

            var inTown = SceneManager.GetActiveScene().name == SceneNames.Town;
            if (_shopGo != null && _shopGo.activeSelf != inTown)
            {
                _shopGo.SetActive(inTown);
            }

            var bag = FindFirstObjectByType<BagUI>();
            var shop = FindFirstObjectByType<ShopInteractable>();
            var quest = FindFirstObjectByType<QuestLogUI>();
            _bagOpen = bag != null && bag.IsOpen;
            _shopOpen = shop != null && shop.IsShopUiOpen;
            _questOpen = quest != null && quest.IsOpen;
            RefreshHighlight();
        }

        private void RefreshHighlight()
        {
            ApplyTileState(_bagBg, _bagAccent, _bagOutline, _bagOpen,
                _bagIdle, _bagActive, _bagAccentIdle, _bagAccentActive);
            ApplyTileState(_questBg, _questAccent, _questOutline, _questOpen,
                _questIdle, _questActive, _questAccentIdle, _questAccentActive);
            if (_shopGo != null && _shopGo.activeSelf)
            {
                ApplyTileState(_shopBg, _shopAccent, _shopOutline, _shopOpen,
                    _shopIdle, _shopActiveCol, _shopAccentIdle, _shopAccentActive);
            }
        }

        private static void ApplyTileState(Image bg, Image accent, Outline outline, bool active,
            Color idle, Color activeCol, Color accentIdle, Color accentActive)
        {
            if (bg != null)
            {
                bg.color = active ? activeCol : idle;
            }

            if (accent != null)
            {
                accent.color = active ? accentActive : accentIdle;
            }

            if (outline != null)
            {
                var c = active ? accentActive : accentIdle;
                outline.effectColor = new Color(c.r, c.g, c.b, active ? 0.75f : 0.4f);
                outline.effectDistance = active ? new Vector2(1.6f, -1.6f) : new Vector2(1.1f, -1.1f);
            }
        }

        private void OnBagClicked()
        {
            if (GameUi.IsPaused)
            {
                return;
            }

            CombatAudio.UiClick();
            FindFirstObjectByType<QuestLogUI>()?.Close();
            var forge = FindFirstObjectByType<ForgeShopUI>();
            if (forge != null && forge.IsOpen)
            {
                forge.Hide();
            }

            var bag = FindFirstObjectByType<BagUI>();
            if (bag == null)
            {
                return;
            }

            if (bag.IsOpen)
            {
                bag.Close();
            }
            else
            {
                bag.Open();
            }
        }

        private void OnQuestClicked()
        {
            if (GameUi.IsPaused)
            {
                return;
            }

            CombatAudio.UiClick();
            var bag = FindFirstObjectByType<BagUI>();
            if (bag != null && bag.IsOpen)
            {
                bag.Close();
            }

            var forge = FindFirstObjectByType<ForgeShopUI>();
            if (forge != null && forge.IsOpen)
            {
                forge.Hide();
            }

            var quest = FindFirstObjectByType<QuestLogUI>();
            if (quest == null)
            {
                FindFirstObjectByType<HudUI>()?.SetHint("퀘스트 창을 열 수 없다");
                return;
            }

            quest.Toggle();
        }

        private void OnShopClicked()
        {
            if (GameUi.IsPaused)
            {
                return;
            }

            if (SceneManager.GetActiveScene().name != SceneNames.Town)
            {
                FindFirstObjectByType<HudUI>()?.SetHint("상점은 마을에서만 이용할 수 있다");
                return;
            }

            CombatAudio.UiClick();
            FindFirstObjectByType<QuestLogUI>()?.Close();
            var bag = FindFirstObjectByType<BagUI>();
            if (bag != null && bag.IsOpen)
            {
                bag.Close();
            }

            var shop = FindFirstObjectByType<ShopInteractable>();
            if (shop == null)
            {
                FindFirstObjectByType<HudUI>()?.SetHint("상점을 찾을 수 없다");
                return;
            }

            shop.ToggleFromUi();
        }

        /// <summary>아이콘 위 + 라벨 아래. 프레임이 아이콘을 뚫고 나오지 않는 단순 타일.</summary>
        private static GameObject CreateMenuTile(Transform parent, string name, string label, string iconKind,
            Vector2 anchoredPos, Color bg, Color accent,
            UnityEngine.Events.UnityAction onClick,
            out Image bgImage, out Image accentBar, out Outline outline)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(86f, 68f);

            bgImage = go.AddComponent<Image>();
            bgImage.color = bg;

            outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(accent.r, accent.g, accent.b, 0.4f);
            outline.effectDistance = new Vector2(1.1f, -1.1f);

            // 하단 액센트 한 줄만 (아이콘과 겹치지 않음)
            var bar = new GameObject("Accent");
            bar.transform.SetParent(go.transform, false);
            var barRt = bar.AddComponent<RectTransform>();
            barRt.anchorMin = new Vector2(0.15f, 0f);
            barRt.anchorMax = new Vector2(0.85f, 0f);
            barRt.pivot = new Vector2(0.5f, 0f);
            barRt.sizeDelta = new Vector2(0f, 2.5f);
            barRt.anchoredPosition = new Vector2(0f, 4f);
            accentBar = bar.AddComponent<Image>();
            accentBar.color = accent;
            accentBar.raycastTarget = false;

            // 아이콘 — 타일 안쪽에만
            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(go.transform, false);
            var iconRt = iconGo.AddComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.5f, 0.5f);
            iconRt.anchorMax = new Vector2(0.5f, 0.5f);
            iconRt.anchoredPosition = new Vector2(0f, 8f);
            iconRt.sizeDelta = new Vector2(36f, 36f);
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = UiItemIcon.ForShopKind(iconKind);
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            iconImg.color = new Color(1f, 1f, 1f, 0.95f);

            var labelText = RuntimeUiFactory.CreateText(go.transform, "Label", label, 12,
                new Vector2(0f, -22f), new Vector2(78f, 18f), TextAnchor.MiddleCenter,
                new Color(0.92f, 0.88f, 0.8f));
            labelText.fontStyle = FontStyle.Bold;
            labelText.raycastTarget = false;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = bgImage;
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.06f, 1.02f, 1f);
            colors.pressedColor = new Color(0.85f, 0.82f, 0.78f, 1f);
            colors.fadeDuration = 0.08f;
            btn.colors = colors;
            btn.onClick.AddListener(onClick);
            return go;
        }
    }
}
