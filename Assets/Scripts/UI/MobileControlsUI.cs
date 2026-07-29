using DungeonOdyssey.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DungeonOdyssey.UI
{
    /// <summary>가상 조이스틱 + 액션 버튼. 모바일/터치 환경에서 표시.</summary>
    public class MobileControlsUI : MonoBehaviour
    {
        private RectTransform _stickArea;
        private RectTransform _knob;
        private GameObject _root;
        private GameObject _menuRow;
        private bool _dragging;
        private Vector2 _origin;
        private const float StickRadius = 64f;

        public static MobileControlsUI Build(Transform canvasRoot)
        {
            var host = new GameObject("MobileControls");
            host.transform.SetParent(canvasRoot, false);
            var ui = host.AddComponent<MobileControlsUI>();
            ui.Construct();
            return ui;
        }

        private void Construct()
        {
            _root = gameObject;
            var stretch = gameObject.AddComponent<RectTransform>();
            stretch.anchorMin = Vector2.zero;
            stretch.anchorMax = Vector2.one;
            stretch.offsetMin = Vector2.zero;
            stretch.offsetMax = Vector2.zero;

            BuildJoystick();
            BuildActionButtons();
            BuildMenuRow();

            _root.SetActive(GameInput.IsMobileLayout);
            if (_menuRow != null)
            {
                _menuRow.SetActive(false);
            }
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.F10))
            {
                _root.SetActive(!_root.activeSelf);
            }
#endif
            if (!_root.activeSelf)
            {
                return;
            }

            var blocking = GameUi.IsBlocking && !GameUi.IsPaused;
            if (_menuRow != null && _menuRow.activeSelf != blocking)
            {
                _menuRow.SetActive(blocking);
            }

            if (!_dragging && _knob != null)
            {
                _knob.anchoredPosition = Vector2.Lerp(_knob.anchoredPosition, Vector2.zero,
                    14f * Time.unscaledDeltaTime);
            }
        }

        public void OnStickDown(Vector2 screenPos)
        {
            _dragging = true;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_stickArea, screenPos, null, out _origin);
        }

        public void OnStickDrag(Vector2 screenPos)
        {
            if (!_dragging || _stickArea == null)
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_stickArea, screenPos, null, out var local);
            var delta = local - (_origin == Vector2.zero ? Vector2.zero : Vector2.zero);
            // 영역 중심 기준
            delta = local;
            var clamped = Vector2.ClampMagnitude(delta, StickRadius);
            if (_knob != null)
            {
                _knob.anchoredPosition = clamped;
            }

            GameInput.SetMobileMove(clamped / StickRadius);
        }

        public void OnStickUp()
        {
            _dragging = false;
            _origin = Vector2.zero;
            GameInput.ClearMobileMove();
            if (_knob != null)
            {
                _knob.anchoredPosition = Vector2.zero;
            }
        }

        private void BuildJoystick()
        {
            var area = CreateCircle(transform, "StickArea", new Vector2(110f, 110f), new Vector2(0f, 0f),
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(128f, 128f),
                new Color(1f, 1f, 1f, 0.12f));
            area.anchoredPosition = new Vector2(110f, 110f);
            _stickArea = area;

            var knob = CreateCircle(area, "Knob", Vector2.zero, new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(56f, 56f),
                new Color(0.92f, 0.86f, 0.72f, 0.55f));
            _knob = knob;

            var trigger = area.gameObject.AddComponent<MobileStickZone>();
            trigger.Bind(this);
        }

        private void BuildActionButtons()
        {
            var cluster = new GameObject("Actions");
            cluster.transform.SetParent(transform, false);
            var crt = cluster.AddComponent<RectTransform>();
            crt.anchorMin = new Vector2(1f, 0f);
            crt.anchorMax = new Vector2(1f, 0f);
            crt.pivot = new Vector2(1f, 0f);
            crt.anchoredPosition = new Vector2(-24f, 24f);
            crt.sizeDelta = new Vector2(220f, 220f);

            MakeAction(cluster.transform, "Attack", "공격", new Vector2(-50f, 70f), 72f,
                new Color(0.75f, 0.32f, 0.28f, 0.72f), GameInput.PressAttack);
            MakeAction(cluster.transform, "Skill", "스킬", new Vector2(-130f, 40f), 58f,
                new Color(0.35f, 0.5f, 0.7f, 0.72f), GameInput.PressSkill);
            MakeAction(cluster.transform, "Interact", "행동", new Vector2(-50f, 150f), 58f,
                new Color(0.4f, 0.7f, 0.55f, 0.72f), GameInput.PressInteract);
            MakeAction(cluster.transform, "Potion", "포션", new Vector2(-130f, 110f), 50f,
                new Color(0.45f, 0.7f, 0.45f, 0.7f), GameInput.PressPotion);
            MakeAction(cluster.transform, "Cycle", "스킬⇄", new Vector2(-180f, 170f), 44f,
                new Color(0.55f, 0.5f, 0.4f, 0.7f), GameInput.PressSkillCycle);
            MakeAction(cluster.transform, "Bag", "가방", new Vector2(-130f, 170f), 44f,
                new Color(0.4f, 0.55f, 0.7f, 0.72f), GameInput.PressBag);
            MakeAction(cluster.transform, "Weapon", "무기⇄", new Vector2(-180f, 118f), 44f,
                new Color(0.65f, 0.48f, 0.35f, 0.72f), GameInput.PressWeaponCycle);
            MakeAction(cluster.transform, "WepPrev", "무기←", new Vector2(-180f, 66f), 40f,
                new Color(0.5f, 0.42f, 0.35f, 0.7f), GameInput.PressWeaponCyclePrev);
            MakeAction(cluster.transform, "Pause", "메뉴", new Vector2(-30f, 210f), 40f,
                new Color(0.35f, 0.35f, 0.4f, 0.7f), GameInput.PressPause);
        }

        private void BuildMenuRow()
        {
            _menuRow = new GameObject("MenuKeys");
            _menuRow.transform.SetParent(transform, false);
            var rt = _menuRow.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 56f);
            rt.sizeDelta = new Vector2(480f, 56f);

            for (var i = 1; i <= 6; i++)
            {
                var idx = i;
                MakeAction(_menuRow.transform, $"M{i}", i.ToString(),
                    new Vector2(-190f + (i - 1) * 76f, 0f), 46f,
                    new Color(0.78f, 0.68f, 0.45f, 0.8f), () => GameInput.PressMenu(idx));
            }
        }

        private static void MakeAction(Transform parent, string name, string label, Vector2 pos, float size,
            Color color, System.Action onClick)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(size, size);

            var img = go.AddComponent<Image>();
            img.color = color;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.ColorTint;
            var colors = btn.colors;
            colors.pressedColor = new Color(1f, 1f, 1f, 0.9f);
            colors.highlightedColor = new Color(1.05f, 1.05f, 1f, 1f);
            btn.colors = colors;
            btn.onClick.AddListener(() => onClick?.Invoke());

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform, false);
            var tr = textGo.AddComponent<RectTransform>();
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;
            var text = textGo.AddComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.fontSize = Mathf.RoundToInt(size * 0.28f);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                        ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.raycastTarget = false;
        }

        private static RectTransform CreateCircle(Transform parent, string name, Vector2 anchored,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchored;
            rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.color = color;
            return rt;
        }
    }

    /// <summary>조이스틱 드래그 수신.</summary>
    public class MobileStickZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private MobileControlsUI _owner;

        public void Bind(MobileControlsUI owner) => _owner = owner;

        public void OnPointerDown(PointerEventData eventData) => _owner?.OnStickDown(eventData.position);

        public void OnDrag(PointerEventData eventData) => _owner?.OnStickDrag(eventData.position);

        public void OnPointerUp(PointerEventData eventData) => _owner?.OnStickUp();
    }
}
