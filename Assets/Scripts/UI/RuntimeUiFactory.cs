using DungeonOdyssey.Art;
using DungeonOdyssey.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DungeonOdyssey.UI
{
    public static class RuntimeUiFactory
    {
        // Contemporary action UI — cool glass · ice accent · coral HP (브라스 장식 배제)
        private static readonly Color Gold = new(0.55f, 0.92f, 0.88f, 1f);           // ice teal accent
        private static readonly Color SoftWhite = new(0.91f, 0.93f, 0.95f, 1f);       // cool paper
        private static readonly Color Muted = new(0.55f, 0.58f, 0.64f, 1f);
        private static readonly Color PanelDark = new(0.06f, 0.07f, 0.09f, 0.78f);    // frosted glass
        private static readonly Color PanelDeep = new(0.04f, 0.045f, 0.06f, 0.92f);
        private static readonly Color Accent = new(0.45f, 0.78f, 0.82f, 1f);
        private static readonly Color HpFill = new(1f, 0.32f, 0.38f, 1f);             // high-vis coral
        private static readonly Color Line = new(0.45f, 0.85f, 0.82f, 0.55f);
        private static readonly Color Currency = new(1f, 0.84f, 0.42f, 1f);           // gold numbers only
        private static readonly Color PrimaryFill = new(0.12f, 0.28f, 0.3f, 0.95f);  // CTA fill

        public static Canvas CreateCanvas(string name)
        {
            EnsureEventSystem();
            var canvasGo = new GameObject(name);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        /// <summary>WebGL 포함 — 한글 글리프가 있는 번들 폰트 우선.</summary>
        public static Font UiFont() => GameFonts.Ui();


        public static Text CreateText(Transform parent, string name, string content, int fontSize, Vector2 anchoredPos,
            Vector2 size, TextAnchor anchor = TextAnchor.MiddleCenter, Color? color = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPos;
            var text = go.AddComponent<Text>();
            text.font = UiFont();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = color ?? SoftWhite;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
            shadow.effectDistance = new Vector2(1f, -1f);
            return text;
        }

        public static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size,
            int fontSize = 24)
        {
            ArtCatalog.EnsureLoaded();
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPos;

            var image = go.AddComponent<Image>();
            image.sprite = null;
            image.color = new Color(0.1f, 0.12f, 0.15f, 0.94f);

            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0.35f, 0.7f, 0.68f, 0.4f);
            outline.effectDistance = new Vector2(1f, -1f);

            // 좌측 악센트 피프 — 모던 UI 관례
            var pip = new GameObject("AccentPip");
            pip.transform.SetParent(go.transform, false);
            var pipRt = pip.AddComponent<RectTransform>();
            pipRt.anchorMin = new Vector2(0f, 0.15f);
            pipRt.anchorMax = new Vector2(0f, 0.85f);
            pipRt.pivot = new Vector2(0f, 0.5f);
            pipRt.sizeDelta = new Vector2(3f, 0f);
            pipRt.anchoredPosition = new Vector2(0f, 0f);
            pip.AddComponent<Image>().color = Gold;

            var button = go.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.12f, 1.12f, 1.14f, 1f);
            colors.pressedColor = new Color(0.78f, 0.82f, 0.84f, 1f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            var labelSize = new Vector2(Mathf.Max(8f, size.x - 16f), Mathf.Max(8f, size.y - 6f));
            var labelText = CreateText(go.transform, "Label", label, fontSize, new Vector2(4f, 0f), labelSize,
                TextAnchor.MiddleCenter, SoftWhite);
            labelText.horizontalOverflow = HorizontalWrapMode.Overflow;
            labelText.verticalOverflow = VerticalWrapMode.Truncate;
            labelText.resizeTextForBestFit = true;
            labelText.resizeTextMinSize = 12;
            labelText.resizeTextMaxSize = fontSize;
            return button;
        }

        public static Slider CreateHpSlider(Transform parent, Vector2 anchoredPos, Vector2 size)
        {
            var slider = CreateBarSlider(parent, "HpSlider", anchoredPos, size, HpFill);
            // HP 전용 — 트랙 대비 · 밝은 림
            var bg = slider.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = new Color(0.08f, 0.02f, 0.04f, 0.95f);
            }

            var edge = slider.GetComponent<Outline>();
            if (edge != null)
            {
                edge.effectColor = new Color(1f, 0.45f, 0.5f, 0.55f);
                edge.effectDistance = new Vector2(1.5f, -1.5f);
            }

            var fillImg = slider.fillRect != null ? slider.fillRect.GetComponent<Image>() : null;
            if (fillImg != null)
            {
                fillImg.color = HpFill;
                var glow = fillImg.gameObject.GetComponent<Outline>()
                            ?? fillImg.gameObject.AddComponent<Outline>();
                glow.effectColor = new Color(1f, 0.55f, 0.5f, 0.35f);
                glow.effectDistance = new Vector2(0f, 1f);
            }

            return slider;
        }

        public static Slider CreateBarSlider(Transform parent, string name, Vector2 anchoredPos, Vector2 size,
            Color fillColor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPos;

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.02f, 0.025f, 0.035f, 0.88f);
            var barEdge = go.AddComponent<Outline>();
            barEdge.effectColor = new Color(1f, 1f, 1f, 0.08f);
            barEdge.effectDistance = new Vector2(1f, -1f);

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(1f, 1f);
            fillAreaRect.offsetMax = new Vector2(-1f, -1f);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = fillColor;

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.targetGraphic = fillImage;
            slider.minValue = 0;
            slider.maxValue = 100;
            slider.value = 100;
            slider.interactable = false;
            return slider;
        }

        public static TitleUI BuildTitleUi()
        {
            ArtCatalog.EnsureLoaded();
            var canvas = CreateCanvas("TitleCanvas");
            // 3D 분위기가 비치도록 오버레이만 — 불투명 배경 제거
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var root = canvas.transform;

            var fadeGroup = canvas.gameObject.AddComponent<CanvasGroup>();

            // 하단 그라데이션만 — 상단은 3D 히어로를 열어둠 (모던 풀블리드)
            CreateFullScreen(root, "VignetteBottom", new Color(0.02f, 0.03f, 0.045f, 0.82f));
            var vigBot = root.Find("VignetteBottom").GetComponent<RectTransform>();
            vigBot.anchorMin = Vector2.zero;
            vigBot.anchorMax = new Vector2(1f, 0.48f);
            vigBot.offsetMin = Vector2.zero;
            vigBot.offsetMax = Vector2.zero;

            CreateFullScreen(root, "SideShadeL", new Color(0.01f, 0.015f, 0.025f, 0.28f));
            var sideL = root.Find("SideShadeL").GetComponent<RectTransform>();
            sideL.anchorMin = Vector2.zero;
            sideL.anchorMax = new Vector2(0.12f, 1f);
            sideL.offsetMin = Vector2.zero;
            sideL.offsetMax = Vector2.zero;

            CreateFullScreen(root, "SideShadeR", new Color(0.01f, 0.015f, 0.025f, 0.28f));
            var sideR = root.Find("SideShadeR").GetComponent<RectTransform>();
            sideR.anchorMin = new Vector2(0.88f, 0f);
            sideR.anchorMax = Vector2.one;
            sideR.offsetMin = Vector2.zero;
            sideR.offsetMax = Vector2.zero;

            // —— 브랜드 히어로 ——
            var brand = new GameObject("Brand");
            brand.transform.SetParent(root, false);
            var brandRt = brand.AddComponent<RectTransform>();
            brandRt.anchorMin = new Vector2(0.5f, 0.76f);
            brandRt.anchorMax = new Vector2(0.5f, 0.76f);
            brandRt.pivot = new Vector2(0.5f, 0.5f);
            brandRt.sizeDelta = new Vector2(920f, 150f);
            brandRt.anchoredPosition = Vector2.zero;

            var brandMark = CreateText(brand.transform, "Mark", "DUNGEON ODYSSEY", 12,
                new Vector2(0f, 48f), new Vector2(640f, 20f), TextAnchor.MiddleCenter,
                new Color(0.45f, 0.88f, 0.84f, 0.95f));
            brandMark.fontStyle = FontStyle.Bold;

            var rule = new GameObject("Rule");
            rule.transform.SetParent(brand.transform, false);
            var ruleRt = rule.AddComponent<RectTransform>();
            ruleRt.sizeDelta = new Vector2(48f, 2f);
            ruleRt.anchoredPosition = new Vector2(0f, 30f);
            rule.AddComponent<Image>().color = new Color(0.45f, 0.9f, 0.86f, 0.85f);

            var title = CreateText(brand.transform, "Title", "던전 오디세이", 64,
                new Vector2(0f, -4f), new Vector2(920f, 78f), TextAnchor.MiddleCenter,
                SoftWhite);
            title.fontStyle = FontStyle.Bold;
            var titleOutline = title.gameObject.AddComponent<Outline>();
            titleOutline.effectColor = new Color(0.02f, 0.04f, 0.06f, 0.7f);
            titleOutline.effectDistance = new Vector2(2f, -2f);

            CreateText(brand.transform, "Tagline", "끝없는 지하로 향하는 여정", 16,
                new Vector2(0f, -52f), new Vector2(720f, 26f), TextAnchor.MiddleCenter,
                new Color(0.62f, 0.68f, 0.74f, 0.92f));

            // —— CTA / 인증 (하단 카드, 푸터와 안 겹치게) ——
            var cta = new GameObject("Cta");
            cta.transform.SetParent(root, false);
            var ctaRt = cta.AddComponent<RectTransform>();
            ctaRt.anchorMin = new Vector2(0.5f, 0.36f);
            ctaRt.anchorMax = new Vector2(0.5f, 0.36f);
            ctaRt.pivot = new Vector2(0.5f, 0.5f);
            ctaRt.sizeDelta = new Vector2(380f, 340f);
            ctaRt.anchoredPosition = Vector2.zero;

            // Auth card
            var authRoot = new GameObject("AuthRoot");
            authRoot.transform.SetParent(cta.transform, false);
            var authRt = authRoot.AddComponent<RectTransform>();
            authRt.anchorMin = new Vector2(0.5f, 0.5f);
            authRt.anchorMax = new Vector2(0.5f, 0.5f);
            authRt.pivot = new Vector2(0.5f, 0.5f);
            authRt.sizeDelta = new Vector2(360f, 330f);
            authRt.anchoredPosition = Vector2.zero;
            var authBg = authRoot.AddComponent<Image>();
            authBg.color = new Color(0.045f, 0.055f, 0.07f, 0.88f);
            var authOutline = authRoot.AddComponent<Outline>();
            authOutline.effectColor = new Color(0.4f, 0.78f, 0.75f, 0.35f);
            authOutline.effectDistance = new Vector2(1f, -1f);

            CreateText(authRoot.transform, "AuthHeading", "계정", 11,
                new Vector2(0f, 122f), new Vector2(300f, 18f), TextAnchor.MiddleCenter,
                new Color(0.5f, 0.78f, 0.75f, 0.9f));

            var modeLogin = CreateTitleGhostButton(authRoot.transform, "ModeLogin", "로그인",
                new Vector2(-58f, 92f), new Vector2(100f, 28f));
            var modeSignUp = CreateTitleGhostButton(authRoot.transform, "ModeSignUp", "회원가입",
                new Vector2(58f, 92f), new Vector2(100f, 28f));

            var tabRule = new GameObject("TabRule");
            tabRule.transform.SetParent(authRoot.transform, false);
            var tabRuleRt = tabRule.AddComponent<RectTransform>();
            tabRuleRt.sizeDelta = new Vector2(280f, 1f);
            tabRuleRt.anchoredPosition = new Vector2(0f, 74f);
            tabRule.AddComponent<Image>().color = new Color(0.35f, 0.45f, 0.48f, 0.45f);

            var userField = CreateInputField(authRoot.transform, "UserField", "아이디",
                new Vector2(0f, 40f), new Vector2(280f, 40f));
            var passField = CreateInputField(authRoot.transform, "PassField", "비밀번호",
                new Vector2(0f, -8f), new Vector2(280f, 40f), password: true);
            var displayField = CreateInputField(authRoot.transform, "DisplayField", "모험가 이름 (선택)",
                new Vector2(0f, -56f), new Vector2(280f, 40f));
            displayField.gameObject.SetActive(false);

            var submitBtn = CreateTitleButton(authRoot.transform, "SubmitButton", "로그인",
                new Vector2(0f, -72f), new Vector2(280f, 44f), primary: true);
            var guestBtn = CreateTitleGhostButton(authRoot.transform, "GuestButton", "게스트로 시작",
                new Vector2(0f, -118f), new Vector2(200f, 26f));

            var authStatus = CreateText(authRoot.transform, "AuthStatus", "", 12,
                new Vector2(0f, -148f), new Vector2(300f, 28f), TextAnchor.MiddleCenter,
                new Color(0.62f, 0.72f, 0.74f, 0.95f));

            // Play panel (slots)
            var playRoot = new GameObject("PlayRoot");
            playRoot.transform.SetParent(cta.transform, false);
            var playRt = playRoot.AddComponent<RectTransform>();
            playRt.anchorMin = new Vector2(0.5f, 0.5f);
            playRt.anchorMax = new Vector2(0.5f, 0.5f);
            playRt.pivot = new Vector2(0.5f, 0.5f);
            playRt.sizeDelta = new Vector2(360f, 330f);
            playRt.anchoredPosition = Vector2.zero;
            var playBg = playRoot.AddComponent<Image>();
            playBg.color = new Color(0.045f, 0.055f, 0.07f, 0.88f);
            var playOutline = playRoot.AddComponent<Outline>();
            playOutline.effectColor = new Color(0.4f, 0.78f, 0.75f, 0.35f);
            playOutline.effectDistance = new Vector2(1f, -1f);
            playRoot.SetActive(false);

            var accountLabel = CreateText(playRoot.transform, "AccountLabel", "", 13,
                new Vector2(-36f, 118f), new Vector2(220f, 24f), TextAnchor.MiddleLeft,
                new Color(0.55f, 0.9f, 0.85f, 0.95f));
            var logoutBtn = CreateTitleGhostButton(playRoot.transform, "LogoutButton", "로그아웃",
                new Vector2(120f, 118f), new Vector2(90f, 26f));

            var slot1 = CreateTitleButton(playRoot.transform, "Slot1", "슬롯 1",
                new Vector2(-100f, 72f), new Vector2(92f, 34f), primary: false);
            var slot2 = CreateTitleButton(playRoot.transform, "Slot2", "슬롯 2",
                new Vector2(0f, 72f), new Vector2(92f, 34f), primary: false);
            var slot3 = CreateTitleButton(playRoot.transform, "Slot3", "슬롯 3",
                new Vector2(100f, 72f), new Vector2(92f, 34f), primary: false);

            var newGame = CreateTitleButton(playRoot.transform, "NewGameButton", "새 게임",
                new Vector2(0f, 18f), new Vector2(280f, 46f), primary: true);
            var cont = CreateTitleButton(playRoot.transform, "ContinueButton", "이어하기",
                new Vector2(0f, -32f), new Vector2(280f, 40f), primary: false);
            var quit = CreateTitleButton(playRoot.transform, "QuitButton", "종료",
                new Vector2(0f, -78f), new Vector2(280f, 40f), primary: false);

            var status = CreateText(playRoot.transform, "Status", "", 12, new Vector2(0f, -124f),
                new Vector2(320f, 44f), TextAnchor.MiddleCenter, new Color(0.62f, 0.72f, 0.74f, 0.95f));

            var footerText = CreateText(root, "Footer", "WASD  ·  Space 공격  ·  R 스킬  ·  Tab/V 교체  ·  E 상호작용", 11,
                Vector2.zero, new Vector2(780f, 20f), TextAnchor.MiddleCenter,
                new Color(0.42f, 0.46f, 0.52f, 0.55f));
            var footer = footerText.rectTransform;
            footer.anchorMin = new Vector2(0.5f, 0f);
            footer.anchorMax = new Vector2(0.5f, 0f);
            footer.pivot = new Vector2(0.5f, 0f);
            footer.anchoredPosition = new Vector2(0f, 10f);

            var motion = canvas.gameObject.AddComponent<TitleMenuMotion>();
            motion.Bind(fadeGroup, brandRt, ctaRt);

            var titleUi = canvas.gameObject.AddComponent<TitleUI>();
            titleUi.Bind(newGame, cont, quit, status, slot1, slot2, slot3);
            titleUi.BindAuth(authRoot, playRoot, userField, passField, displayField, authStatus, accountLabel,
                submitBtn, guestBtn, logoutBtn, modeLogin, modeSignUp);
            return titleUi;
        }

        private static Button CreateTitleGhostButton(Transform parent, string name, string label, Vector2 pos,
            Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = pos;
            var img = go.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.02f);
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.85f, 1f, 0.98f, 1f);
            colors.pressedColor = new Color(0.7f, 0.9f, 0.88f, 1f);
            btn.colors = colors;

            var text = CreateText(go.transform, "Label", label, 14, Vector2.zero, size,
                TextAnchor.MiddleCenter, new Color(0.72f, 0.8f, 0.82f, 0.95f));
            text.raycastTarget = false;
            return btn;
        }

        public static InputField CreateInputField(Transform parent, string name, string placeholder, Vector2 pos,
            Vector2 size, bool password = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = pos;
            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.07f, 0.09f, 0.11f, 0.95f);
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0.35f, 0.7f, 0.68f, 0.45f);
            outline.effectDistance = new Vector2(1f, -1f);

            var input = go.AddComponent<InputField>();
            input.contentType = password ? InputField.ContentType.Password : InputField.ContentType.Standard;
            input.lineType = InputField.LineType.SingleLine;
            input.characterLimit = 24;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(12f, 4f);
            textRt.offsetMax = new Vector2(-12f, -4f);
            var text = textGo.AddComponent<Text>();
            text.font = UiFont();
            text.fontSize = 15;
            text.color = SoftWhite;
            text.alignment = TextAnchor.MiddleLeft;
            text.supportRichText = false;
            input.textComponent = text;

            var phGo = new GameObject("Placeholder");
            phGo.transform.SetParent(go.transform, false);
            var phRt = phGo.AddComponent<RectTransform>();
            phRt.anchorMin = Vector2.zero;
            phRt.anchorMax = Vector2.one;
            phRt.offsetMin = new Vector2(12f, 4f);
            phRt.offsetMax = new Vector2(-12f, -4f);
            var ph = phGo.AddComponent<Text>();
            ph.font = UiFont();
            ph.fontSize = 14;
            ph.fontStyle = FontStyle.Normal;
            ph.color = new Color(0.48f, 0.54f, 0.58f, 0.85f);
            ph.alignment = TextAnchor.MiddleLeft;
            ph.text = placeholder;
            input.placeholder = ph;

            return input;
        }

        private static Button CreateTitleButton(Transform parent, string name, string label, Vector2 pos,
            Vector2 size, bool primary)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = pos;

            var image = go.AddComponent<Image>();
            image.color = primary ? PrimaryFill : new Color(0.08f, 0.09f, 0.12f, 0.72f);

            var btnOutline = go.AddComponent<Outline>();
            btnOutline.effectColor = primary
                ? new Color(0.45f, 0.9f, 0.86f, 0.65f)
                : new Color(1f, 1f, 1f, 0.12f);
            btnOutline.effectDistance = new Vector2(1f, -1f);

            var accentGo = new GameObject("Accent");
            accentGo.transform.SetParent(go.transform, false);
            var accentRt = accentGo.AddComponent<RectTransform>();
            accentRt.anchorMin = new Vector2(0f, 0.2f);
            accentRt.anchorMax = new Vector2(0f, 0.8f);
            accentRt.pivot = new Vector2(0f, 0.5f);
            accentRt.sizeDelta = new Vector2(3f, 0f);
            accentRt.anchoredPosition = new Vector2(0f, 0f);
            var accent = accentGo.AddComponent<Image>();
            accent.color = primary
                ? new Color(0.55f, 0.95f, 0.9f, 1f)
                : new Color(0.45f, 0.7f, 0.72f, 0.45f);

            var button = go.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.18f, 1f);
            colors.pressedColor = new Color(0.8f, 0.85f, 0.86f, 1f);
            colors.disabledColor = new Color(0.45f, 0.45f, 0.48f, 0.4f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;

            var textColor = primary ? SoftWhite : new Color(0.78f, 0.82f, 0.86f, 1f);
            CreateText(go.transform, "Label", label, primary ? 24 : 20, Vector2.zero, size,
                TextAnchor.MiddleCenter, textColor);

            TitleMenuMotion.WireButtonHover(button, accent);
            return button;
        }

        public struct GameplayUiBundle
        {
            public HudUI hud;
            public DialogueUI dialogue;
            public ForgeShopUI forgeShop;
            public ClearBannerUI clear;
            public PauseMenuUI pause;
            public LevelUpUI levelUp;
            public GameOverUI gameOver;
            public MinimapUI minimap;
            public WeaponHotbarUI weaponHotbar;
            public BagUI bag;
        }

        public static GameplayUiBundle BuildGameplayHud(bool includeClearBanner, bool includeMinimap = true)
        {
            ArtCatalog.EnsureLoaded();
            var canvas = CreateCanvas("GameplayCanvas");
            var root = canvas.transform;

            // —— 슬림 HUD: 상단 골드 · 생존(HP) 만 ——
            const float modX = 16f;
            const float modW = 272f;
            var y = -14f;

            // 상단 골드 — 천 단위 · 초과 시 K/M
            var goldBar = CreateHudModule(root, "HudGold", new Vector2(modX, y), new Vector2(128f, 32f));
            var gold = CreateText(goldBar.transform, "GoldText", "0G", 14, new Vector2(10f, -5f),
                new Vector2(108f, 22f), TextAnchor.MiddleLeft, Currency);
            AnchorTopLeft(gold.rectTransform, new Vector2(10f, -5f));
            StripTextFx(gold);
            gold.fontStyle = FontStyle.Bold;
            gold.horizontalOverflow = HorizontalWrapMode.Overflow;
            gold.verticalOverflow = VerticalWrapMode.Truncate;
            gold.resizeTextForBestFit = true;
            gold.resizeTextMinSize = 11;
            gold.resizeTextMaxSize = 14;

            Text potion = null;

            y -= 32f + 8f;

            // 생존 — HP · Lv · EXP
            var vital = CreateHudModule(root, "HudVital", new Vector2(modX, y), new Vector2(modW, 54f));
            var hpTag = CreateText(vital.transform, "HpTag", "HP", 12, new Vector2(12f, -8f),
                new Vector2(28f, 18f), TextAnchor.MiddleLeft, new Color(1f, 0.55f, 0.55f, 1f));
            AnchorTopLeft(hpTag.rectTransform, new Vector2(12f, -8f));
            StripTextFx(hpTag);
            hpTag.fontStyle = FontStyle.Bold;

            var hp = CreateHpSlider(vital.transform, new Vector2(42f, -8f), new Vector2(218f, 18f));
            var hpFillImg = hp.fillRect != null ? hp.fillRect.GetComponent<Image>() : null;
            var hpLabel = CreateText(vital.transform, "HpText", "100/100", 13, new Vector2(42f, -8f),
                new Vector2(218f, 18f), TextAnchor.MiddleCenter, SoftWhite);
            AnchorTopLeft(hpLabel.rectTransform, new Vector2(42f, -8f));
            hpLabel.raycastTarget = false;
            hpLabel.fontStyle = FontStyle.Bold;
            var hpOutline = hpLabel.gameObject.AddComponent<Outline>();
            hpOutline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            hpOutline.effectDistance = new Vector2(1.2f, -1.2f);

            var level = CreateText(vital.transform, "LevelText", "Lv 1", 11, new Vector2(12f, -34f),
                new Vector2(42f, 14f), TextAnchor.MiddleLeft, Gold);
            AnchorTopLeft(level.rectTransform, new Vector2(12f, -34f));
            StripTextFx(level);

            var expBar = CreateBarSlider(vital.transform, "ExpSlider", new Vector2(54f, -37f), new Vector2(206f, 5f),
                new Color(0.35f, 0.72f, 0.95f, 0.95f));
            var exp = CreateText(vital.transform, "ExpText", "", 1, Vector2.zero, Vector2.one, TextAnchor.MiddleLeft, Muted);
            exp.gameObject.SetActive(false);

            y -= 54f + 8f;
            var hudStackBottom = -y;

            // 무기/스킬/퀘스트 인라인 텍스트는 제거 (핫바·R키·상단 퀘스트 아이콘 사용)
            Text clears = null;
            Text skill = null;
            Text nextWep = null;
            Text quest = null;
            Slider skillCd = null;

            var roomTitle = CreateText(root, "RoomTitle", "", 18, new Vector2(0f, -14f),
                new Vector2(520f, 28f), TextAnchor.MiddleCenter, Gold);
            var roomTitleRt = roomTitle.rectTransform;
            roomTitleRt.anchorMin = new Vector2(0.5f, 1f);
            roomTitleRt.anchorMax = new Vector2(0.5f, 1f);
            roomTitleRt.pivot = new Vector2(0.5f, 1f);
            roomTitleRt.anchoredPosition = new Vector2(0f, -14f);

            var threat = CreateText(root, "ThreatChip", "", 12, new Vector2(0f, -42f),
                new Vector2(360f, 20f), TextAnchor.MiddleCenter, new Color(0.55f, 0.72f, 0.95f));
            var threatRt = threat.rectTransform;
            threatRt.anchorMin = new Vector2(0.5f, 1f);
            threatRt.anchorMax = new Vector2(0.5f, 1f);
            threatRt.pivot = new Vector2(0.5f, 1f);
            threatRt.anchoredPosition = new Vector2(0f, -42f);

            var style = CreateText(root, "StyleCombo", "", 22, new Vector2(0f, -78f),
                new Vector2(400f, 32f), TextAnchor.MiddleCenter, new Color(0.55f, 0.92f, 0.88f));
            var styleRt = style.rectTransform;
            styleRt.anchorMin = new Vector2(0.5f, 1f);
            styleRt.anchorMax = new Vector2(0.5f, 1f);
            styleRt.pivot = new Vector2(0.5f, 1f);
            styleRt.anchoredPosition = new Vector2(0f, -78f);
            style.fontStyle = FontStyle.Bold;

            var hintBg = CreatePanel(root, "HintBg", new Vector2(0f, 112f), new Vector2(720f, 40f),
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), true);
            hintBg.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.08f, 0.55f);
            var hint = CreateText(hintBg.transform, "HintText", "", 12, Vector2.zero, new Vector2(680f, 32f),
                TextAnchor.MiddleCenter, SoftWhite);
            hint.horizontalOverflow = HorizontalWrapMode.Wrap;
            hint.verticalOverflow = VerticalWrapMode.Truncate;
            hint.resizeTextForBestFit = true;
            hint.resizeTextMinSize = 9;
            hint.resizeTextMaxSize = 13;

            var help = CreatePanel(root, "HelpPanel", new Vector2(-16f, 16f), new Vector2(188f, 80f),
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), true);
            help.GetComponent<Image>().color = new Color(0.04f, 0.045f, 0.06f, 0.55f);
            CreateText(help.transform, "HelpBody",
                "WASD/스틱 이동 · Space/A 공격\nR/B 스킬 · LB/RB 무기\n가방·퀘스트 상단 아이콘\nQ/Y 포션 · E/X · Esc", 11,
                new Vector2(0f, 0f), new Vector2(170f, 64f), TextAnchor.MiddleCenter, Muted);
            if (GameInput.IsMobileLayout)
            {
                help.SetActive(false);
            }

            var vig = CreateFullScreen(root, "LowHpVignette", new Color(0.55f, 0.05f, 0.08f, 0f));
            vig.transform.SetAsFirstSibling();
            var vigImg = vig.GetComponent<Image>();
            vigImg.raycastTarget = false;

            var hud = canvas.gameObject.AddComponent<HudUI>();
            hud.BindWidgets(hp, hpLabel, level, exp, hint, gold, potion, clears, skill, roomTitle, expBar, quest,
                style, threat, skillCd, nextWep, hpFillImg, vigImg);

            var weaponHotbar = WeaponHotbarUI.Build(root);
            var bag = BagUI.Build(root);
            QuestLogUI.Build(root);
            TopMenuIconsUI.Build(root);

            MobileControlsUI.Build(root);

            // 대화 딤 + 중앙 모달 (하단 힌트와 겹치지 않음)
            var dialogueDim = CreateFullScreen(root, "DialogueDim", new Color(0.02f, 0.02f, 0.03f, 0.55f));
            dialogueDim.SetActive(false);

            var dialoguePanel = CreatePanel(root, "DialoguePanel", Vector2.zero, new Vector2(700f, 480f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), true);
            dialoguePanel.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.09f, 0.94f);

            var rail = new GameObject("AccentRail");
            rail.transform.SetParent(dialoguePanel.transform, false);
            var railRt = rail.AddComponent<RectTransform>();
            railRt.anchorMin = new Vector2(0f, 0f);
            railRt.anchorMax = new Vector2(0f, 1f);
            railRt.pivot = new Vector2(0f, 0.5f);
            railRt.sizeDelta = new Vector2(5f, 0f);
            railRt.anchoredPosition = new Vector2(3f, 0f);
            rail.AddComponent<Image>().color = Gold;

            var name = CreateText(dialoguePanel.transform, "Name", "NPC", 24, Vector2.zero,
                new Vector2(0f, 36f), TextAnchor.MiddleLeft, Gold);
            var nameRt = name.rectTransform;
            nameRt.anchorMin = new Vector2(0f, 1f);
            nameRt.anchorMax = new Vector2(1f, 1f);
            nameRt.pivot = new Vector2(0.5f, 1f);
            nameRt.offsetMin = new Vector2(32f, -48f);
            nameRt.offsetMax = new Vector2(-28f, -12f);
            name.fontStyle = FontStyle.Bold;
            name.horizontalOverflow = HorizontalWrapMode.Overflow;
            name.verticalOverflow = VerticalWrapMode.Truncate;

            var divider = new GameObject("NameDivider");
            divider.transform.SetParent(dialoguePanel.transform, false);
            var divRt = divider.AddComponent<RectTransform>();
            divRt.anchorMin = new Vector2(0f, 1f);
            divRt.anchorMax = new Vector2(1f, 1f);
            divRt.pivot = new Vector2(0.5f, 1f);
            divRt.sizeDelta = new Vector2(-60f, 2f);
            divRt.anchoredPosition = new Vector2(2f, -52f);
            divider.AddComponent<Image>().color = new Color(Gold.r, Gold.g, Gold.b, 0.5f);

            // 본문 스크롤 영역 — 패널 밖으로 글자가 새지 않음
            var viewportGo = new GameObject("BodyViewport");
            viewportGo.transform.SetParent(dialoguePanel.transform, false);
            var viewportRt = viewportGo.AddComponent<RectTransform>();
            viewportRt.anchorMin = new Vector2(0f, 0f);
            viewportRt.anchorMax = new Vector2(1f, 1f);
            viewportRt.offsetMin = new Vector2(28f, 48f);
            viewportRt.offsetMax = new Vector2(-28f, -60f);
            var viewportImg = viewportGo.AddComponent<Image>();
            viewportImg.color = new Color(0f, 0f, 0f, 0.01f);
            viewportGo.AddComponent<Mask>().showMaskGraphic = false;

            var contentGo = new GameObject("BodyContent");
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRt = contentGo.AddComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0f, 1f);
            contentRt.anchorMax = new Vector2(1f, 1f);
            contentRt.pivot = new Vector2(0.5f, 1f);
            contentRt.anchoredPosition = Vector2.zero;
            contentRt.sizeDelta = new Vector2(0f, 0f);

            var body = CreateText(contentGo.transform, "Body", "...", 16, Vector2.zero,
                new Vector2(0f, 40f), TextAnchor.UpperLeft, SoftWhite);
            var bodyRt = body.rectTransform;
            bodyRt.anchorMin = new Vector2(0f, 1f);
            bodyRt.anchorMax = new Vector2(1f, 1f);
            bodyRt.pivot = new Vector2(0.5f, 1f);
            bodyRt.anchoredPosition = Vector2.zero;
            bodyRt.sizeDelta = new Vector2(0f, 40f);
            bodyRt.offsetMin = new Vector2(4f, bodyRt.offsetMin.y);
            bodyRt.offsetMax = new Vector2(-4f, 0f);
            body.lineSpacing = 1.18f;
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.verticalOverflow = VerticalWrapMode.Overflow;
            body.color = new Color(0.95f, 0.93f, 0.89f, 1f);
            var fitter = body.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = viewportGo.AddComponent<ScrollRect>();
            scroll.content = contentRt;
            scroll.viewport = viewportRt;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 28f;

            var prompt = CreateText(dialoguePanel.transform, "Prompt", "Space  닫기", 14, Vector2.zero,
                new Vector2(0f, 28f), TextAnchor.MiddleCenter, new Color(0.78f, 0.72f, 0.58f, 1f));
            var promptRt = prompt.rectTransform;
            promptRt.anchorMin = new Vector2(0f, 0f);
            promptRt.anchorMax = new Vector2(1f, 0f);
            promptRt.pivot = new Vector2(0.5f, 0f);
            promptRt.offsetMin = new Vector2(28f, 10f);
            promptRt.offsetMax = new Vector2(-28f, 40f);

            var choiceGo = new GameObject("ChoiceRoot");
            choiceGo.transform.SetParent(dialoguePanel.transform, false);
            var choiceRt = choiceGo.AddComponent<RectTransform>();
            choiceRt.anchorMin = new Vector2(0.5f, 0f);
            choiceRt.anchorMax = new Vector2(0.5f, 0f);
            choiceRt.pivot = new Vector2(0.5f, 0f);
            choiceRt.anchoredPosition = new Vector2(0f, 52f);
            choiceRt.sizeDelta = new Vector2(660f, 200f);
            choiceGo.SetActive(false);

            var dialogue = canvas.gameObject.AddComponent<DialogueUI>();
            dialogue.Bind(dialoguePanel, name, body, prompt, dialogueDim, scroll, choiceRt);

            // 대장간 패널 — 대장간 느낌(숯·구리·불꽃), 스크롤 목록
            var forgeDim = CreateFullScreen(root, "ForgeDim", new Color(0.02f, 0.03f, 0.05f, 0.7f));
            forgeDim.SetActive(false);

            var forgePanel = CreatePanel(root, "ForgePanel", Vector2.zero, new Vector2(720f, 620f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), true);
            forgePanel.GetComponent<Image>().color = new Color(0.06f, 0.07f, 0.09f, 0.95f);
            var forgeOutline = forgePanel.GetComponent<Outline>();
            if (forgeOutline != null)
            {
                forgeOutline.effectColor = new Color(0.4f, 0.78f, 0.75f, 0.4f);
                forgeOutline.effectDistance = new Vector2(1f, -1f);
            }

            // 상단 악센트 라인
            var forgeTopLine = new GameObject("ForgeEmberLine");
            forgeTopLine.transform.SetParent(forgePanel.transform, false);
            var emberRt = forgeTopLine.AddComponent<RectTransform>();
            emberRt.anchorMin = new Vector2(0f, 1f);
            emberRt.anchorMax = new Vector2(1f, 1f);
            emberRt.pivot = new Vector2(0.5f, 1f);
            emberRt.anchoredPosition = Vector2.zero;
            emberRt.sizeDelta = new Vector2(0f, 2f);
            forgeTopLine.AddComponent<Image>().color = new Color(0.45f, 0.9f, 0.86f, 0.85f);

            var forgeRail = new GameObject("ForgeRail");
            forgeRail.transform.SetParent(forgePanel.transform, false);
            var forgeRailRt = forgeRail.AddComponent<RectTransform>();
            forgeRailRt.anchorMin = new Vector2(0f, 0f);
            forgeRailRt.anchorMax = new Vector2(0f, 1f);
            forgeRailRt.pivot = new Vector2(0f, 0.5f);
            forgeRailRt.sizeDelta = new Vector2(3f, 0f);
            forgeRailRt.anchoredPosition = Vector2.zero;
            forgeRail.AddComponent<Image>().color = new Color(0.45f, 0.88f, 0.84f, 0.9f);

            var forgeTitle = CreateText(forgePanel.transform, "ForgeTitle", "대장간 · Borin", 26, Vector2.zero,
                new Vector2(0f, 36f), TextAnchor.MiddleLeft, SoftWhite);
            var forgeTitleRt = forgeTitle.rectTransform;
            forgeTitleRt.anchorMin = new Vector2(0f, 1f);
            forgeTitleRt.anchorMax = new Vector2(1f, 1f);
            forgeTitleRt.pivot = new Vector2(0.5f, 1f);
            forgeTitleRt.offsetMin = new Vector2(28f, -52f);
            forgeTitleRt.offsetMax = new Vector2(-120f, -14f);
            forgeTitle.fontStyle = FontStyle.Bold;

            var forgeSub = CreateText(forgePanel.transform, "ForgeSub", "", 12, Vector2.zero,
                new Vector2(0f, 20f), TextAnchor.MiddleRight, Muted);
            var forgeSubRt = forgeSub.rectTransform;
            forgeSubRt.anchorMin = new Vector2(1f, 1f);
            forgeSubRt.anchorMax = new Vector2(1f, 1f);
            forgeSubRt.pivot = new Vector2(1f, 1f);
            forgeSubRt.anchoredPosition = new Vector2(-24f, -22f);
            forgeSubRt.sizeDelta = new Vector2(140f, 22f);

            var forgeDiv = new GameObject("ForgeDivider");
            forgeDiv.transform.SetParent(forgePanel.transform, false);
            var forgeDivRt = forgeDiv.AddComponent<RectTransform>();
            forgeDivRt.anchorMin = new Vector2(0f, 1f);
            forgeDivRt.anchorMax = new Vector2(1f, 1f);
            forgeDivRt.pivot = new Vector2(0.5f, 1f);
            forgeDivRt.sizeDelta = new Vector2(-48f, 1f);
            forgeDivRt.anchoredPosition = new Vector2(0f, -56f);
            forgeDiv.AddComponent<Image>().color = new Color(0.85f, 0.5f, 0.25f, 0.35f);

            // 상태 — 콤팩트 스트립 (카드 박스 최소화)
            var statusCard = new GameObject("StatusStrip");
            statusCard.transform.SetParent(forgePanel.transform, false);
            var statusCardRt = statusCard.AddComponent<RectTransform>();
            statusCardRt.anchorMin = new Vector2(0f, 1f);
            statusCardRt.anchorMax = new Vector2(1f, 1f);
            statusCardRt.pivot = new Vector2(0.5f, 1f);
            statusCardRt.offsetMin = new Vector2(24f, -176f);
            statusCardRt.offsetMax = new Vector2(-24f, -64f);
            var statusBg = statusCard.AddComponent<Image>();
            statusBg.color = new Color(0.12f, 0.09f, 0.07f, 0.92f);
            var statusAccent = new GameObject("StatusAccent");
            statusAccent.transform.SetParent(statusCard.transform, false);
            var saRt = statusAccent.AddComponent<RectTransform>();
            saRt.anchorMin = new Vector2(0f, 0f);
            saRt.anchorMax = new Vector2(0f, 1f);
            saRt.pivot = new Vector2(0f, 0.5f);
            saRt.sizeDelta = new Vector2(3f, 0f);
            statusAccent.AddComponent<Image>().color = new Color(1f, 0.55f, 0.25f, 0.8f);

            var forgeStatus = CreateText(statusCard.transform, "ForgeStatus", "", 13, Vector2.zero,
                new Vector2(0f, 0f), TextAnchor.UpperLeft, SoftWhite);
            var forgeStatusRt = forgeStatus.rectTransform;
            forgeStatusRt.anchorMin = Vector2.zero;
            forgeStatusRt.anchorMax = Vector2.one;
            forgeStatusRt.offsetMin = new Vector2(16f, 8f);
            forgeStatusRt.offsetMax = new Vector2(-14f, -8f);
            forgeStatus.lineSpacing = 1.15f;
            forgeStatus.horizontalOverflow = HorizontalWrapMode.Wrap;
            forgeStatus.verticalOverflow = VerticalWrapMode.Truncate;
            forgeStatus.color = new Color(0.9f, 0.84f, 0.74f, 1f);
            forgeStatus.fontSize = 12;

            // 스크롤 목록
            var forgeViewport = new GameObject("ForgeViewport");
            forgeViewport.transform.SetParent(forgePanel.transform, false);
            var forgeViewportRt = forgeViewport.AddComponent<RectTransform>();
            forgeViewportRt.anchorMin = new Vector2(0f, 0f);
            forgeViewportRt.anchorMax = new Vector2(1f, 1f);
            forgeViewportRt.offsetMin = new Vector2(22f, 48f);
            forgeViewportRt.offsetMax = new Vector2(-22f, -188f);
            forgeViewport.AddComponent<Image>().color = new Color(0.05f, 0.045f, 0.04f, 0.55f);
            var forgeMask = forgeViewport.AddComponent<Mask>();
            forgeMask.showMaskGraphic = false;

            var forgeListGo = new GameObject("ForgeList");
            forgeListGo.transform.SetParent(forgeViewport.transform, false);
            var forgeListRt = forgeListGo.AddComponent<RectTransform>();
            forgeListRt.anchorMin = new Vector2(0f, 1f);
            forgeListRt.anchorMax = new Vector2(1f, 1f);
            forgeListRt.pivot = new Vector2(0.5f, 1f);
            forgeListRt.anchoredPosition = Vector2.zero;
            forgeListRt.sizeDelta = new Vector2(0f, 0f);
            var forgeLayout = forgeListGo.AddComponent<VerticalLayoutGroup>();
            forgeLayout.spacing = 6f;
            forgeLayout.childAlignment = TextAnchor.UpperCenter;
            forgeLayout.childControlWidth = true;
            forgeLayout.childControlHeight = true;
            forgeLayout.childForceExpandWidth = true;
            forgeLayout.childForceExpandHeight = false;
            forgeLayout.padding = new RectOffset(8, 8, 8, 8);
            var forgeFitter = forgeListGo.AddComponent<ContentSizeFitter>();
            forgeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var forgeScroll = forgeViewport.AddComponent<ScrollRect>();
            forgeScroll.content = forgeListRt;
            forgeScroll.viewport = forgeViewportRt;
            forgeScroll.horizontal = false;
            forgeScroll.vertical = true;
            forgeScroll.movementType = ScrollRect.MovementType.Clamped;
            forgeScroll.scrollSensitivity = 28f;

            var forgePrompt = CreateText(forgePanel.transform, "ForgePrompt",
                "아이콘에 올리면 상세 설명  ·  클릭으로 구매  ·  Space/Esc 닫기", 12,
                Vector2.zero, new Vector2(0f, 28f), TextAnchor.MiddleCenter,
                new Color(0.65f, 0.52f, 0.4f, 1f));
            var forgePromptRt = forgePrompt.rectTransform;
            forgePromptRt.anchorMin = new Vector2(0f, 0f);
            forgePromptRt.anchorMax = new Vector2(1f, 0f);
            forgePromptRt.pivot = new Vector2(0.5f, 0f);
            forgePromptRt.offsetMin = new Vector2(24f, 12f);
            forgePromptRt.offsetMax = new Vector2(-24f, 40f);

            var forgeShop = canvas.gameObject.AddComponent<ForgeShopUI>();
            forgeShop.Bind(forgePanel, forgeDim, forgeTitle, forgeStatus, forgeListRt, forgePrompt);

            // Pause — 스테이터스 + 세션 + 설정
            var pausePanel = CreatePanel(root, "PausePanel", Vector2.zero, new Vector2(520f, 560f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            CreateText(pausePanel.transform, "PauseTitle", "스테이터스 · 도감", 28, new Vector2(0f, 248f),
                new Vector2(480f, 36f), TextAnchor.MiddleCenter, Gold);
            var pauseMeta = CreateText(pausePanel.transform, "PauseMeta", "", 12, new Vector2(0f, 150f),
                new Vector2(470f, 150f), TextAnchor.MiddleCenter, SoftWhite);
            CreateText(pausePanel.transform, "VolLabel", "마스터 볼륨", 13, new Vector2(0f, 58f),
                new Vector2(220f, 20f), TextAnchor.MiddleCenter, Muted);
            var vol = CreateSimpleSlider(pausePanel.transform, new Vector2(0f, 34f), new Vector2(300f, 18f));
            CreateText(pausePanel.transform, "SfxLabel", "효과음", 13, new Vector2(0f, 8f),
                new Vector2(220f, 20f), TextAnchor.MiddleCenter, Muted);
            var sfx = CreateSimpleSlider(pausePanel.transform, new Vector2(0f, -16f), new Vector2(300f, 18f));
            CreateText(pausePanel.transform, "ShakeLabel", "화면 흔들림", 13, new Vector2(0f, -42f),
                new Vector2(220f, 20f), TextAnchor.MiddleCenter, Muted);
            var shake = CreateSimpleSlider(pausePanel.transform, new Vector2(0f, -66f), new Vector2(300f, 18f));
            var resume = CreateButton(pausePanel.transform, "Resume", "계속하기", new Vector2(0f, -120f),
                new Vector2(300f, 44f));
            var toTitle = CreateButton(pausePanel.transform, "Title", "타이틀로", new Vector2(0f, -176f),
                new Vector2(300f, 44f));
            var quit = CreateButton(pausePanel.transform, "Quit", "종료", new Vector2(0f, -232f),
                new Vector2(300f, 44f));
            var pause = canvas.gameObject.AddComponent<PauseMenuUI>();
            pause.Bind(pausePanel, vol, resume, toTitle, quit, pauseMeta, sfx, shake);

            // Level up — 카드형 (무기 연마 포함)
            var levelPanel = CreatePanel(root, "LevelUpPanel", Vector2.zero, new Vector2(620f, 440f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            var luTitle = CreateText(levelPanel.transform, "LuTitle", "성장 보너스 선택", 22,
                new Vector2(0f, 178f), new Vector2(580f, 36f), TextAnchor.MiddleCenter, Gold);
            luTitle.horizontalOverflow = HorizontalWrapMode.Overflow;
            CreateText(levelPanel.transform, "LuSub", "기본 성장은 이미 적용됨 · 추가 방향을 고르세요", 13,
                new Vector2(0f, 146f), new Vector2(560f, 24f), TextAnchor.MiddleCenter, Muted);
            var bHp = CreateButton(levelPanel.transform, "BonusHp", "♥ 체력\n+36 HP · +2 ATK", new Vector2(-190f, 55f),
                new Vector2(180f, 88f), 14);
            var bAtk = CreateButton(levelPanel.transform, "BonusAtk", "⚔ 공격\n+7 ATK · +12 HP", new Vector2(0f, 55f),
                new Vector2(180f, 88f), 14);
            var bWep = CreateButton(levelPanel.transform, "BonusWep", "◆ 무기 연마\n장착 무기 +1", new Vector2(190f, 55f),
                new Vector2(180f, 88f), 14);
            var bSpd = CreateButton(levelPanel.transform, "BonusSpd", "➤ 신속\n+0.9 이속 · +2 ATK", new Vector2(-100f, -55f),
                new Vector2(200f, 88f), 14);
            var bCrit = CreateButton(levelPanel.transform, "BonusCrit", "✧ 치명\n+12% 치명 · +3 ATK", new Vector2(100f, -55f),
                new Vector2(200f, 88f), 14);
            var levelUp = canvas.gameObject.AddComponent<LevelUpUI>();
            levelUp.Bind(levelPanel, bHp, bAtk, bSpd, bCrit, luTitle, bWep);

            // Game over
            var goPanel = CreatePanel(root, "GameOverPanel", Vector2.zero, new Vector2(560f, 160f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            var goMsg = CreateText(goPanel.transform, "GoMsg", "쓰러졌다...", 24, Vector2.zero,
                new Vector2(520f, 120f), TextAnchor.MiddleCenter, SoftWhite);
            var gameOver = canvas.gameObject.AddComponent<GameOverUI>();
            gameOver.Bind(goPanel, goMsg);

            MinimapUI minimap = null;
            if (includeMinimap)
            {
                // 던전 지도 — 방 배치가 잘 보이도록 패널·영역 확대
                var mapPanel = CreatePanel(root, "MinimapPanel", new Vector2(14f, -(hudStackBottom + 12f)), new Vector2(288f, 208f),
                    new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), true);
                var mapPanelImg = mapPanel.GetComponent<Image>();
                if (mapPanelImg != null)
                {
                    mapPanelImg.color = new Color(0.05f, 0.07f, 0.1f, 0.92f);
                }

                var mapAccent = new GameObject("MapAccent");
                mapAccent.transform.SetParent(mapPanel.transform, false);
                var accentRt = mapAccent.AddComponent<RectTransform>();
                accentRt.anchorMin = new Vector2(0f, 1f);
                accentRt.anchorMax = new Vector2(1f, 1f);
                accentRt.pivot = new Vector2(0.5f, 1f);
                accentRt.anchoredPosition = Vector2.zero;
                accentRt.sizeDelta = new Vector2(0f, 2.5f);
                var accentImg = mapAccent.AddComponent<Image>();
                accentImg.color = new Color(0.4f, 0.9f, 1f, 0.85f);
                accentImg.raycastTarget = false;

                var mapTitle = CreateText(mapPanel.transform, "MapTitle", "던전 지도", 13, new Vector2(0f, 82f),
                    new Vector2(260f, 20f), TextAnchor.MiddleCenter,
                    new Color(0.85f, 0.92f, 0.98f, 1f));
                mapTitle.fontStyle = FontStyle.Bold;
                var mapLegend = CreateText(mapPanel.transform, "MapLegend",
                    "●나  ◆입구  ★보물  ✦성소  ☠보스  ▲출구  ·회색=미탐험", 9, new Vector2(0f, -82f),
                    new Vector2(270f, 18f), TextAnchor.MiddleCenter,
                    new Color(0.7f, 0.76f, 0.82f, 1f));
                mapLegend.horizontalOverflow = HorizontalWrapMode.Overflow;
                mapLegend.verticalOverflow = VerticalWrapMode.Truncate;
                var mapRootGo = new GameObject("MapRoot");
                mapRootGo.transform.SetParent(mapPanel.transform, false);
                var mapRoot = mapRootGo.AddComponent<RectTransform>();
                mapRoot.anchoredPosition = new Vector2(0f, 2f);
                mapRoot.sizeDelta = new Vector2(252f, 140f);
                minimap = canvas.gameObject.AddComponent<MinimapUI>();
                minimap.Bind(mapRoot, mapTitle, mapLegend);
            }

            // 토스트는 다른 오버레이보다 항상 위에 (가방 SetAsLastSibling 이후에도 sorting으로 보장)
            ClearBannerUI clear = null;
            if (includeClearBanner)
            {
                var clearPanel = CreatePanel(root, "ClearPanel", new Vector2(0f, 48f), new Vector2(720f, 52f),
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), true);
                var clearBg = clearPanel.GetComponent<Image>();
                clearBg.color = new Color(0.05f, 0.06f, 0.08f, 0.9f);
                clearBg.raycastTarget = false;
                var accent = new GameObject("ToastAccent");
                accent.transform.SetParent(clearPanel.transform, false);
                var accRt = accent.AddComponent<RectTransform>();
                accRt.anchorMin = new Vector2(0f, 0.12f);
                accRt.anchorMax = new Vector2(0f, 0.88f);
                accRt.pivot = new Vector2(0f, 0.5f);
                accRt.sizeDelta = new Vector2(4f, 0f);
                accRt.anchoredPosition = new Vector2(8f, 0f);
                var accImg = accent.AddComponent<Image>();
                accImg.color = Gold;
                accImg.raycastTarget = false;
                var msg = CreateText(clearPanel.transform, "Message", "클리어!", 16, Vector2.zero, new Vector2(680f, 36f),
                    TextAnchor.MiddleCenter, SoftWhite);
                msg.horizontalOverflow = HorizontalWrapMode.Wrap;
                msg.verticalOverflow = VerticalWrapMode.Truncate;
                msg.resizeTextForBestFit = true;
                msg.resizeTextMinSize = 11;
                msg.resizeTextMaxSize = 16;
                msg.raycastTarget = false;
                clear = canvas.gameObject.AddComponent<ClearBannerUI>();
                clear.Bind(clearPanel, msg, accImg);
            }

            return new GameplayUiBundle
            {
                hud = hud,
                dialogue = dialogue,
                forgeShop = forgeShop,
                clear = clear,
                pause = pause,
                levelUp = levelUp,
                gameOver = gameOver,
                minimap = minimap,
                weaponHotbar = weaponHotbar,
                bag = bag
            };
        }

        private static Slider CreateSimpleSlider(Transform parent, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject("VolumeSlider");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPos;
            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.12f, 0.16f, 1f);

            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            var far = fillArea.AddComponent<RectTransform>();
            far.anchorMin = Vector2.zero;
            far.anchorMax = Vector2.one;
            far.offsetMin = new Vector2(2, 2);
            far.offsetMax = new Vector2(-2, -2);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fr = fill.AddComponent<RectTransform>();
            fr.anchorMin = Vector2.zero;
            fr.anchorMax = Vector2.one;
            fr.offsetMin = Vector2.zero;
            fr.offsetMax = Vector2.zero;
            var fi = fill.AddComponent<Image>();
            fi.color = Accent;

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fr;
            slider.targetGraphic = fi;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.8f;
            return slider;
        }

        private static GameObject CreateFullScreen(Transform parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = go.AddComponent<Image>();
            image.color = color;
            return go;
        }

        private static GameObject CreateHudModule(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            var go = CreatePanel(parent, name, pos, size,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), true);
            go.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.08f, 0.78f);
            return go;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 pos, Vector2 size,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, bool styled = false)
        {
            ArtCatalog.EnsureLoaded();
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = pos;
            var image = go.AddComponent<Image>();
            image.sprite = null;
            image.color = styled ? PanelDark : PanelDeep;

            // 얇은 쿨 엣지 — 장식 림 대신 소프트 아웃라인
            var edge = go.AddComponent<Outline>();
            edge.effectColor = styled
                ? new Color(0.4f, 0.75f, 0.72f, 0.28f)
                : new Color(1f, 1f, 1f, 0.1f);
            edge.effectDistance = new Vector2(1f, -1f);

            if (styled)
            {
                // 좌측 아이스 악센트 바
                var rail = new GameObject("AccentRail");
                rail.transform.SetParent(go.transform, false);
                var rr = rail.AddComponent<RectTransform>();
                rr.anchorMin = new Vector2(0f, 0.08f);
                rr.anchorMax = new Vector2(0f, 0.92f);
                rr.pivot = new Vector2(0f, 0.5f);
                rr.sizeDelta = new Vector2(3f, 0f);
                rr.anchoredPosition = Vector2.zero;
                rail.AddComponent<Image>().color = Line;
            }

            return go;
        }

        private static void AnchorTopLeft(RectTransform rect, Vector2 pos)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = pos;
        }

        private static void StripTextFx(Text text)
        {
            if (text == null)
            {
                return;
            }

            var shadow = text.GetComponent<Shadow>();
            if (shadow != null)
            {
                Object.Destroy(shadow);
            }

            var outline = text.GetComponent<Outline>();
            if (outline != null)
            {
                Object.Destroy(outline);
            }
        }
    }
}
