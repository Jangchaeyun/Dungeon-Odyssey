using System.Collections.Generic;
using DungeonOdyssey.Combat;
using DungeonOdyssey.Core;
using DungeonOdyssey.Player;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonOdyssey.UI
{
    /// <summary>
    /// 가방 — 슬롯 기반 인벤토리. 아이콘 그리드 + 우측 요약/스킬 + 하단 상세·액션.
    /// 열기: 상단 [가방] 아이콘 (단축키 B/I)
    /// </summary>
    public class BagUI : MonoBehaviour
    {
        private const float SlotCellW = 84f;
        private const float SlotCellH = 92f;
        private const float SlotGap = 7f;
        private const int SlotCols = 5;
        private const int LayoutVersion = 27;
        private static readonly Color BtnSell = new(0.42f, 0.22f, 0.24f, 1f);
        private static readonly Color BtnPrecision = new(0.22f, 0.3f, 0.36f, 1f);

        private static readonly Color PanelBg = new(0.06f, 0.07f, 0.09f, 0.94f);
        private static readonly Color EmberGold = new(0.4f, 0.85f, 0.82f, 0.55f);
        private static readonly Color WellBg = new(0.04f, 0.05f, 0.065f, 0.92f);
        private static readonly Color WellDash = new(0.35f, 0.55f, 0.55f, 0.4f);
        private static readonly Color BtnForge = new(0.18f, 0.32f, 0.34f, 1f);
        private static readonly Color BtnSynth = new(0.22f, 0.28f, 0.36f, 1f);
        private static readonly Color BtnEquip = new(0.18f, 0.36f, 0.3f, 1f);
        private static readonly Color BtnClose = new(0.14f, 0.15f, 0.18f, 1f);
        private static readonly Color BtnDisabled = new(0.1f, 0.11f, 0.13f, 0.9f);
        private static readonly Color SynthEligible = new(0.35f, 0.55f, 0.4f, 1f);
        private static readonly Color OverlayBg = new(0.04f, 0.05f, 0.07f, 0.95f);

        private GameObject _root;
        private GameObject _dim;
        private GameObject _panel;
        private Transform _weaponGrid;
        private Transform _itemGrid;
        private Transform _skillGrid;
        private ScrollRect _slotScroll;
        private ScrollRect _skillScroll;
        private Text _detailTitle;
        private Text _detailBody;
        private Text _headerMeta;
        private Text _equipBtnLabel;
        private Button _equipBtn;
        private Image _equipBtnBg;
        private Text _forgeBtnLabel;
        private Button _forgeBtn;
        private Image _forgeBtnBg;
        private Text _synthBtnLabel;
        private Button _synthBtn;
        private Image _synthBtnBg;
        private Text _sellBtnLabel;
        private Button _sellBtn;
        private Image _sellBtnBg;
        private Text _forgeEffectText;
        private Text _actionGuide;
        private PlayerStats _stats;
        private bool _open;
        private int? _selectedSlot;
        /// <summary>가방 스킬 목록에서 고른 스킬 출처 무기. null이면 무기/아이템 선택 모드.</summary>
        private WeaponId? _skillPickWeapon;
        private string _lastForgeEffect = "";
        private int _builtLayoutVersion;
        private readonly List<GameObject> _weaponCards = new();
        private readonly List<GameObject> _itemCards = new();
        private readonly List<GameObject> _skillCards = new();
        private readonly Dictionary<int, Image> _slotCardBgs = new();

        // 강화 확인 오버레이
        private GameObject _forgeOverlay;
        private Text _forgeOverlayBody;
        private Text _forgeCountLabel;
        private Button _forgeConfirmBtn;
        private Button _forgePrecisionBtn;
        private Button _forgeMaxBtn;
        private int _forgeCount = 1;

        // 강화 안내 오버레이
        private GameObject _forgeHelpOverlay;

        // 강화 성공 · 탄생 연출
        private GameObject _forgeResultOverlay;
        private Text _forgeResultTitle;
        private Text _forgeResultSubtitle;
        private Text _forgeResultBefore;
        private Text _forgeResultAfter;
        private Text _forgeResultStats;
        private Image _forgeResultIcon;

        // 합성 마법사 오버레이
        private GameObject _synthOverlay;
        private int? _synthSlotA;
        private int? _synthSlotB;
        private Text _synthCardALabel;
        private Text _synthCardBLabel;
        private Image _synthIconA;
        private Image _synthIconB;
        private Text _synthGuideText;
        private Text _synthErrorText;
        private Button _synthConfirmBtn;
        private Image _synthConfirmBg;

        private bool IsSynthOverlayOpen => _synthOverlay != null && _synthOverlay.activeSelf;
        private bool IsForgeOverlayOpen => _forgeOverlay != null && _forgeOverlay.activeSelf;
        private bool IsForgeHelpOpen =>
            _forgeHelpOverlay != null && _forgeHelpOverlay.activeSelf;
        private bool IsForgeResultOpen =>
            _forgeResultOverlay != null && _forgeResultOverlay.activeSelf;

        public bool IsOpen => _open;

        public static BagUI Build(Transform canvasRoot)
        {
            var host = canvasRoot.gameObject.GetComponent<BagUI>();
            if (host == null)
            {
                host = canvasRoot.gameObject.AddComponent<BagUI>();
            }

            host.Construct(canvasRoot);
            return host;
        }

        private void Construct(Transform canvasRoot)
        {
            // 레이아웃 버전이 바뀌면 패널 재생성 (스킬/버튼 개편 반영)
            if (_root != null && _builtLayoutVersion == LayoutVersion)
            {
                return;
            }

            if (_dim != null)
            {
                Destroy(_dim);
            }

            if (_panel != null)
            {
                Destroy(_panel);
            }

            _root = null;
            _weaponGrid = null;
            _itemGrid = null;
            _skillGrid = null;
            _slotScroll = null;
            _skillScroll = null;
            _forgeOverlay = null;
            _forgeOverlayBody = null;
            _forgeCountLabel = null;
            _forgeConfirmBtn = null;
            _forgePrecisionBtn = null;
            _forgeMaxBtn = null;
            _forgeCount = 1;
            _forgeHelpOverlay = null;
            _forgeResultOverlay = null;
            _synthOverlay = null;
            _synthSlotA = null;
            _synthSlotB = null;
            _weaponCards.Clear();
            _itemCards.Clear();
            _skillCards.Clear();
            _slotCardBgs.Clear();

            _dim = CreateDim(canvasRoot);
            _dim.SetActive(false);

            // 상단 헤더 / 중단 슬롯·스킬 / 하단 상세 / 최하단 버튼 — 구역이 겹치지 않게
            _panel = CreatePanel(canvasRoot, "BagPanel", new Vector2(900f, 640f));
            _panel.SetActive(false);
            _root = _panel;
            _builtLayoutVersion = LayoutVersion;

            var title = MakeText(_panel.transform, "Title", "가방", 26, new Vector2(0f, 288f),
                new Vector2(420f, 34f), new Color(0.95f, 0.82f, 0.55f), FontStyle.Bold);
            title.alignment = TextAnchor.MiddleCenter;

            _headerMeta = MakeText(_panel.transform, "Meta", "가방 0/10", 12, new Vector2(0f, 258f),
                new Vector2(840f, 22f), new Color(0.7f, 0.58f, 0.42f));
            _headerMeta.alignment = TextAnchor.MiddleCenter;

            MakeText(_panel.transform, "Hint",
                "슬롯 확장 Lv5/10/20 · B 닫기", 11,
                new Vector2(0f, 236f), new Vector2(520f, 18f), new Color(0.48f, 0.4f, 0.32f))
                .alignment = TextAnchor.MiddleCenter;

            MakeText(_panel.transform, "WepLabel", "가방 슬롯", 13,
                new Vector2(-160f, 210f),
                new Vector2(540f, 20f), new Color(0.9f, 0.72f, 0.45f)).alignment = TextAnchor.MiddleLeft;

            // 슬롯 영역 하단이 상세 패널과 겹치지 않도록 높이 제한
            _weaponGrid = CreateScrollArea(_panel.transform, "WeaponScroll", new Vector2(-160f, 40f),
                new Vector2(540f, 300f), out _slotScroll);

            MakeText(_panel.transform, "EquipLabel", "장착 중", 13, new Vector2(300f, 218f),
                new Vector2(280f, 20f), new Color(0.9f, 0.72f, 0.45f)).alignment = TextAnchor.MiddleLeft;

            // 라벨(218) 아래 · 스킬(95) 위 — 카드가 라벨을 덮지 않게
            _itemGrid = CreateContentRoot(_panel.transform, "ItemGrid", new Vector2(300f, 152f),
                new Vector2(280f, 96f));

            MakeText(_panel.transform, "SkillLabel", "무기 스킬", 13, new Vector2(300f, 88f),
                new Vector2(280f, 20f), new Color(0.9f, 0.72f, 0.45f)).alignment = TextAnchor.MiddleLeft;

            _skillGrid = CreateSkillList(_panel.transform, "SkillList", new Vector2(300f, -16f),
                new Vector2(280f, 188f), out _skillScroll);

            // 상세: 중단 콘텐츠(-110) 아래, 버튼 위 — 단독 띠
            var detailBg = new GameObject("DetailBg");
            detailBg.transform.SetParent(_panel.transform, false);
            var dRt = detailBg.AddComponent<RectTransform>();
            dRt.anchoredPosition = new Vector2(0f, -168f);
            dRt.sizeDelta = new Vector2(860f, 104f);
            var dImg = detailBg.AddComponent<Image>();
            dImg.color = new Color(0.06f, 0.045f, 0.035f, 0.96f);
            dImg.raycastTarget = false;
            var dOutline = detailBg.AddComponent<Outline>();
            dOutline.effectColor = new Color(0.55f, 0.35f, 0.15f, 0.35f);
            dOutline.effectDistance = new Vector2(1f, -1f);

            _detailTitle = MakeText(detailBg.transform, "DetailTitle", "슬롯을 선택하세요", 14,
                new Vector2(0f, 30f), new Vector2(820f, 22f), new Color(0.95f, 0.82f, 0.5f), FontStyle.Bold);
            _detailTitle.alignment = TextAnchor.MiddleLeft;

            _detailBody = MakeText(detailBg.transform, "DetailBody",
                "왼쪽 슬롯을 클릭하면 정보가 표시됩니다.", 11,
                new Vector2(0f, -2f), new Vector2(820f, 48f), new Color(0.72f, 0.62f, 0.5f));
            _detailBody.alignment = TextAnchor.UpperLeft;
            _detailBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            _detailBody.verticalOverflow = VerticalWrapMode.Truncate;
            _detailBody.lineSpacing = 1.05f;

            _forgeEffectText = MakeText(detailBg.transform, "ForgeFx", "", 11,
                new Vector2(0f, -38f), new Vector2(820f, 16f), new Color(1f, 0.78f, 0.35f), FontStyle.Bold);
            _forgeEffectText.alignment = TextAnchor.MiddleLeft;
            _forgeEffectText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _forgeEffectText.verticalOverflow = VerticalWrapMode.Truncate;

            // 안내: 상세와 버튼 사이 한 줄
            _actionGuide = MakeText(_panel.transform, "ActionGuide",
                "가방 · 인벤 강화/합성  |  대장간 · 장착 연마·각인", 11,
                new Vector2(0f, -232f), new Vector2(840f, 18f), new Color(0.85f, 0.7f, 0.45f));
            _actionGuide.alignment = TextAnchor.MiddleCenter;
            _actionGuide.horizontalOverflow = HorizontalWrapMode.Wrap;
            _actionGuide.verticalOverflow = VerticalWrapMode.Truncate;

            var actionBar = new GameObject("ActionBar");
            actionBar.transform.SetParent(_panel.transform, false);
            var abRt = actionBar.AddComponent<RectTransform>();
            abRt.anchoredPosition = new Vector2(0f, -268f);
            abRt.sizeDelta = new Vector2(860f, 44f);
            var abImg = actionBar.AddComponent<Image>();
            abImg.color = new Color(0.07f, 0.055f, 0.04f, 0.95f);
            abImg.raycastTarget = false;
            var abOl = actionBar.AddComponent<Outline>();
            abOl.effectColor = new Color(0.45f, 0.3f, 0.14f, 0.4f);
            abOl.effectDistance = new Vector2(1f, -1f);

            var forgeGo = CreateActionButton(actionBar.transform, "ForgeBtn", "가방 강화",
                new Vector2(-340f, 0f), new Vector2(130f, 34f), BtnForge, OnForgeClicked);
            _forgeBtn = forgeGo.GetComponent<Button>();
            _forgeBtnBg = forgeGo.GetComponent<Image>();
            _forgeBtnLabel = forgeGo.transform.Find("L").GetComponent<Text>();

            var synthGo = CreateActionButton(actionBar.transform, "SynthBtn", "무기 합성",
                new Vector2(-195f, 0f), new Vector2(130f, 34f), BtnSynth, OnSynthClicked);
            _synthBtn = synthGo.GetComponent<Button>();
            _synthBtnBg = synthGo.GetComponent<Image>();
            _synthBtnLabel = synthGo.transform.Find("L").GetComponent<Text>();

            var sellGo = CreateActionButton(actionBar.transform, "SellBtn", "판매",
                new Vector2(-50f, 0f), new Vector2(110f, 34f), BtnSell, OnSellClicked);
            _sellBtn = sellGo.GetComponent<Button>();
            _sellBtnBg = sellGo.GetComponent<Image>();
            _sellBtnLabel = sellGo.transform.Find("L").GetComponent<Text>();

            var equipGo = CreateActionButton(actionBar.transform, "EquipBtn", "장착",
                new Vector2(85f, 0f), new Vector2(110f, 34f), BtnEquip, OnEquipClicked);
            _equipBtn = equipGo.GetComponent<Button>();
            _equipBtnBg = equipGo.GetComponent<Image>();
            _equipBtnLabel = equipGo.transform.Find("L").GetComponent<Text>();

            CreateActionButton(actionBar.transform, "Close", "닫기",
                new Vector2(220f, 0f), new Vector2(100f, 34f), BtnClose, Close);

            BuildForgeOverlay();
            BuildForgeHelpOverlay();
            BuildForgeResultOverlay();
            BuildSynthOverlay();

            SetEquipButtonState(false, "장착");
            SetForgeButtonState(true, "가방 강화");
            SetSynthButtonState(true, "무기 합성");
            SetSellButtonState(false, "판매");
            SetActionGuide("가방 · 인벤 강화/합성  |  마을 대장간 · 장착 연마·각인");
        }

        private void BuildForgeOverlay()
        {
            // 강화는 가방 클릭이 필요 없으므로 배경을 막아 오조작 방지
            _forgeOverlay = CreateOverlayPanel(_panel.transform, "ForgeOverlay",
                new Vector2(580f, 420f), Vector2.zero, blockBackground: true);
            MakeText(_forgeOverlay.transform, "Title", "가방 강화 (인벤)", 22,
                new Vector2(0f, 172f), new Vector2(480f, 32f),
                new Color(0.98f, 0.85f, 0.5f), FontStyle.Bold).alignment = TextAnchor.MiddleCenter;

            CreateActionButton(_forgeOverlay.transform, "Help", "설명",
                new Vector2(230f, 172f), new Vector2(72f, 32f), BtnPrecision, ShowForgeHelp);

            _forgeOverlayBody = MakeText(_forgeOverlay.transform, "Body", "", 12,
                new Vector2(0f, 58f), new Vector2(520f, 150f),
                new Color(0.88f, 0.78f, 0.62f));
            _forgeOverlayBody.alignment = TextAnchor.MiddleCenter;
            _forgeOverlayBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            _forgeOverlayBody.verticalOverflow = VerticalWrapMode.Overflow;
            _forgeOverlayBody.lineSpacing = 1.12f;

            MakeText(_forgeOverlay.transform, "CountHint", "강화 횟수", 12,
                new Vector2(0f, -28f), new Vector2(200f, 20f),
                new Color(0.75f, 0.62f, 0.42f)).alignment = TextAnchor.MiddleCenter;

            CreateActionButton(_forgeOverlay.transform, "Minus5", "-5",
                new Vector2(-195f, -60f), new Vector2(56f, 34f), BtnClose,
                () => AdjustForgeCount(-5));
            CreateActionButton(_forgeOverlay.transform, "Minus1", "-",
                new Vector2(-125f, -60f), new Vector2(50f, 34f), BtnClose,
                () => AdjustForgeCount(-1));

            _forgeCountLabel = MakeText(_forgeOverlay.transform, "Count", "1회", 16,
                new Vector2(-20f, -60f), new Vector2(90f, 34f),
                new Color(0.98f, 0.88f, 0.55f), FontStyle.Bold);
            _forgeCountLabel.alignment = TextAnchor.MiddleCenter;

            CreateActionButton(_forgeOverlay.transform, "Plus1", "+",
                new Vector2(70f, -60f), new Vector2(50f, 34f), BtnForge,
                () => AdjustForgeCount(1));
            CreateActionButton(_forgeOverlay.transform, "Plus5", "+5",
                new Vector2(140f, -60f), new Vector2(56f, 34f), BtnForge,
                () => AdjustForgeCount(5));

            var maxGo = CreateActionButton(_forgeOverlay.transform, "Max", "최대",
                new Vector2(220f, -60f), new Vector2(70f, 34f), BtnPrecision, SetForgeCountToMax);
            _forgeMaxBtn = maxGo.GetComponent<Button>();
            maxGo.transform.Find("L").GetComponent<Text>().fontSize = 13;

            var confirmGo = CreateActionButton(_forgeOverlay.transform, "Confirm", "일반 강화",
                new Vector2(-150f, -150f), new Vector2(140f, 40f), BtnForge,
                () => OnForgeConfirmClicked(false));
            _forgeConfirmBtn = confirmGo.GetComponent<Button>();
            confirmGo.transform.Find("L").GetComponent<Text>().fontSize = 14;

            var precGo = CreateActionButton(_forgeOverlay.transform, "Precision", "정밀 강화",
                new Vector2(10f, -150f), new Vector2(140f, 40f), BtnPrecision,
                () => OnForgeConfirmClicked(true));
            _forgePrecisionBtn = precGo.GetComponent<Button>();
            precGo.transform.Find("L").GetComponent<Text>().fontSize = 14;

            CreateActionButton(_forgeOverlay.transform, "Cancel", "취소",
                new Vector2(160f, -150f), new Vector2(100f, 40f), BtnClose, HideForgeOverlay);

            _forgeOverlay.SetActive(false);
        }

        private void BuildForgeHelpOverlay()
        {
            _forgeHelpOverlay = CreateOverlayPanel(_panel.transform, "ForgeHelpOverlay",
                new Vector2(620f, 520f), Vector2.zero, blockBackground: true);

            MakeText(_forgeHelpOverlay.transform, "Title", WeaponCatalog.ForgeGuideTitle, 20,
                new Vector2(0f, 220f), new Vector2(560f, 30f),
                new Color(0.98f, 0.85f, 0.5f), FontStyle.Bold).alignment = TextAnchor.MiddleCenter;

            var body = MakeText(_forgeHelpOverlay.transform, "Body", WeaponCatalog.ForgeGuideBody, 12,
                new Vector2(0f, -10f), new Vector2(560f, 400f),
                new Color(0.88f, 0.78f, 0.62f));
            body.alignment = TextAnchor.UpperLeft;
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.verticalOverflow = VerticalWrapMode.Truncate;
            body.lineSpacing = 1.12f;

            CreateActionButton(_forgeHelpOverlay.transform, "Ok", "확인",
                new Vector2(0f, -220f), new Vector2(160f, 40f), BtnForge, HideForgeHelp);

            _forgeHelpOverlay.SetActive(false);
        }

        private void ShowForgeHelp()
        {
            if (_forgeHelpOverlay == null)
            {
                return;
            }

            CombatAudio.UiClick();
            _forgeHelpOverlay.SetActive(true);
            _forgeHelpOverlay.transform.SetAsLastSibling();
        }

        private void HideForgeHelp()
        {
            if (_forgeHelpOverlay != null)
            {
                _forgeHelpOverlay.SetActive(false);
            }

            CombatAudio.UiClick();
            if (IsForgeOverlayOpen)
            {
                _forgeOverlay.transform.SetAsLastSibling();
            }
        }

        private void BuildForgeResultOverlay()
        {
            _forgeResultOverlay = CreateOverlayPanel(_panel.transform, "ForgeResultOverlay",
                new Vector2(540f, 360f), Vector2.zero, blockBackground: true);

            _forgeResultTitle = MakeText(_forgeResultOverlay.transform, "Title",
                "강화 성공!", 22,
                new Vector2(0f, 140f), new Vector2(500f, 32f),
                new Color(1f, 0.88f, 0.45f), FontStyle.Bold);
            _forgeResultTitle.alignment = TextAnchor.MiddleCenter;

            _forgeResultSubtitle = MakeText(_forgeResultOverlay.transform, "Sub",
                "무기가 한 단계 더 강해졌습니다", 12,
                new Vector2(0f, 112f), new Vector2(480f, 22f),
                new Color(0.78f, 0.68f, 0.52f));
            _forgeResultSubtitle.alignment = TextAnchor.MiddleCenter;

            // 큰 무기 아이콘
            var iconWell = new GameObject("IconWell");
            iconWell.transform.SetParent(_forgeResultOverlay.transform, false);
            var wellRt = iconWell.AddComponent<RectTransform>();
            wellRt.anchoredPosition = new Vector2(0f, 48f);
            wellRt.sizeDelta = new Vector2(88f, 88f);
            var wellImg = iconWell.AddComponent<Image>();
            wellImg.color = new Color(0.05f, 0.04f, 0.03f, 0.95f);
            wellImg.raycastTarget = false;
            var wellOl = iconWell.AddComponent<Outline>();
            wellOl.effectColor = new Color(0.95f, 0.7f, 0.3f, 0.85f);
            wellOl.effectDistance = new Vector2(2f, -2f);

            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(iconWell.transform, false);
            var iconRt = iconGo.AddComponent<RectTransform>();
            iconRt.anchorMin = Vector2.zero;
            iconRt.anchorMax = Vector2.one;
            iconRt.offsetMin = new Vector2(10f, 10f);
            iconRt.offsetMax = new Vector2(-10f, -10f);
            _forgeResultIcon = iconGo.AddComponent<Image>();
            _forgeResultIcon.preserveAspect = true;
            _forgeResultIcon.raycastTarget = false;

            _forgeResultBefore = MakeText(_forgeResultOverlay.transform, "Before", "+0", 13,
                new Vector2(-110f, -20f), new Vector2(160f, 24f),
                new Color(0.55f, 0.48f, 0.4f));
            _forgeResultBefore.alignment = TextAnchor.MiddleRight;

            MakeText(_forgeResultOverlay.transform, "Arrow", "→", 22,
                new Vector2(0f, -20f), new Vector2(40f, 28f),
                new Color(1f, 0.82f, 0.4f), FontStyle.Bold).alignment = TextAnchor.MiddleCenter;

            _forgeResultAfter = MakeText(_forgeResultOverlay.transform, "After", "+1", 16,
                new Vector2(110f, -20f), new Vector2(180f, 28f),
                new Color(1f, 0.9f, 0.55f), FontStyle.Bold);
            _forgeResultAfter.alignment = TextAnchor.MiddleLeft;

            _forgeResultStats = MakeText(_forgeResultOverlay.transform, "Stats", "", 13,
                new Vector2(0f, -70f), new Vector2(480f, 56f),
                new Color(0.88f, 0.78f, 0.6f));
            _forgeResultStats.alignment = TextAnchor.MiddleCenter;
            _forgeResultStats.horizontalOverflow = HorizontalWrapMode.Wrap;
            _forgeResultStats.verticalOverflow = VerticalWrapMode.Truncate;
            _forgeResultStats.lineSpacing = 1.15f;

            CreateActionButton(_forgeResultOverlay.transform, "Ok", "확인",
                new Vector2(0f, -140f), new Vector2(200f, 40f), BtnForge, HideForgeResultOverlay);

            _forgeResultOverlay.SetActive(false);
        }

        private void BuildSynthOverlay()
        {
            // 왼쪽 가방 슬롯을 가리지 않도록 우측 컬럼에만 붙임
            _synthOverlay = CreateOverlayPanel(_panel.transform, "SynthOverlay",
                new Vector2(300f, 420f), new Vector2(280f, 10f), blockBackground: false);

            MakeText(_synthOverlay.transform, "Title", "무기 합성", 18,
                new Vector2(0f, 185f), new Vector2(280f, 26f),
                new Color(0.98f, 0.85f, 0.5f), FontStyle.Bold).alignment = TextAnchor.MiddleCenter;

            MakeText(_synthOverlay.transform, "Sub", "같은 등급 · 강화 합산", 12,
                new Vector2(0f, 162f), new Vector2(280f, 18f),
                new Color(0.75f, 0.65f, 0.5f)).alignment = TextAnchor.MiddleCenter;

            CreateSynthMaterialCard(_synthOverlay.transform, "CardA",
                new Vector2(0f, 95f), "1번 재료", out _synthCardALabel, out _synthIconA);

            MakeText(_synthOverlay.transform, "Plus", "+", 22,
                new Vector2(0f, 30f), new Vector2(40f, 28f),
                new Color(0.9f, 0.75f, 0.45f), FontStyle.Bold).alignment = TextAnchor.MiddleCenter;

            CreateSynthMaterialCard(_synthOverlay.transform, "CardB",
                new Vector2(0f, -35f), "2번 재료", out _synthCardBLabel, out _synthIconB);

            MakeText(_synthOverlay.transform, "Arrow", "→ 더 높은 등급", 12,
                new Vector2(0f, -100f), new Vector2(260f, 18f),
                new Color(0.7f, 0.6f, 0.45f)).alignment = TextAnchor.MiddleCenter;

            _synthGuideText = MakeText(_synthOverlay.transform, "Guide",
                "① 왼쪽 가방에서\n첫 무기를 누르세요", 12,
                new Vector2(0f, -130f), new Vector2(270f, 36f),
                new Color(0.95f, 0.8f, 0.45f), FontStyle.Bold);
            _synthGuideText.alignment = TextAnchor.MiddleCenter;
            _synthGuideText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _synthGuideText.verticalOverflow = VerticalWrapMode.Overflow;

            _synthErrorText = MakeText(_synthOverlay.transform, "Error", "", 11,
                new Vector2(0f, -160f), new Vector2(270f, 18f),
                new Color(0.95f, 0.4f, 0.35f));
            _synthErrorText.alignment = TextAnchor.MiddleCenter;

            var confirmGo = CreateActionButton(_synthOverlay.transform, "Confirm", "합성하기",
                new Vector2(-60f, -185f), new Vector2(130f, 34f), BtnSynth, OnSynthConfirmClicked);
            _synthConfirmBtn = confirmGo.GetComponent<Button>();
            _synthConfirmBg = confirmGo.GetComponent<Image>();

            CreateActionButton(_synthOverlay.transform, "Cancel", "취소",
                new Vector2(80f, -185f), new Vector2(90f, 34f), BtnClose, () => HideSynthOverlay());

            _synthOverlay.SetActive(false);
        }

        private static GameObject CreateSynthMaterialCard(Transform parent, string name, Vector2 pos,
            string header, out Text label, out Image iconImg)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(250f, 88f);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.12f, 0.1f, 0.08f, 0.98f);
            var ol = go.AddComponent<Outline>();
            ol.effectColor = new Color(0.55f, 0.4f, 0.2f, 0.55f);
            ol.effectDistance = new Vector2(1.5f, -1.5f);

            MakeText(go.transform, "Head", header, 11,
                new Vector2(0f, 30f), new Vector2(230f, 16f),
                new Color(0.9f, 0.72f, 0.45f), FontStyle.Bold).alignment = TextAnchor.MiddleCenter;

            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(go.transform, false);
            var iconRt = iconGo.AddComponent<RectTransform>();
            iconRt.anchoredPosition = new Vector2(-80f, -4f);
            iconRt.sizeDelta = new Vector2(40f, 40f);
            iconImg = iconGo.AddComponent<Image>();
            iconImg.preserveAspect = true;
            iconImg.color = new Color(1f, 1f, 1f, 0f);
            iconImg.raycastTarget = false;

            label = MakeText(go.transform, "Name", "(비어있음)", 12,
                new Vector2(30f, -4f), new Vector2(160f, 40f),
                new Color(0.55f, 0.48f, 0.4f));
            label.alignment = TextAnchor.MiddleLeft;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;

            return go;
        }

        private static GameObject CreateOverlayPanel(Transform parent, string name, Vector2 size,
            Vector2 anchoredPos, bool blockBackground)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
            var img = go.AddComponent<Image>();
            img.color = OverlayBg;
            var outline = go.AddComponent<Outline>();
            outline.effectColor = EmberGold;
            outline.effectDistance = new Vector2(2.5f, -2.5f);

            if (blockBackground)
            {
                var veil = new GameObject("Veil");
                veil.transform.SetParent(go.transform, false);
                veil.transform.SetAsFirstSibling();
                var vRt = veil.AddComponent<RectTransform>();
                vRt.anchorMin = new Vector2(0.5f, 0.5f);
                vRt.anchorMax = new Vector2(0.5f, 0.5f);
                vRt.pivot = new Vector2(0.5f, 0.5f);
                vRt.sizeDelta = new Vector2(900f, 640f);
                vRt.anchoredPosition = -anchoredPos;
                var vImg = veil.AddComponent<Image>();
                vImg.color = new Color(0.02f, 0.015f, 0.01f, 0.55f);
                vImg.raycastTarget = true;
            }

            return go;
        }

        public void Bind(PlayerStats stats)
        {
            _stats = stats;
            if (_open)
            {
                Refresh();
            }
        }

        private void Update()
        {
            if (GameInput.BagDown)
            {
                if (_open)
                {
                    Close();
                }
                else if (!GameUi.IsPaused && !GameUi.IsBlocking)
                {
                    Open();
                }
            }
        }

        public void Open()
        {
            if (_builtLayoutVersion != LayoutVersion)
            {
                var canvasRoot = _panel != null
                    ? _panel.transform.parent
                    : FindFirstObjectByType<Canvas>()?.transform;
                if (canvasRoot != null)
                {
                    Construct(canvasRoot);
                }
            }

            if (_panel == null || _stats == null)
            {
                _stats ??= FindFirstObjectByType<PlayerStats>();
            }

            if (_panel == null || _stats == null)
            {
                return;
            }

            _open = true;
            _skillPickWeapon = null;
            HideForgeOverlay();
            HideForgeResultOverlay();
            HideSynthOverlay(refreshUi: false);
            GameUi.IsBlocking = true;
            Time.timeScale = 0f;
            _dim.SetActive(true);
            _dim.transform.SetAsLastSibling();
            _panel.transform.SetAsLastSibling();
            Refresh();
            ResetScrolls();
            StartCoroutine(UiPanelMotion.FadeIn(_panel, 0.16f, 14f));
            CombatAudio.UiClick();
            FindFirstObjectByType<HudUI>()?.RefreshHintVisibility();
        }

        public void Close()
        {
            if (!_open)
            {
                return;
            }

            HideForgeOverlay();
            HideForgeResultOverlay();
            HideSynthOverlay(refreshUi: false);
            _open = false;
            if (_dim != null)
            {
                _dim.SetActive(false);
            }

            if (_panel != null)
            {
                StartCoroutine(UiPanelMotion.FadeOut(_panel, 0.1f));
            }

            GameUi.IsBlocking = false;
            if (!GameUi.IsPaused)
            {
                Time.timeScale = 1f;
            }

            CombatAudio.UiClick();
            FindFirstObjectByType<HudUI>()?.RefreshHintVisibility();
        }

        private void Refresh()
        {
            if (_stats == null)
            {
                return;
            }

            // 세이브/합성으로 생긴 중간 빈칸을 열 때마다 앞으로 모은 뒤 저장
            _stats.CompactInventory();
            _stats.SyncToSave();

            // 정렬 후 선택 슬롯이 빈칸이면 장착 무기로 되돌림
            if (!_selectedSlot.HasValue ||
                !_stats.IsValidSlot(_selectedSlot.Value) ||
                _stats.GetInvSlot(_selectedSlot.Value).IsEmpty)
            {
                _selectedSlot = _stats.EquippedInvSlot;
            }

            _headerMeta.text =
                $"모험가 Lv {_stats.Level}  ·  가방 {_stats.UsedBagSlots}/{_stats.BagCapacity}  ·  " +
                $"무기 {_stats.CountWeaponSlots()}  ·  포션 ×{_stats.Potions}  ·  {_stats.Gold}G  ·  축복 +{_stats.MetaBlessing}";

            RebuildWeapons();
            RebuildItems();
            RebuildSkills();
            ResetScrolls();

            if (_selectedSlot.HasValue && _stats.IsValidSlot(_selectedSlot.Value) &&
                !_stats.GetInvSlot(_selectedSlot.Value).IsEmpty)
            {
                SelectSlot(_selectedSlot.Value, refreshCards: false);
            }
            else
            {
                _selectedSlot = _stats.EquippedInvSlot;
                SelectSlot(_stats.EquippedInvSlot, refreshCards: false);
            }

            if (IsSynthOverlayOpen)
            {
                RefreshSynthWizardUI();
            }
        }

        private void ResetScrolls()
        {
            if (_slotScroll != null)
            {
                _slotScroll.verticalNormalizedPosition = 1f;
            }

            if (_skillScroll != null)
            {
                _skillScroll.verticalNormalizedPosition = 1f;
            }
        }

        private void RebuildWeapons()
        {
            ClearChildren(_weaponCards, _weaponGrid);
            _slotCardBgs.Clear();
            if (_stats == null)
            {
                return;
            }

            var capacity = _stats.BagCapacity;
            var synthOpen = IsSynthOverlayOpen;

            for (var i = 0; i < capacity; i++)
            {
                var index = i;
                var slot = _stats.GetInvSlot(index);
                var selected = !synthOpen && _selectedSlot.HasValue && _selectedSlot.Value == index;
                var isMatA = _synthSlotA.HasValue && _synthSlotA.Value == index;
                var isMatB = _synthSlotB.HasValue && _synthSlotB.Value == index;
                var isMat = isMatA || isMatB;
                var equipped = index == _stats.EquippedInvSlot && _stats.IsWeaponSlot(index);
                var eligible = IsSynthEligible(index);

                var bgColor = SlotBgColor(slot, selected, isMat, equipped, eligible);
                var card = CreateGridSlot(_weaponGrid, $"Slot_{i}", bgColor, () => OnSlotClick(index));
                _slotCardBgs[index] = card.GetComponent<Image>();

                if (slot.Kind == InvItemKind.Weapon)
                {
                    var rarity = WeaponCatalog.GetRarity(slot.Weapon);
                    var outline = card.AddComponent<Outline>();
                    outline.effectColor = eligible
                        ? SynthEligible
                        : new Color(
                            WeaponCatalog.RarityAccent(rarity).r,
                            WeaponCatalog.RarityAccent(rarity).g,
                            WeaponCatalog.RarityAccent(rarity).b, 0.9f);
                    outline.effectDistance = new Vector2(eligible || selected || isMat ? 2f : 1.5f,
                        eligible || selected || isMat ? -2f : -1.5f);

                    var dim = synthOpen && !isMat && !eligible && _synthSlotA.HasValue;
                    AttachCenterIcon(card.transform, UiItemIcon.ForWeapon(slot.Weapon), 58f,
                        dim ? 0.35f : 1f);

                    var tag = isMatA
                        ? "1번"
                        : isMatB
                            ? "2번"
                            : eligible
                                ? "가능"
                                : selected
                                    ? "선택"
                                    : equipped
                                        ? "장착"
                                        : $"+{slot.upgrade}";
                    AttachBadge(card.transform, tag, isMat
                        ? new Color(0.5f, 0.28f, 0.55f, 0.95f)
                        : eligible
                            ? new Color(0.35f, 0.45f, 0.15f, 0.95f)
                            : equipped
                                ? new Color(0.35f, 0.55f, 0.4f, 0.95f)
                                : new Color(0.12f, 0.09f, 0.06f, 0.92f));
                }
                else if (slot.Kind == InvItemKind.Potion)
                {
                    AttachCenterIcon(card.transform, UiItemIcon.ForShopKind("potion"), 58f,
                        synthOpen ? 0.35f : 1f);
                    AttachBadge(card.transform, $"×{slot.count}",
                        new Color(0.12f, 0.22f, 0.14f, 0.95f));
                }
                else if (slot.Kind == InvItemKind.Luck)
                {
                    AttachCenterIcon(card.transform, UiItemIcon.ForShopKind("luck"), 58f,
                        synthOpen ? 0.35f : 1f);
                    AttachBadge(card.transform, $"×{slot.count}",
                        new Color(0.18f, 0.14f, 0.24f, 0.95f));
                }
                else
                {
                    // 빈 칸: 디버그성 "#N 빈칸" 라벨 없이 얕은 홈만
                    var dash = card.AddComponent<Outline>();
                    dash.effectColor = selected
                        ? new Color(0.85f, 0.6f, 0.28f, 0.85f)
                        : WellDash;
                    dash.effectDistance = selected ? new Vector2(2f, -2f) : new Vector2(1f, -1f);
                }

                _weaponCards.Add(card);
            }

            // ContentSizeFitter가 preferred를 잡도록 레이아웃 강제 갱신
            if (_weaponGrid is RectTransform content)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                var rows = (capacity + SlotCols - 1) / SlotCols;
                var pad = 16f;
                var h = rows * SlotCellH + Mathf.Max(0, rows - 1) * SlotGap + pad;
                if (content.GetComponent<ContentSizeFitter>() == null)
                {
                    content.sizeDelta = new Vector2(0f, Mathf.Max(340f, h));
                }
            }
        }

        private static Color SlotBgColor(InvSlot slot, bool selected, bool isMat, bool equipped,
            bool synthEligible = false)
        {
            Color bg;
            if (slot == null || slot.IsEmpty)
            {
                bg = WellBg;
            }
            else if (slot.Kind == InvItemKind.Weapon)
            {
                bg = WeaponCatalog.RarityBg(WeaponCatalog.GetRarity(slot.Weapon));
            }
            else if (slot.Kind == InvItemKind.Potion)
            {
                bg = new Color(0.12f, 0.2f, 0.14f, 0.95f);
            }
            else if (slot.Kind == InvItemKind.Luck)
            {
                bg = new Color(0.16f, 0.12f, 0.2f, 0.95f);
            }
            else
            {
                bg = WellBg;
            }

            if (isMat)
            {
                bg = Color.Lerp(bg, new Color(0.55f, 0.28f, 0.6f), 0.5f);
            }
            else if (synthEligible)
            {
                bg = Color.Lerp(bg, SynthEligible, 0.45f);
            }
            else if (selected)
            {
                // 빈 칸은 배경을 어둡게 만들지 않고 테두리로만 선택 표시
                if (slot != null && !slot.IsEmpty)
                {
                    bg = Color.Lerp(bg, new Color(0.75f, 0.5f, 0.22f), 0.4f);
                }
                else
                {
                    bg = new Color(0.12f, 0.09f, 0.07f, 0.95f);
                }
            }
            else if (equipped)
            {
                bg = Color.Lerp(bg, new Color(0.3f, 0.45f, 0.28f), 0.28f);
            }

            return bg;
        }

        private bool IsSynthEligible(int index)
        {
            if (!IsSynthOverlayOpen || !_synthSlotA.HasValue || _stats == null)
            {
                return false;
            }

            var mat = _synthSlotA.Value;
            if (index == mat || (_synthSlotB.HasValue && index == _synthSlotB.Value))
            {
                return false;
            }

            if (!_stats.IsWeaponSlot(index) || !_stats.IsWeaponSlot(mat))
            {
                return false;
            }

            var a = _stats.GetInvSlot(mat);
            var b = _stats.GetInvSlot(index);
            var ra = WeaponCatalog.GetRarity(a.Weapon);
            var rb = WeaponCatalog.GetRarity(b.Weapon);
            return ra == rb && ra < WeaponRarity.Legendary;
        }

        private static void AttachCenterIcon(Transform parent, Sprite sprite, float size, float alpha = 1f)
        {
            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(parent, false);
            var iconRt = iconGo.AddComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.5f, 0.58f);
            iconRt.anchorMax = new Vector2(0.5f, 0.58f);
            iconRt.pivot = new Vector2(0.5f, 0.5f);
            iconRt.anchoredPosition = Vector2.zero;
            iconRt.sizeDelta = new Vector2(size, size);
            var iconBg = iconGo.AddComponent<Image>();
            iconBg.color = new Color(0.05f, 0.04f, 0.03f, 0.88f * alpha);
            iconBg.raycastTarget = false;

            var iconSprite = new GameObject("S");
            iconSprite.transform.SetParent(iconGo.transform, false);
            var isRt = iconSprite.AddComponent<RectTransform>();
            isRt.anchorMin = Vector2.zero;
            isRt.anchorMax = Vector2.one;
            isRt.offsetMin = new Vector2(3f, 3f);
            isRt.offsetMax = new Vector2(-3f, -3f);
            var isImg = iconSprite.AddComponent<Image>();
            isImg.sprite = sprite;
            isImg.preserveAspect = true;
            isImg.color = new Color(1f, 1f, 1f, alpha);
            isImg.raycastTarget = false;
        }

        private static void AttachBadge(Transform parent, string label, Color bg,
            Color? textColor = null)
        {
            var badge = new GameObject("Badge");
            badge.transform.SetParent(parent, false);
            var bRt = badge.AddComponent<RectTransform>();
            bRt.anchorMin = new Vector2(0.5f, 0f);
            bRt.anchorMax = new Vector2(0.5f, 0f);
            bRt.pivot = new Vector2(0.5f, 0f);
            bRt.anchoredPosition = new Vector2(0f, 5f);
            bRt.sizeDelta = new Vector2(76f, 18f);
            var bImg = badge.AddComponent<Image>();
            bImg.color = bg;
            bImg.raycastTarget = false;

            var t = MakeText(badge.transform, "T", label, 10, Vector2.zero,
                new Vector2(72f, 16f), textColor ?? new Color(0.95f, 0.88f, 0.7f), FontStyle.Bold);
            t.alignment = TextAnchor.MiddleCenter;
        }

        private void RebuildItems()
        {
            ClearChildren(_itemCards, _itemGrid);
            if (_stats == null)
            {
                return;
            }

            // 장착 중 카드
            var eqSlot = _stats.GetInvSlot(_stats.EquippedInvSlot);
            var eqLabel = _stats.IsWeaponSlot(_stats.EquippedInvSlot)
                ? WeaponCatalog.Label(eqSlot.Weapon, eqSlot.upgrade, eqSlot.temper, eqSlot.HasSigil)
                : "무기 없음";
            var eqAtk = $"ATK {_stats.AttackPower}";

            var eqCard = CreateCard(_itemGrid, "Equipped", new Vector2(0f, 20f), new Vector2(270f, 48f),
                new Color(0.14f, 0.12f, 0.09f, 0.96f), () =>
                {
                    if (_stats.IsWeaponSlot(_stats.EquippedInvSlot))
                    {
                        SelectSlot(_stats.EquippedInvSlot, refreshCards: true);
                    }
                    else
                    {
                        ShowDetail("장착 중", "장착된 무기가 없습니다.");
                        SetEquipButtonState(false, "장착");
                        SetForgeButtonState(true, "가방 강화");
                    }
                });
            var eqOutline = eqCard.AddComponent<Outline>();
            eqOutline.effectColor = new Color(0.7f, 0.45f, 0.2f, 0.5f);
            eqOutline.effectDistance = new Vector2(1f, -1f);

            if (_stats.IsWeaponSlot(_stats.EquippedInvSlot))
            {
                AttachItemIcon(eqCard.transform, UiItemIcon.ForWeapon(eqSlot.Weapon), -105f, 34f);
            }

            MakeText(eqCard.transform, "T", eqLabel, 12, new Vector2(18f, 7f),
                new Vector2(200f, 20f), new Color(0.95f, 0.85f, 0.6f), FontStyle.Bold).alignment =
                TextAnchor.MiddleLeft;
            MakeText(eqCard.transform, "H", eqAtk, 11, new Vector2(18f, -11f),
                new Vector2(200f, 16f), new Color(0.65f, 0.55f, 0.4f)).alignment =
                TextAnchor.MiddleLeft;
            _itemCards.Add(eqCard);

            var goldCard = CreateCard(_itemGrid, "Gold", new Vector2(0f, -28f), new Vector2(270f, 34f),
                new Color(0.16f, 0.13f, 0.08f, 0.95f), () =>
                {
                    ShowDetail("골드", $"{_stats.Gold} G\n대장간·상점에서 무기 구매·강화에 사용합니다.");
                    SetEquipButtonState(false, "장착");
                    SetForgeButtonState(true, "가방 강화");
                });
            AttachItemIcon(goldCard.transform, UiItemIcon.ForShopKind("gold"), -108f, 26f);
            MakeText(goldCard.transform, "T", $"{_stats.Gold} G", 13,
                new Vector2(16f, 0f), new Vector2(200f, 24f), new Color(0.95f, 0.78f, 0.4f), FontStyle.Bold)
                .alignment = TextAnchor.MiddleLeft;
            _itemCards.Add(goldCard);
        }

        private static void AttachItemIcon(Transform parent, Sprite sprite, float x, float size = 32f)
        {
            var iconGo = new GameObject("Icon");
            iconGo.transform.SetParent(parent, false);
            var iconRt = iconGo.AddComponent<RectTransform>();
            iconRt.anchoredPosition = new Vector2(x, 0f);
            iconRt.sizeDelta = new Vector2(size, size);
            var bg = iconGo.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.04f, 0.03f, 0.85f);
            bg.raycastTarget = false;
            var sGo = new GameObject("S");
            sGo.transform.SetParent(iconGo.transform, false);
            var sRt = sGo.AddComponent<RectTransform>();
            sRt.anchorMin = Vector2.zero;
            sRt.anchorMax = Vector2.one;
            sRt.offsetMin = new Vector2(2f, 2f);
            sRt.offsetMax = new Vector2(-2f, -2f);
            var img = sGo.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
        }

        private void RebuildSkills()
        {
            ClearChildren(_skillCards, _skillGrid);
            if (_stats == null)
            {
                return;
            }

            // 보유 무기별 고유 스킬 목록 — 클릭 후 [스킬 장착]으로 R 교체
            var seen = new HashSet<int>();
            var active = _stats.SkillSourceWeapon;

            for (var i = 0; i < _stats.BagCapacity; i++)
            {
                if (!_stats.IsWeaponSlot(i))
                {
                    continue;
                }

                var w = _stats.GetInvSlot(i).Weapon;
                var wid = (int)w;
                if (!seen.Add(wid))
                {
                    continue;
                }

                var def = SkillCatalog.ForWeaponDef(w);
                var title = SkillCatalog.WeaponSkillName(w);
                var wName = WeaponCatalog.Get(w).Name;
                var isActive = active == w;
                var picked = _skillPickWeapon.HasValue && _skillPickWeapon.Value == w;
                var row = CreateSkillPickRow(
                    _skillGrid, title, wName, def, isActive || picked,
                    isActive ? "R로 사용 중" : "클릭 후 스킬 장착",
                    () => OnSkillWeaponClick(w));
                _skillCards.Add(row);
            }

            if (_skillGrid is RectTransform content)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            }

            if (_skillScroll != null)
            {
                _skillScroll.verticalNormalizedPosition = 1f;
            }
        }

        private static GameObject CreateSkillPickRow(Transform parent, string title, string weaponName,
            SkillDef def, bool highlighted, string statusLine, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("SkillPick");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(260f, 72f);

            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 72f;
            le.preferredHeight = 72f;
            le.flexibleWidth = 1f;

            var img = go.AddComponent<Image>();
            img.color = highlighted
                ? new Color(0.2f, 0.15f, 0.1f, 0.98f)
                : new Color(0.13f, 0.1f, 0.08f, 0.96f);
            var ol = go.AddComponent<Outline>();
            ol.effectColor = highlighted
                ? new Color(0.9f, 0.6f, 0.28f, 0.75f)
                : new Color(0.4f, 0.3f, 0.18f, 0.4f);
            ol.effectDistance = new Vector2(1f, -1f);

            MakeText(go.transform, "T", title, 13,
                new Vector2(0f, 20f), new Vector2(244f, 20f),
                highlighted
                    ? new Color(0.98f, 0.88f, 0.6f)
                    : new Color(0.9f, 0.8f, 0.62f),
                FontStyle.Bold).alignment = TextAnchor.MiddleCenter;

            MakeText(go.transform, "W", $"{weaponName}  ·  {def.Role}  ·  쿨 {def.Cooldown:0.#}초", 10,
                new Vector2(0f, 2f), new Vector2(244f, 16f),
                new Color(0.75f, 0.55f, 0.28f)).alignment = TextAnchor.MiddleCenter;

            MakeText(go.transform, "S", statusLine, 10,
                new Vector2(0f, -18f), new Vector2(244f, 16f),
                highlighted
                    ? new Color(0.7f, 0.85f, 0.55f)
                    : new Color(0.65f, 0.58f, 0.48f)).alignment = TextAnchor.MiddleCenter;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);
            return go;
        }

        private void OnSkillWeaponClick(WeaponId weapon)
        {
            if (_stats == null)
            {
                return;
            }

            _skillPickWeapon = weapon;
            var title = SkillCatalog.WeaponSkillName(weapon);
            var active = _stats.SkillSourceWeapon == weapon;
            ShowDetail(active ? $"사용 중  ·  {title}" : $"스킬  ·  {title}",
                SkillCatalog.DetailForWeapon(weapon) +
                "\n\n무기와 별개로 R 스킬만 이것으로 바꿀 수 있습니다.");
            SetEquipButtonState(!active, active ? "사용 중" : "스킬 장착");
            SetSellButtonState(false, "판매");
            SetForgeButtonState(true, "가방 강화");
            SetActionGuide(active
                ? $"{title} 사용 중  ·  R 키로 발동"
                : $"{title} 선택  ·  [스킬 장착]으로 R 스킬 변경");
            CombatAudio.UiClick();
            RebuildSkills();
        }

        private static GameObject CreateSkillRow(Transform parent, SkillDef def, bool equipped,
            UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject($"Sk_{(int)def.Id}");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(260f, 36f);

            var le = go.AddComponent<LayoutElement>();
            le.minHeight = 36f;
            le.preferredHeight = 36f;
            le.flexibleWidth = 1f;

            var bg = equipped
                ? new Color(0.22f, 0.16f, 0.1f, 0.98f)
                : new Color(0.11f, 0.09f, 0.07f, 0.96f);
            var img = go.AddComponent<Image>();
            img.color = bg;

            var ol = go.AddComponent<Outline>();
            ol.effectColor = equipped
                ? new Color(0.85f, 0.55f, 0.25f, 0.65f)
                : new Color(0.4f, 0.3f, 0.18f, 0.35f);
            ol.effectDistance = new Vector2(1f, -1f);

            // 좌측 액센트
            var rail = new GameObject("Rail");
            rail.transform.SetParent(go.transform, false);
            var railRt = rail.AddComponent<RectTransform>();
            railRt.anchorMin = new Vector2(0f, 0f);
            railRt.anchorMax = new Vector2(0f, 1f);
            railRt.pivot = new Vector2(0f, 0.5f);
            railRt.sizeDelta = new Vector2(3f, -6f);
            railRt.anchoredPosition = new Vector2(3f, 0f);
            rail.AddComponent<Image>().color = equipped
                ? new Color(1f, 0.72f, 0.32f)
                : new Color(0.55f, 0.4f, 0.22f);
            rail.GetComponent<Image>().raycastTarget = false;

            // 아이콘 우물
            var well = new GameObject("Well");
            well.transform.SetParent(go.transform, false);
            var wellRt = well.AddComponent<RectTransform>();
            wellRt.anchorMin = new Vector2(0f, 0.5f);
            wellRt.anchorMax = new Vector2(0f, 0.5f);
            wellRt.pivot = new Vector2(0f, 0.5f);
            wellRt.anchoredPosition = new Vector2(12f, 0f);
            wellRt.sizeDelta = new Vector2(28f, 28f);
            var wellImg = well.AddComponent<Image>();
            wellImg.color = new Color(0.05f, 0.04f, 0.03f, 0.9f);
            wellImg.raycastTarget = false;
            var iconGo = new GameObject("S");
            iconGo.transform.SetParent(well.transform, false);
            var iconRt = iconGo.AddComponent<RectTransform>();
            iconRt.anchorMin = Vector2.zero;
            iconRt.anchorMax = Vector2.one;
            iconRt.offsetMin = new Vector2(3f, 3f);
            iconRt.offsetMax = new Vector2(-3f, -3f);
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = UiItemIcon.ForShopKind("skill");
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            var name = MakeText(go.transform, "T", def.Name, 12,
                new Vector2(48f, 7f), new Vector2(150f, 18f),
                equipped ? new Color(0.98f, 0.88f, 0.6f) : new Color(0.82f, 0.72f, 0.58f),
                FontStyle.Bold);
            name.alignment = TextAnchor.MiddleLeft;
            var nameRt = name.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0f, 0.5f);
            nameRt.anchorMax = new Vector2(0f, 0.5f);
            nameRt.pivot = new Vector2(0f, 0.5f);
            nameRt.anchoredPosition = new Vector2(48f, 7f);

            var sub = MakeText(go.transform, "S",
                equipped ? $"장착 중  ·  {def.Role}" : def.Role,
                10, new Vector2(48f, -9f), new Vector2(160f, 16f),
                equipped ? new Color(0.75f, 0.55f, 0.28f) : new Color(0.5f, 0.42f, 0.34f));
            sub.alignment = TextAnchor.MiddleLeft;
            var subRt = sub.GetComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0f, 0.5f);
            subRt.anchorMax = new Vector2(0f, 0.5f);
            subRt.pivot = new Vector2(0f, 0.5f);
            subRt.anchoredPosition = new Vector2(48f, -9f);

            if (equipped)
            {
                var chip = new GameObject("Chip");
                chip.transform.SetParent(go.transform, false);
                var chipRt = chip.AddComponent<RectTransform>();
                chipRt.anchorMin = new Vector2(1f, 0.5f);
                chipRt.anchorMax = new Vector2(1f, 0.5f);
                chipRt.pivot = new Vector2(1f, 0.5f);
                chipRt.anchoredPosition = new Vector2(-8f, 0f);
                chipRt.sizeDelta = new Vector2(48f, 20f);
                var chipImg = chip.AddComponent<Image>();
                chipImg.color = new Color(0.55f, 0.35f, 0.12f, 0.95f);
                chipImg.raycastTarget = false;
                MakeText(chip.transform, "C", "장착", 10, Vector2.zero, new Vector2(44f, 18f),
                    new Color(1f, 0.9f, 0.7f), FontStyle.Bold).alignment = TextAnchor.MiddleCenter;
            }

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.highlightedColor = new Color(1.12f, 1.08f, 1f, 1f);
            colors.pressedColor = new Color(0.8f, 0.75f, 0.7f, 1f);
            btn.colors = colors;
            btn.onClick.AddListener(onClick);
            return go;
        }

        private void OnSlotClick(int index)
        {
            CombatAudio.UiClick();
            _lastForgeEffect = "";
            SetForgeEffect("");

            if (IsSynthOverlayOpen)
            {
                HandleSynthWizardPick(index);
                return;
            }

            SelectSlot(index, refreshCards: true);
        }

        private void HandleSynthWizardPick(int index)
        {
            if (_stats == null || !_stats.IsWeaponSlot(index))
            {
                SetSynthError("무기가 있는 칸을 누르세요");
                return;
            }

            var slot = _stats.GetInvSlot(index);
            var rarity = WeaponCatalog.GetRarity(slot.Weapon);
            if (rarity >= WeaponRarity.Legendary)
            {
                SetSynthError("전설 등급은 합성할 수 없습니다");
                FindFirstObjectByType<ClearBannerUI>()
                    ?.Show("전설 등급은 합성할 수 없다", ToastKind.Danger, 2.2f);
                return;
            }

            // 이미 채워진 슬롯을 다시 누르면 무시
            if (_synthSlotA.HasValue && _synthSlotA.Value == index)
            {
                return;
            }

            if (_synthSlotB.HasValue && _synthSlotB.Value == index)
            {
                return;
            }

            if (!_synthSlotA.HasValue)
            {
                _synthSlotA = index;
                SetSynthError("");
                RefreshSynthWizardUI();
                RebuildWeapons();
                return;
            }

            // 2번 재료: 등급 검사
            var mat = _stats.GetInvSlot(_synthSlotA.Value);
            var matR = WeaponCatalog.GetRarity(mat.Weapon);
            if (rarity != matR)
            {
                var gradeMsg = $"등급이 다릅니다 · [{WeaponCatalog.RarityName(matR)}] 무기를 고르세요";
                SetSynthError(gradeMsg);
                FindFirstObjectByType<ClearBannerUI>()
                    ?.Show(gradeMsg, ToastKind.Danger, 2.2f);
                return;
            }

            if (_synthSlotB.HasValue)
            {
                // 이미 둘 다 채워져 있으면 2번만 교체
                _synthSlotB = index;
            }
            else
            {
                _synthSlotB = index;
            }

            SetSynthError("");
            RefreshSynthWizardUI();
            RebuildWeapons();
        }

        private void SetSynthError(string msg)
        {
            if (_synthErrorText != null)
            {
                _synthErrorText.text = msg ?? "";
            }
        }

        private void RefreshSynthWizardUI()
        {
            FillSynthCard(_synthSlotA, _synthCardALabel, _synthIconA);
            FillSynthCard(_synthSlotB, _synthCardBLabel, _synthIconB);

            var bothReady = _synthSlotA.HasValue && _synthSlotB.HasValue
                            && IsSynthPairCompatible(_synthSlotA.Value, _synthSlotB.Value);

            if (_synthConfirmBtn != null)
            {
                _synthConfirmBtn.interactable = bothReady;
            }

            if (_synthConfirmBg != null)
            {
                _synthConfirmBg.color = bothReady ? BtnSynth : BtnDisabled;
            }

            if (_synthGuideText != null)
            {
                if (!_synthSlotA.HasValue)
                {
                    _synthGuideText.text = "① 왼쪽에서\n첫 무기를 누르세요";
                    SetActionGuide("① 왼쪽 가방에서 첫 무기를 누르세요");
                }
                else if (!_synthSlotB.HasValue)
                {
                    _synthGuideText.text = "② 같은 등급\n무기를 누르세요";
                    SetActionGuide("② 같은 등급 무기를 누르세요");
                }
                else if (bothReady)
                {
                    var a = _stats.GetInvSlot(_synthSlotA.Value);
                    var b = _stats.GetInvSlot(_synthSlotB.Value);
                    var sum = Mathf.Max(0, a.upgrade) + Mathf.Max(0, b.upgrade);
                    _synthGuideText.text = sum > 0
                        ? $"③ 강화 +{a.upgrade}+{b.upgrade}\n→ 결과 +{sum}"
                        : "③ [합성하기]\n를 누르세요";
                    SetActionGuide(sum > 0
                        ? $"합성 시 강화 +{a.upgrade}+{b.upgrade} → +{sum} 합산"
                        : "③ [합성하기]를 누르세요");
                }
                else
                {
                    _synthGuideText.text = "같은 등급 2개가\n필요합니다";
                    SetActionGuide("등급이 같은 무기 2개가 필요합니다");
                }
            }
        }

        private bool IsSynthPairCompatible(int slotA, int slotB)
        {
            if (_stats == null || !_stats.IsWeaponSlot(slotA) || !_stats.IsWeaponSlot(slotB))
            {
                return false;
            }

            var a = _stats.GetInvSlot(slotA);
            var b = _stats.GetInvSlot(slotB);
            var ra = WeaponCatalog.GetRarity(a.Weapon);
            var rb = WeaponCatalog.GetRarity(b.Weapon);
            return ra == rb && ra < WeaponRarity.Legendary;
        }

        private void FillSynthCard(int? slotIndex, Text label, Image icon)
        {
            if (label == null)
            {
                return;
            }

            if (!slotIndex.HasValue || _stats == null || !_stats.IsWeaponSlot(slotIndex.Value))
            {
                label.text = "(비어있음)";
                label.color = new Color(0.55f, 0.48f, 0.4f);
                if (icon != null)
                {
                    icon.sprite = null;
                    icon.color = new Color(1f, 1f, 1f, 0f);
                }

                return;
            }

            var slot = _stats.GetInvSlot(slotIndex.Value);
            var name = WeaponCatalog.Get(slot.Weapon).Name;
            var rarity = WeaponCatalog.RarityName(WeaponCatalog.GetRarity(slot.Weapon));
            label.text = $"[{rarity}] {name}\n+{slot.upgrade}";
            label.color = new Color(0.95f, 0.88f, 0.7f);
            if (icon != null)
            {
                icon.sprite = UiItemIcon.ForWeapon(slot.Weapon);
                icon.color = Color.white;
            }
        }

        private void SelectSlot(int index, bool refreshCards)
        {
            if (_stats == null || !_stats.IsValidSlot(index))
            {
                return;
            }

            _selectedSlot = index;
            _skillPickWeapon = null;
            var slot = _stats.GetInvSlot(index);

            if (slot.IsEmpty)
            {
                ShowDetail($"빈 칸  ·  슬롯 #{index + 1}",
                    "이 칸은 비어 있습니다.\n상점에서 무기·포션을 구매하면 여기에 들어갑니다.");
                SetEquipButtonState(false, "장착");
                SetForgeButtonState(true, "가방 강화");
                SetSynthButtonState(true, "무기 합성");
                SetSellButtonState(false, "판매");
                if (!IsSynthOverlayOpen)
                {
                    SetActionGuide("가방 · 인벤 강화/합성  |  대장간 · 장착 연마·각인");
                }

                SetForgeEffect("");
            }
            else if (slot.Kind == InvItemKind.Potion)
            {
                var sell = _stats.PreviewSellPrice(index);
                ShowDetail("회복 포션",
                    $"슬롯 #{index + 1}  ·  보유 ×{slot.count}\n" +
                    "휴대용 녹빛 포션. 최대 체력의 약 40%를 즉시 회복한다.\n" +
                    $"[사용] 또는 Q  ·  판매 시 {sell}G");
                SetEquipButtonState(true, "사용");
                SetForgeButtonState(true, "가방 강화");
                SetSynthButtonState(true, "무기 합성");
                SetSellButtonState(true, $"판매 {sell}G");
                if (!IsSynthOverlayOpen)
                {
                    SetActionGuide($"포션 선택됨 · [사용] 또는 [판매 {sell}G]");
                }

                SetForgeEffect("");
            }
            else if (slot.Kind == InvItemKind.Luck)
            {
                var sell = _stats.PreviewSellPrice(index);
                ShowDetail("행운 부적",
                    $"슬롯 #{index + 1}  ·  충전 ×{slot.count}\n" +
                    "희미한 보라빛 부적. 탐험 중 자동으로 발동해 골드 획득 등 작은 행운을 더한다.\n" +
                    $"판매 시 {sell}G");
                SetEquipButtonState(false, "장착");
                SetForgeButtonState(true, "가방 강화");
                SetSynthButtonState(true, "무기 합성");
                SetSellButtonState(true, $"판매 {sell}G");
                if (!IsSynthOverlayOpen)
                {
                    SetActionGuide($"행운 부적 · [판매 {sell}G] 가능");
                }

                SetForgeEffect("");
            }
            else if (slot.Kind == InvItemKind.Weapon)
            {
                SelectWeaponSlot(index, slot);
            }

            if (!string.IsNullOrEmpty(_lastForgeEffect) && slot.Kind == InvItemKind.Weapon)
            {
                SetForgeEffect(_lastForgeEffect);
            }

            if (refreshCards)
            {
                RebuildWeapons();
            }
            else
            {
                RefreshCardHighlights();
            }
        }

        private void SelectWeaponSlot(int index, InvSlot slot)
        {
            var id = slot.Weapon;
            var up = slot.upgrade;
            var temper = slot.temper;
            var sigil = slot.HasSigil;
            var equipped = index == _stats.EquippedInvSlot;
            var rarity = WeaponCatalog.GetRarity(id);
            var bonus = WeaponCatalog.GetWeaponBonus(id, up, temper)
                        + (sigil && id == WeaponId.IronSword ? 4 : 0);
            var critPct = Mathf.RoundToInt(WeaponCatalog.GetCritBonus(id, sigil) * 100f);
            var forgeCap = WeaponCatalog.MaxUpgradeAllowed(id, _stats.Level);
            var label = WeaponCatalog.Label(id, up, temper, sigil);
            var grade = WeaponCatalog.GradeLine(id, up, temper, sigil, forgeCap);
            var def = WeaponCatalog.Get(id);

            var projectedAtk = Mathf.RoundToInt(
                (_stats.BaseAttackPower + bonus + _stats.MetaBlessing) * _stats.AttackMultiplier);

            var nextUp = Mathf.Min(up + 1, forgeCap);
            var nextBonus = WeaponCatalog.GetWeaponBonus(id, nextUp, temper)
                            + (sigil && id == WeaponId.IronSword ? 4 : 0);
            var forgeCost = WeaponCatalog.UpgradeCost(up, _stats.Level);
            var canForge = up < forgeCap;
            var forgePreview = canForge
                ? $"다음 +{up}→+{nextUp}  ·  보너스 +{bonus}→+{nextBonus}  ·  {forgeCost}G"
                : $"강화 상한 (+{forgeCap} / Lv {_stats.Level})";

            // 상세는 3줄로 고정 — 박스 밖으로 넘치지 않게
            var passive = def.Passive ?? "";
            if (passive.Length > 48)
            {
                passive = passive.Substring(0, 46) + "…";
            }

            var skill = SkillCatalog.Spec(id);
            ShowDetail(
                equipped ? $"장착 중  ·  {label}" : $"선택  ·  {label}",
                $"[{WeaponCatalog.RarityName(rarity)}]  ·  {grade}  ·  슬롯 #{index + 1}\n" +
                $"{passive}\n" +
                $"무기 스킬  ·  {skill.Name} (R)  ·  {skill.Role}  ·  쿨 {skill.Cooldown:0.#}초\n" +
                $"{skill.Description}\n" +
                $"보너스 +{bonus}  ·  ATK {projectedAtk}" +
                (critPct > 0 ? $"  ·  치명 +{critPct}%" : "") +
                $"  ·  {forgePreview}");

            if (equipped)
            {
                SetEquipButtonState(false, "장착 중");
            }
            else
            {
                SetEquipButtonState(!IsSynthOverlayOpen, "장착");
            }

            SetForgeButtonState(true, "가방 강화");
            SetSynthButtonState(true, "무기 합성");
            var sellPrice = _stats.PreviewSellPrice(index);
            var canSell = !equipped && _stats.CountWeaponSlots() > 1;
            SetSellButtonState(canSell, canSell ? $"판매 {sellPrice}G" : "판매");

            // 선택한 무기 스킬 카드로 즉시 갱신 (장착 무기와 다를 수 있음)
            RebuildSkills();

            if (!IsSynthOverlayOpen)
            {
                if (canForge)
                {
                    SetActionGuide(
                        $"선택됨 · 스킬 {skill.Name}  ·  강화 {forgeCost}G" +
                        (canSell ? $"  ·  판매 {sellPrice}G" : ""));
                }
                else
                {
                    SetActionGuide($"스킬 {skill.Name}  ·  강화 상한 · [무기 합성] 가능");
                }
            }
        }

        private void RefreshCardHighlights()
        {
            foreach (var kv in _slotCardBgs)
            {
                if (kv.Value == null || _stats == null)
                {
                    continue;
                }

                var slot = _stats.GetInvSlot(kv.Key);
                var selected = !IsSynthOverlayOpen && _selectedSlot.HasValue &&
                               _selectedSlot.Value == kv.Key;
                var isMat = (_synthSlotA.HasValue && _synthSlotA.Value == kv.Key)
                            || (_synthSlotB.HasValue && _synthSlotB.Value == kv.Key);
                var equipped = kv.Key == _stats.EquippedInvSlot && _stats.IsWeaponSlot(kv.Key);
                var eligible = IsSynthEligible(kv.Key);
                kv.Value.color = SlotBgColor(slot, selected, isMat, equipped, eligible);
            }
        }

        private void OnSynthClicked()
        {
            if (_stats == null)
            {
                return;
            }

            HideForgeOverlay();
            CombatAudio.UiClick();
            ShowSynthOverlay();
        }

        private void ShowSynthOverlay()
        {
            if (_synthOverlay == null)
            {
                return;
            }

            _synthSlotA = null;
            _synthSlotB = null;
            SetSynthError("");
            _synthOverlay.SetActive(true);
            _synthOverlay.transform.SetAsLastSibling();
            RefreshSynthWizardUI();
            RebuildWeapons();
            FindFirstObjectByType<HudUI>()?.SetHint("합성 · 왼쪽에서 무기 2개를 차례로 고르세요");
        }

        private void HideSynthOverlay(bool refreshUi = true)
        {
            var wasOpen = IsSynthOverlayOpen;
            _synthSlotA = null;
            _synthSlotB = null;
            SetSynthError("");
            if (_synthOverlay != null)
            {
                _synthOverlay.SetActive(false);
            }

            if (!wasOpen || !refreshUi || !_open || IsForgeOverlayOpen)
            {
                return;
            }

            SetActionGuide("가방 · 인벤 강화/합성  |  대장간 · 장착 연마·각인");
            if (_selectedSlot.HasValue)
            {
                SelectSlot(_selectedSlot.Value, refreshCards: true);
            }
            else
            {
                RebuildWeapons();
            }
        }

        private void OnSynthConfirmClicked()
        {
            if (_stats == null || !_synthSlotA.HasValue || !_synthSlotB.HasValue)
            {
                return;
            }

            var slotA = _synthSlotA.Value;
            var slotB = _synthSlotB.Value;
            if (!_stats.TrySynthesizeInvSlots(slotA, slotB, out var msg, out var gained))
            {
                var err = msg.Split('\n')[0];
                SetSynthError(err);
                FindFirstObjectByType<ClearBannerUI>()
                    ?.Show(err, ToastKind.Danger, 2.2f);
                CombatAudio.UiClick();
                return;
            }

            _lastForgeEffect = msg.Replace("\n", "  ·  ");
            CombatAudio.LevelUp();
            FindFirstObjectByType<HudUI>()?.SetHint(msg.Split('\n')[0]);
            FindFirstObjectByType<ClearBannerUI>()
                ?.Show(gained.HasValue
                    ? $"합성  ·  {WeaponCatalog.Get(gained.Value).Name}"
                    : "합성 완료", ToastKind.Success, 1.6f);

            HideSynthOverlay();
            _selectedSlot = _stats.EquippedInvSlot;
            // 결과 무기 슬롯 선택 (강화 합산 반영 후이므로 +0만 찾지 않음)
            if (gained.HasValue)
            {
                var best = -1;
                var bestUp = -1;
                for (var i = 0; i < _stats.BagCapacity; i++)
                {
                    var s = _stats.GetInvSlot(i);
                    if (s.Kind == InvItemKind.Weapon && s.Weapon == gained.Value && s.upgrade >= bestUp)
                    {
                        bestUp = s.upgrade;
                        best = i;
                    }
                }

                if (best >= 0)
                {
                    _selectedSlot = best;
                }
            }

            _lastForgeEffect = "";
            Refresh();
            SetForgeEffect("");
            SetActionGuide(gained.HasValue
                ? $"합성 성공! {WeaponCatalog.Get(gained.Value).Name}+{_stats.GetInvSlot(_selectedSlot ?? 0).upgrade} 획득"
                : "합성 완료");
        }

        private void SetActionGuide(string text)
        {
            if (_actionGuide != null)
            {
                _actionGuide.text = text ?? "";
            }
        }

        private void OnForgeClicked()
        {
            if (_stats == null)
            {
                return;
            }

            HideSynthOverlay();

            if (!_selectedSlot.HasValue || !_stats.IsWeaponSlot(_selectedSlot.Value))
            {
                SetActionGuide("먼저 왼쪽에서 무기를 누르세요");
                FindFirstObjectByType<ClearBannerUI>()
                    ?.Show("먼저 왼쪽에서 무기를 누르세요", ToastKind.Info, 1.8f);
                CombatAudio.UiClick();
                return;
            }

            CombatAudio.UiClick();
            ShowForgeOverlay();
        }

        private void ShowForgeOverlay()
        {
            if (_forgeOverlay == null || _stats == null || !_selectedSlot.HasValue)
            {
                return;
            }

            var index = _selectedSlot.Value;
            if (!_stats.IsWeaponSlot(index))
            {
                SetActionGuide("먼저 왼쪽에서 무기를 누르세요");
                return;
            }

            if (!IsForgeOverlayOpen)
            {
                _forgeCount = 1;
            }

            _forgeCount = Mathf.Max(1, _forgeCount);
            RefreshForgeOverlayContent();
            _forgeOverlay.SetActive(true);
            _forgeOverlay.transform.SetAsLastSibling();
        }

        private void AdjustForgeCount(int delta)
        {
            _forgeCount = Mathf.Max(1, _forgeCount + delta);
            ClampForgeCountToAffordable();
            RefreshForgeOverlayContent();
            CombatAudio.UiClick();
        }

        private void SetForgeCountToMax()
        {
            if (_stats == null || !_selectedSlot.HasValue || !_stats.IsWeaponSlot(_selectedSlot.Value))
            {
                return;
            }

            var slot = _stats.GetInvSlot(_selectedSlot.Value);
            var forgeCap = WeaponCatalog.MaxUpgradeAllowed(slot.Weapon, _stats.Level);
            var maxN = WeaponCatalog.MaxAffordableForgeAttempts(
                _stats.Gold, slot.upgrade, _stats.Level, forgeCap, false, slot.temper);
            _forgeCount = Mathf.Max(1, maxN);
            RefreshForgeOverlayContent();
            CombatAudio.UiClick();
        }

        private void ClampForgeCountToAffordable()
        {
            if (_stats == null || !_selectedSlot.HasValue || !_stats.IsWeaponSlot(_selectedSlot.Value))
            {
                _forgeCount = Mathf.Max(1, _forgeCount);
                return;
            }

            var slot = _stats.GetInvSlot(_selectedSlot.Value);
            var forgeCap = WeaponCatalog.MaxUpgradeAllowed(slot.Weapon, _stats.Level);
            var room = Mathf.Max(1, forgeCap - slot.upgrade);
            // 상한까지만 선택 가능. 골드 부족 시 버튼이 비활성화된다.
            _forgeCount = Mathf.Clamp(_forgeCount, 1, room);
        }

        private void RefreshForgeOverlayContent()
        {
            if (_forgeOverlayBody == null || _stats == null || !_selectedSlot.HasValue ||
                !_stats.IsWeaponSlot(_selectedSlot.Value))
            {
                return;
            }

            var index = _selectedSlot.Value;
            var slot = _stats.GetInvSlot(index);
            var id = slot.Weapon;
            var up = slot.upgrade;
            var temper = slot.temper;
            var forgeCap = WeaponCatalog.MaxUpgradeAllowed(id, _stats.Level);
            var cost = WeaponCatalog.UpgradeCost(up, _stats.Level);
            var precCost = WeaponCatalog.PrecisionCost(up, _stats.Level);
            var name = WeaponCatalog.Get(id).Name;
            var canForge = up < forgeCap;
            var canPrecision = canForge && temper < WeaponCatalog.MaxTemper;
            var count = Mathf.Max(1, _forgeCount);

            var maxNormal = WeaponCatalog.MaxAffordableForgeAttempts(
                _stats.Gold, up, _stats.Level, forgeCap, false, temper);
            var maxPrec = WeaponCatalog.MaxAffordableForgeAttempts(
                _stats.Gold, up, _stats.Level, forgeCap, true, temper);
            var sumNormal = WeaponCatalog.EstimateSequentialForgeCost(
                up, count, _stats.Level, forgeCap, false, temper);
            var sumPrec = WeaponCatalog.EstimateSequentialForgeCost(
                up, count, _stats.Level, forgeCap, true, temper);
            var expectUp = Mathf.Min(forgeCap, up + count);

            if (_forgeCountLabel != null)
            {
                _forgeCountLabel.text = $"{count}회";
            }

            if (!canForge)
            {
                _forgeOverlayBody.text =
                    $"{name}\n현재 +{up}  ·  강화 상한 도달\n(캐릭터 Lv {_stats.Level})\n\n보유 골드 {_stats.Gold}G\n\n" +
                    "자세한 규칙은 오른쪽 위 [설명]을 보세요";
            }
            else
            {
                _forgeOverlayBody.text =
                    $"{name}  (+{up}  ·  템퍼 ⋆{temper}/{WeaponCatalog.MaxTemper}  ·  상한 +{forgeCap})\n" +
                    $"일반: 저렴 · 보통+1/대성공+2/완벽+3 · 템퍼↑ 없음  ·  {cost}G\n" +
                    $"정밀: 1.65배 · 항상+1 · 템퍼⋆+1  ·  {precCost}G\n" +
                    $"{count}회 예상  ·  일반 {sumNormal}G / 정밀 {sumPrec}G  ·  +{up}→+{expectUp}\n" +
                    $"보유 {_stats.Gold}G  ·  최대 일반 {maxNormal}회 / 정밀 {maxPrec}회  ·  [설명] 자세히";
            }

            if (_forgeConfirmBtn != null)
            {
                // 선택한 횟수만큼 골드로 감당 가능해야 활성화 (부족하면 [최대]로 맞춤)
                var canPay = maxNormal > 0 && count <= maxNormal;
                _forgeConfirmBtn.interactable = canForge && canPay;
                SetButtonLabel(_forgeConfirmBtn, count > 1 ? $"일반 ×{count}" : "일반 강화");
            }

            if (_forgePrecisionBtn != null)
            {
                var canPay = maxPrec > 0 && count <= maxPrec;
                _forgePrecisionBtn.interactable = canPrecision && canPay;
                SetButtonLabel(_forgePrecisionBtn, count > 1 ? $"정밀 ×{count}" : "정밀 강화");
            }

            if (_forgeMaxBtn != null)
            {
                _forgeMaxBtn.interactable = canForge && maxNormal > 0;
            }

            SetActionGuide(canForge
                ? $"{count}회 강화  ·  일반 약 {sumNormal}G  ·  정밀 약 {sumPrec}G"
                : "강화 상한에 도달했습니다");
        }

        private static void SetButtonLabel(Button btn, string label)
        {
            if (btn == null)
            {
                return;
            }

            var t = btn.transform.Find("L")?.GetComponent<Text>();
            if (t != null)
            {
                t.text = label;
            }
        }

        private void HideForgeOverlay()
        {
            var wasOpen = IsForgeOverlayOpen;
            if (_forgeHelpOverlay != null)
            {
                _forgeHelpOverlay.SetActive(false);
            }

            if (_forgeOverlay != null)
            {
                _forgeOverlay.SetActive(false);
            }

            if (!wasOpen || !_open || IsSynthOverlayOpen || _stats == null)
            {
                return;
            }

            if (_selectedSlot.HasValue && _stats.IsWeaponSlot(_selectedSlot.Value))
            {
                var slot = _stats.GetInvSlot(_selectedSlot.Value);
                var cost = WeaponCatalog.UpgradeCost(slot.upgrade, _stats.Level);
                var cap = WeaponCatalog.MaxUpgradeAllowed(slot.Weapon, _stats.Level);
                SetActionGuide(slot.upgrade < cap
                    ? $"선택됨 · [이 무기 강화]를 누르세요 (비용 {cost}G)"
                    : "강화 상한 · [무기 합성]으로 같은 등급 2개 합성");
            }
            else
            {
                SetActionGuide("가방 · 인벤 강화/합성  |  대장간 · 장착 연마·각인");
            }
        }

        private void OnForgeConfirmClicked(bool precision)
        {
            if (_stats == null || !_selectedSlot.HasValue || !_stats.IsWeaponSlot(_selectedSlot.Value))
            {
                HideForgeOverlay();
                SetActionGuide("먼저 왼쪽에서 무기를 누르세요");
                return;
            }

            var index = _selectedSlot.Value;
            var slot = _stats.GetInvSlot(index);
            var id = slot.Weapon;
            var before = slot.upgrade;
            var beforeBonus = WeaponCatalog.GetWeaponBonus(id, before, slot.temper);
            var forgeCap = WeaponCatalog.MaxUpgradeAllowed(id, _stats.Level);
            var maxAfford = WeaponCatalog.MaxAffordableForgeAttempts(
                _stats.Gold, before, _stats.Level, forgeCap, precision, slot.temper);
            var attempts = Mathf.Clamp(_forgeCount, 1, Mathf.Max(1, maxAfford));
            var estCost = WeaponCatalog.EstimateSequentialForgeCost(
                before, attempts, _stats.Level, forgeCap, precision, slot.temper);

            string msg;
            ForgeResult result;
            int steps;
            int done;
            bool ok;
            if (attempts <= 1)
            {
                ok = _stats.TryForgeInvSlot(index, precision, out msg, out result, out steps);
                done = ok ? 1 : 0;
            }
            else
            {
                ok = _stats.TryForgeInvSlotBatch(index, precision, attempts, out msg, out result,
                    out steps, out done, out _);
            }

            if (!ok)
            {
                var err = msg.Contains("골드") ? "골드가 부족합니다" : msg.Split('\n')[0];
                SetForgeEffect(err);
                FindFirstObjectByType<ClearBannerUI>()
                    ?.Show(err, ToastKind.Danger, 2.2f);
                CombatAudio.UiClick();
                HideForgeOverlay();
                Refresh();
                SetActionGuide(msg.Contains("골드")
                    ? $"골드 부족 · 약 {estCost}G 필요"
                    : msg.Split('\n')[0]);
                return;
            }

            var afterSlot = _stats.GetInvSlot(index);
            var after = afterSlot.upgrade;
            var afterBonus = WeaponCatalog.GetWeaponBonus(id, after, afterSlot.temper);
            var fxTag = result switch
            {
                ForgeResult.Perfect => "✦ 완벽!",
                ForgeResult.Great => "◆ 대성공!",
                _ => done > 1 ? $"연속 {done}회" : "연마 성공"
            };
            _lastForgeEffect =
                $"{fxTag}  +{before}→+{after} (+{steps})  ·  보너스 +{beforeBonus}→+{afterBonus}  ·  ATK {_stats.AttackPower}";

            CombatAudio.UiClick();
            if (result == ForgeResult.Perfect || result == ForgeResult.Great)
            {
                CombatAudio.LevelUp();
            }
            else
            {
                CombatAudio.Coin();
            }

            QuestCatalog.NotifyUpgrade();
            FindFirstObjectByType<HudUI>()?.SetHint(msg);
            FindFirstObjectByType<HudUI>()?.RefreshQuest();
            HideForgeOverlay();
            _forgeCount = 1;
            Refresh();
            ShowForgeResultReveal(id, before, after, beforeBonus, afterBonus, steps, result);
            SetForgeEffect($"{fxTag}  +{before}→+{after}  ·  보너스 +{beforeBonus}→+{afterBonus}");
            SetActionGuide("다시 [이 무기 강화]로 추가 연마");
        }

        private void ShowForgeResultReveal(WeaponId id, int before, int after, int beforeBonus,
            int afterBonus, int steps, ForgeResult result)
        {
            if (_forgeResultOverlay == null || _stats == null)
            {
                return;
            }

            var def = WeaponCatalog.Get(id);
            if (_forgeResultBefore != null)
            {
                _forgeResultBefore.text = $"{def.Name} +{before}";
            }

            if (_forgeResultAfter != null)
            {
                _forgeResultAfter.text = $"{def.Name} +{after}";
                _forgeResultAfter.color = result switch
                {
                    ForgeResult.Perfect => new Color(1f, 0.92f, 0.45f),
                    ForgeResult.Great => new Color(0.85f, 0.95f, 0.55f),
                    _ => new Color(1f, 0.9f, 0.55f)
                };
            }

            var title = result switch
            {
                ForgeResult.Perfect => "완벽 강화!",
                ForgeResult.Great => "대성공!",
                _ when steps > 1 => $"강화 성공! (+{steps})",
                _ => "강화 성공!"
            };
            var subtitle = result switch
            {
                ForgeResult.Perfect => "한 번에 크게 성장했습니다",
                ForgeResult.Great => "예상보다 더 강하게 연마되었습니다",
                _ when steps > 1 => $"단번에 +{steps}단계 상승",
                _ => "무기가 한 단계 더 강해졌습니다"
            };

            if (_forgeResultTitle != null)
            {
                _forgeResultTitle.text = title;
            }

            if (_forgeResultSubtitle != null)
            {
                _forgeResultSubtitle.text = subtitle;
            }

            if (_forgeResultIcon != null)
            {
                _forgeResultIcon.sprite = UiItemIcon.ForWeapon(id);
                _forgeResultIcon.color = Color.white;
            }

            var atkBefore = Mathf.RoundToInt(
                (_stats.BaseAttackPower + beforeBonus + _stats.MetaBlessing) * _stats.AttackMultiplier);
            var atkAfter = _stats.AttackPower;
            if (_forgeResultStats != null)
            {
                _forgeResultStats.text =
                    $"강화  +{before}  →  +{after}" + (steps > 1 ? $"  (+{steps})" : "") + "\n" +
                    $"무기 보너스  +{beforeBonus}  →  +{afterBonus}   ·   ATK  {atkBefore}  →  {atkAfter}";
            }

            _forgeResultOverlay.SetActive(true);
            _forgeResultOverlay.transform.SetAsLastSibling();
            StartCoroutine(UiPanelMotion.FadeIn(_forgeResultOverlay, 0.22f, 20f));
            FindFirstObjectByType<ClearBannerUI>()
                ?.Show($"{def.Name}+{after}  강화", ToastKind.Success, 1.6f);
        }

        private void HideForgeResultOverlay()
        {
            if (_forgeResultOverlay != null)
            {
                _forgeResultOverlay.SetActive(false);
            }
        }

        private void SetSynthButtonState(bool enabled, string label)
        {
            if (_synthBtn != null)
            {
                _synthBtn.interactable = enabled;
            }

            if (_synthBtnBg != null)
            {
                _synthBtnBg.color = enabled ? BtnSynth : BtnDisabled;
            }

            if (_synthBtnLabel != null)
            {
                _synthBtnLabel.text = label;
                _synthBtnLabel.color = enabled
                    ? new Color(0.98f, 0.9f, 0.72f)
                    : new Color(0.5f, 0.45f, 0.4f);
            }
        }

        private void SetForgeEffect(string text)
        {
            if (_forgeEffectText == null)
            {
                return;
            }

            _forgeEffectText.text = text ?? "";
            _forgeEffectText.color = string.IsNullOrEmpty(text)
                ? new Color(1f, 0.78f, 0.35f, 0f)
                : new Color(1f, 0.82f, 0.4f, 1f);
        }

        private void SetForgeButtonState(bool enabled, string label)
        {
            if (_forgeBtn != null)
            {
                _forgeBtn.interactable = enabled;
            }

            if (_forgeBtnBg != null)
            {
                _forgeBtnBg.color = enabled ? BtnForge : BtnDisabled;
            }

            if (_forgeBtnLabel != null)
            {
                _forgeBtnLabel.text = label;
                _forgeBtnLabel.color = enabled
                    ? new Color(1f, 0.92f, 0.75f)
                    : new Color(0.5f, 0.45f, 0.4f);
            }
        }

        private void OnEquipClicked()
        {
            if (IsSynthOverlayOpen || IsForgeOverlayOpen)
            {
                FindFirstObjectByType<ClearBannerUI>()
                    ?.Show("확인 창을 먼저 닫아주세요", ToastKind.Info, 1.8f);
                return;
            }

            if (_stats == null)
            {
                return;
            }

            // 스킬 교체 모드
            if (_skillPickWeapon.HasValue)
            {
                var w = _skillPickWeapon.Value;
                if (_stats.SetSkillSourceWeapon(w, out var skillMsg))
                {
                    CombatAudio.UiClick();
                    FindFirstObjectByType<HudUI>()?.SetHint(skillMsg);
                    FindFirstObjectByType<ClearBannerUI>()
                        ?.Show(skillMsg, ToastKind.Success, 1.2f);
                    _skillPickWeapon = null;
                    Refresh();
                    ShowDetail($"사용 중  ·  {SkillCatalog.WeaponSkillName(w)}",
                        SkillCatalog.DetailForWeapon(w));
                    SetEquipButtonState(false, "사용 중");
                    SetActionGuide($"{SkillCatalog.WeaponSkillName(w)} 장착  ·  R 키로 발동");
                }
                else
                {
                    FindFirstObjectByType<HudUI>()?.SetHint(skillMsg);
                    CombatAudio.UiClick();
                }

                return;
            }

            if (!_selectedSlot.HasValue)
            {
                return;
            }

            var index = _selectedSlot.Value;
            var slot = _stats.GetInvSlot(index);

            if (slot.Kind == InvItemKind.Potion)
            {
                if (_stats.TryUsePotion())
                {
                    CombatAudio.UiClick();
                    FindFirstObjectByType<HudUI>()?.SetHint("포션 사용!");
                    _lastForgeEffect = "";
                    Refresh();
                }
                else
                {
                    var msg = _stats.Potions <= 0 ? "포션이 없다" : "체력이 이미 가득하다";
                    FindFirstObjectByType<HudUI>()?.SetHint(msg);
                }

                return;
            }

            if (!_stats.IsWeaponSlot(index))
            {
                return;
            }

            if (index == _stats.EquippedInvSlot)
            {
                FindFirstObjectByType<ClearBannerUI>()
                    ?.Show("이미 장착 중인 무기", ToastKind.Info, 1.6f);
                return;
            }

            if (_stats.TryEquipInvSlot(index, out var equipMsg))
            {
                CombatAudio.UiClick();
                FindFirstObjectByType<HudUI>()?.SetHint(equipMsg);
                FindFirstObjectByType<ClearBannerUI>()
                    ?.Show($"장착  ·  {_stats.WeaponLabel}", ToastKind.Success, 1.2f);
                _lastForgeEffect = "";
                Refresh();
            }
            else
            {
                FindFirstObjectByType<HudUI>()?.SetHint(equipMsg);
            }
        }

        private void OnSellClicked()
        {
            if (_stats == null)
            {
                return;
            }

            HideForgeOverlay();
            HideSynthOverlay(refreshUi: false);

            if (!_selectedSlot.HasValue || !_stats.IsValidSlot(_selectedSlot.Value) ||
                _stats.GetInvSlot(_selectedSlot.Value).IsEmpty)
            {
                SetActionGuide("판매할 슬롯을 먼저 선택하세요");
                CombatAudio.UiClick();
                return;
            }

            var index = _selectedSlot.Value;
            if (!_stats.TrySellInvSlot(index, out var msg, out var gold))
            {
                SetActionGuide(msg);
                FindFirstObjectByType<ClearBannerUI>()
                    ?.Show(msg.Split('\n')[0], ToastKind.Danger, 2f);
                CombatAudio.UiClick();
                return;
            }

            CombatAudio.Coin();
            FindFirstObjectByType<HudUI>()?.SetHint(msg);
            FindFirstObjectByType<ClearBannerUI>()
                ?.Show($"판매  ·  +{gold}G", ToastKind.Success, 1.4f);
            _selectedSlot = _stats.EquippedInvSlot;
            _lastForgeEffect = "";
            Refresh();
            SetActionGuide(msg);
        }

        private void SetSellButtonState(bool enabled, string label)
        {
            if (_sellBtn != null)
            {
                _sellBtn.interactable = enabled;
            }

            if (_sellBtnBg != null)
            {
                _sellBtnBg.color = enabled ? BtnSell : BtnDisabled;
            }

            if (_sellBtnLabel != null)
            {
                _sellBtnLabel.text = label;
                _sellBtnLabel.color = enabled
                    ? new Color(0.98f, 0.88f, 0.75f)
                    : new Color(0.5f, 0.45f, 0.4f);
            }
        }

        private void SetEquipButtonState(bool enabled, string label)
        {
            if (_equipBtn != null)
            {
                _equipBtn.interactable = enabled;
            }

            if (_equipBtnBg != null)
            {
                _equipBtnBg.color = enabled ? BtnEquip : BtnDisabled;
            }

            if (_equipBtnLabel != null)
            {
                _equipBtnLabel.text = label;
                _equipBtnLabel.color = enabled
                    ? new Color(0.92f, 0.95f, 0.8f)
                    : new Color(0.5f, 0.48f, 0.42f);
            }
        }

        private void OnSkillClick(SkillId id)
        {
            if (_stats == null)
            {
                return;
            }

            var def = SkillCatalog.Get(id);
            var equipped = id == _stats.EquippedSkill;
            ShowDetail(equipped ? $"장착 중  ·  {def.Name}" : $"스킬  ·  {def.Name}",
                def.DetailBody(equipped));
            SetEquipButtonState(false, equipped ? "장착 중" : "장착됨");
            SetSellButtonState(false, "판매");
            _stats.SetEquippedSkill(id);
            var skill = _stats.GetComponent<PlayerSkill>();
            skill?.RefreshFromStats();
            CombatAudio.UiClick();
            FindFirstObjectByType<HudUI>()?.SetHint($"{def.Name}  ·  {def.Role}");
            FindFirstObjectByType<ClearBannerUI>()
                ?.Show($"스킬 장착  ·  {def.Name}", ToastKind.Info, 1.2f);
            Refresh();
            // Refresh 후 상세가 덮일 수 있어 다시 표시
            ShowDetail($"장착 중  ·  {def.Name}", def.DetailBody(true));
            SetActionGuide($"{def.Name} 장착  ·  R 키로 발동  ·  {def.Advantage}");
        }

        private void ShowDetail(string title, string body)
        {
            if (_detailTitle != null)
            {
                _detailTitle.text = title ?? "";
            }

            if (_detailBody != null)
            {
                _detailBody.text = body ?? "";
            }
        }

        /// <summary>추적 카드만 파괴. Content의 GridLayoutGroup 호스트는 유지.</summary>
        private static void ClearChildren(List<GameObject> list, Transform parent)
        {
            foreach (var go in list)
            {
                if (go != null)
                {
                    Object.Destroy(go);
                }
            }

            list.Clear();
            if (parent == null)
            {
                return;
            }

            // Content의 자식(슬롯 카드)만 제거 — GridLayoutGroup 컴포넌트는 Content에 남아 있음
            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(parent.GetChild(i).gameObject);
            }
        }

        private static GameObject CreateDim(Transform parent)
        {
            var go = new GameObject("BagDim");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = new Color(0.02f, 0.015f, 0.01f, 0.65f);
            return go;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = PanelBg;
            var outline = go.AddComponent<Outline>();
            outline.effectColor = EmberGold;
            outline.effectDistance = new Vector2(2f, -2f);
            return go;
        }

        /// <summary>
        /// 스크롤 뷰포트 + Content(GridLayoutGroup + ContentSizeFitter). Content Transform을 반환.
        /// </summary>
        private static Transform CreateScrollArea(Transform parent, string name, Vector2 pos, Vector2 size,
            out ScrollRect scroll)
        {
            var viewport = new GameObject(name);
            viewport.transform.SetParent(parent, false);
            var vRt = viewport.AddComponent<RectTransform>();
            vRt.anchoredPosition = pos;
            vRt.sizeDelta = size;
            var vImg = viewport.AddComponent<Image>();
            vImg.color = new Color(0.05f, 0.04f, 0.03f, 0.75f);
            viewport.AddComponent<Mask>().showMaskGraphic = true;
            scroll = viewport.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 28f;

            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var cRt = content.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0f, 1f);
            cRt.anchorMax = new Vector2(1f, 1f);
            cRt.pivot = new Vector2(0.5f, 1f);
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = new Vector2(0f, size.y);

            var grid = content.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(SlotCellW, SlotCellH);
            grid.spacing = new Vector2(SlotGap, SlotGap);
            grid.padding = new RectOffset(12, 12, 8, 8);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = SlotCols;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;

            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = cRt;
            scroll.viewport = vRt;
            return cRt;
        }

        /// <summary>스킬 목록 — VerticalLayoutGroup + 스크롤.</summary>
        private static Transform CreateSkillList(Transform parent, string name, Vector2 pos, Vector2 size,
            out ScrollRect scroll)
        {
            var viewport = new GameObject(name);
            viewport.transform.SetParent(parent, false);
            var vRt = viewport.AddComponent<RectTransform>();
            vRt.anchoredPosition = pos;
            vRt.sizeDelta = size;
            var vImg = viewport.AddComponent<Image>();
            vImg.color = new Color(0.06f, 0.05f, 0.04f, 0.7f);
            viewport.AddComponent<Mask>().showMaskGraphic = true;
            var frameOl = viewport.AddComponent<Outline>();
            frameOl.effectColor = new Color(0.4f, 0.28f, 0.14f, 0.35f);
            frameOl.effectDistance = new Vector2(1f, -1f);

            scroll = viewport.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 24f;

            var content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            var cRt = content.AddComponent<RectTransform>();
            cRt.anchorMin = new Vector2(0f, 1f);
            cRt.anchorMax = new Vector2(1f, 1f);
            cRt.pivot = new Vector2(0.5f, 1f);
            cRt.anchoredPosition = Vector2.zero;
            cRt.sizeDelta = new Vector2(0f, size.y);

            var vertical = content.AddComponent<VerticalLayoutGroup>();
            vertical.padding = new RectOffset(6, 6, 5, 5);
            vertical.spacing = 4f;
            vertical.childAlignment = TextAnchor.UpperCenter;
            vertical.childControlWidth = true;
            vertical.childControlHeight = false;
            vertical.childForceExpandWidth = true;
            vertical.childForceExpandHeight = false;

            var fitter = content.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = cRt;
            scroll.viewport = vRt;
            return cRt;
        }

        private static GameObject CreateActionButton(Transform parent, string name, string label,
            Vector2 pos, Vector2 size, Color bg, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.color = bg;

            var ol = go.AddComponent<Outline>();
            ol.effectColor = new Color(0.75f, 0.5f, 0.22f, 0.45f);
            ol.effectDistance = new Vector2(1f, -1f);

            var rail = new GameObject("Rail");
            rail.transform.SetParent(go.transform, false);
            var railRt = rail.AddComponent<RectTransform>();
            railRt.anchorMin = new Vector2(0f, 0f);
            railRt.anchorMax = new Vector2(0f, 1f);
            railRt.pivot = new Vector2(0f, 0.5f);
            railRt.sizeDelta = new Vector2(3f, -6f);
            railRt.anchoredPosition = new Vector2(3f, 0f);
            var railImg = rail.AddComponent<Image>();
            railImg.color = new Color(0.45f, 0.9f, 0.86f, 0.85f);
            railImg.raycastTarget = false;

            var text = MakeText(go.transform, "L", label, 14, Vector2.zero,
                size - new Vector2(10f, 8f), new Color(0.98f, 0.9f, 0.72f), FontStyle.Bold);
            text.alignment = TextAnchor.MiddleCenter;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.highlightedColor = new Color(1.15f, 1.1f, 1.05f, 1f);
            colors.pressedColor = new Color(0.8f, 0.75f, 0.7f, 1f);
            colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.6f);
            btn.colors = colors;
            btn.onClick.AddListener(onClick);
            return go;
        }

        private static Transform CreateContentRoot(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        /// <summary>GridLayout 자식용 슬롯 — LayoutElement만 설정, anchoredPosition 수동 지정 없음.</summary>
        private static GameObject CreateGridSlot(Transform parent, string name, Color bg,
            UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(SlotCellW, SlotCellH);

            var le = go.AddComponent<LayoutElement>();
            le.minWidth = SlotCellW;
            le.minHeight = SlotCellH;
            le.preferredWidth = SlotCellW;
            le.preferredHeight = SlotCellH;

            var img = go.AddComponent<Image>();
            img.color = bg;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.highlightedColor = new Color(bg.r + 0.1f, bg.g + 0.08f, bg.b + 0.05f, 1f);
            colors.pressedColor = new Color(0.35f, 0.25f, 0.15f, 1f);
            btn.colors = colors;
            btn.onClick.AddListener(onClick);
            return go;
        }

        private static GameObject CreateCard(Transform parent, string name, Vector2 pos, Vector2 size,
            Color bg, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.color = bg;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.highlightedColor = new Color(bg.r + 0.08f, bg.g + 0.08f, bg.b + 0.06f, 1f);
            colors.pressedColor = new Color(0.3f, 0.22f, 0.14f, 1f);
            btn.colors = colors;
            btn.onClick.AddListener(onClick);
            return go;
        }

        private static Button CreateBtn(Transform parent, string name, string label, Vector2 pos, Vector2 size,
            UnityEngine.Events.UnityAction onClick)
        {
            var go = CreateCard(parent, name, pos, size, new Color(0.22f, 0.16f, 0.12f, 1f), onClick);
            MakeText(go.transform, "L", label, 14, Vector2.zero, size - new Vector2(8f, 8f),
                new Color(0.9f, 0.8f, 0.65f)).alignment = TextAnchor.MiddleCenter;
            return go.GetComponent<Button>();
        }

        private static Text MakeText(Transform parent, string name, string content, int size, Vector2 pos,
            Vector2 rect, Color color, FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = rect;
            var text = go.AddComponent<Text>();
            text.font = RuntimeUiFactory.UiFont();
            text.text = content;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }
    }
}
