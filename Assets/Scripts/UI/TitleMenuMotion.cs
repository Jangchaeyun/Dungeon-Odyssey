using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DungeonOdyssey.UI
{
    /// <summary>타이틀 인트로 페이드 + 버튼 호버 악센트.</summary>
    public class TitleMenuMotion : MonoBehaviour
    {
        private CanvasGroup _group;
        private RectTransform _brand;
        private RectTransform _cta;
        private float _brandBaseY;
        private float _ctaBaseY;
        private float _t;
        private bool _ready;

        public void Bind(CanvasGroup group, RectTransform brand, RectTransform cta)
        {
            _group = group;
            _brand = brand;
            _cta = cta;
            _brandBaseY = brand != null ? brand.anchoredPosition.y : 0f;
            _ctaBaseY = cta != null ? cta.anchoredPosition.y : 0f;
            if (_group != null)
            {
                _group.alpha = 0f;
            }

            _ready = true;
        }

        private void Update()
        {
            if (!_ready)
            {
                return;
            }

            _t += Time.unscaledDeltaTime;
            if (_group != null)
            {
                _group.alpha = Mathf.Clamp01((_t - 0.15f) / 0.85f);
            }

            if (_brand != null)
            {
                // 위쪽 여백만 사용 — 아래로 내려가 메뉴와 겹치지 않게
                var rise = Mathf.Lerp(12f, 0f, EaseOut(Mathf.Clamp01((_t - 0.1f) / 0.9f)));
                var breath = Mathf.Sin(Time.unscaledTime * 0.9f) * 2f;
                _brand.anchoredPosition = new Vector2(_brand.anchoredPosition.x, _brandBaseY + rise + breath);
            }

            if (_cta != null)
            {
                var rise = Mathf.Lerp(10f, 0f, EaseOut(Mathf.Clamp01((_t - 0.45f) / 0.75f)));
                _cta.anchoredPosition = new Vector2(_cta.anchoredPosition.x, _ctaBaseY + rise);
            }
        }

        private static float EaseOut(float x) => 1f - (1f - x) * (1f - x);

        public static void WireButtonHover(Button button, Image accent)
        {
            if (button == null || accent == null)
            {
                return;
            }

            var trigger = button.gameObject.GetComponent<EventTrigger>() ??
                          button.gameObject.AddComponent<EventTrigger>();

            void Add(EventTriggerType type, System.Action<BaseEventData> cb)
            {
                var entry = new EventTrigger.Entry { eventID = type };
                entry.callback.AddListener(d => cb(d));
                trigger.triggers.Add(entry);
            }

            var c = accent.color;
            accent.color = new Color(c.r, c.g, c.b, 0.15f);

            Add(EventTriggerType.PointerEnter, _ =>
            {
                accent.color = new Color(c.r, c.g, c.b, 0.85f);
            });
            Add(EventTriggerType.PointerExit, _ =>
            {
                accent.color = new Color(c.r, c.g, c.b, 0.15f);
            });
        }
    }
}
