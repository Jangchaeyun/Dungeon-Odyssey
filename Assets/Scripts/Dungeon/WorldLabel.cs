using System.Collections.Generic;
using DungeonOdyssey.View3D;
using UnityEngine;

namespace DungeonOdyssey.Dungeon
{
    public enum LabelShowMode
    {
        Always,
        /// <summary>플레이어가 가까울 때만 표시</summary>
        Proximity,
        /// <summary>외부에서 SetVisible로만 제어 (문 트리거 등)</summary>
        Manual
    }

    public class WorldLabel : MonoBehaviour
    {
        private static readonly List<WorldLabel> Active = new(32);
        private static Transform _player;

        private TextMesh _text;
        private MeshRenderer _textRenderer;
        private MeshRenderer _plateRenderer;
        private float _baseSize = 0.045f;
        private Color _baseColor = Color.white;
        private Transform _follow;
        private Vector3 _followOffset;
        private Transform _plate;
        private LabelShowMode _mode = LabelShowMode.Always;
        private float _maxDistance = 3.5f;
        private bool _manualVisible = true;
        private float _stackBoost;
        private int _priority;

        public Vector3 WorldAnchor
        {
            get
            {
                if (_follow != null)
                {
                    return _follow.TransformPoint(_followOffset);
                }

                return transform.position;
            }
        }

        public static WorldLabel Attach(Transform parent, string text, Vector3 offset, Color color,
            float characterSize = 0.045f, LabelShowMode mode = LabelShowMode.Always, float maxDistance = 3.5f,
            int priority = 0)
        {
            var go = new GameObject($"Label_{text}");
            // 반드시 부모에 붙여 방/오브젝트 파괴 시 함께 사라지게 함
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
                go.transform.localPosition = offset;
            }

            var follower = go.AddComponent<WorldLabel>();
            follower._baseSize = Mathf.Clamp(characterSize, 0.028f, 0.06f);
            follower._baseColor = color;
            follower._follow = parent;
            follower._followOffset = offset;
            follower._mode = mode;
            follower._maxDistance = maxDistance;
            follower._manualVisible = mode != LabelShowMode.Manual;
            follower._priority = priority;

            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = 42;
            tm.characterSize = follower._baseSize;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = color;
            tm.fontStyle = FontStyle.Bold;
            tm.font = DungeonOdyssey.Core.GameFonts.Ui();
            follower._text = tm;
            follower._textRenderer = tm.GetComponent<MeshRenderer>();
            if (follower._textRenderer != null)
            {
                follower._textRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            var plate = GameObject.CreatePrimitive(PrimitiveType.Quad);
            plate.name = "Plate";
            plate.transform.SetParent(go.transform, false);
            plate.transform.localPosition = new Vector3(0f, 0f, 0.08f);
            plate.transform.localScale = new Vector3(Mathf.Clamp(text.Length * 0.08f, 0.8f, 2.4f), 0.24f, 1f);
            Object.Destroy(plate.GetComponent<Collider>());
            plate.GetComponent<MeshRenderer>().sharedMaterial =
                MatLib.Get(new Color(0.05f, 0.05f, 0.07f), 0.05f);
            follower._plate = plate.transform;
            follower._plateRenderer = plate.GetComponent<MeshRenderer>();

            follower.ApplyVisibility(follower._manualVisible && mode != LabelShowMode.Proximity);
            return follower;
        }

        private void OnEnable()
        {
            if (!Active.Contains(this))
            {
                Active.Add(this);
            }
        }

        private void OnDisable()
        {
            Active.Remove(this);
        }

        private void OnDestroy()
        {
            Active.Remove(this);
        }

        public void SetVisible(bool visible)
        {
            _manualVisible = visible;
            if (_mode == LabelShowMode.Manual || _mode == LabelShowMode.Always)
            {
                ApplyVisibility(visible);
            }
        }

        public void SetText(string text)
        {
            if (_text == null)
            {
                _text = GetComponent<TextMesh>();
            }

            if (_text != null)
            {
                _text.text = text;
            }

            if (_plate != null && text != null)
            {
                _plate.localScale = new Vector3(Mathf.Clamp(text.Length * 0.08f, 0.8f, 2.4f), 0.24f, 1f);
            }
        }

        public void SetColor(Color color)
        {
            _baseColor = color;
            if (_text == null)
            {
                _text = GetComponent<TextMesh>();
            }

            if (_text != null)
            {
                _text.color = color;
            }
        }

        public void SetHighlight(bool on)
        {
            if (_text == null)
            {
                _text = GetComponent<TextMesh>();
            }

            if (_text == null)
            {
                return;
            }

            _text.characterSize = on ? _baseSize * 1.12f : _baseSize;
            _text.color = on ? Color.Lerp(_baseColor, Color.white, 0.4f) : _baseColor;
            if (_mode == LabelShowMode.Manual)
            {
                SetVisible(on);
            }
        }

        private void ApplyVisibility(bool visible)
        {
            if (_textRenderer != null)
            {
                _textRenderer.enabled = visible;
            }

            if (_plateRenderer != null)
            {
                _plateRenderer.enabled = visible;
            }
        }

        private bool IsShown()
        {
            if (_textRenderer != null)
            {
                return _textRenderer.enabled;
            }

            return false;
        }

        private void LateUpdate()
        {
            // 추적 대상이 파괴되면 고아 라벨 제거 (이전 방 잔상 방지)
            if (_follow == null)
            {
                Destroy(gameObject);
                return;
            }

            if (_mode == LabelShowMode.Proximity)
            {
                if (_player == null)
                {
                    var p = GameObject.FindGameObjectWithTag("Player");
                    if (p != null)
                    {
                        _player = p.transform;
                    }
                }

                var show = false;
                if (_player != null)
                {
                    var d = Vector3.Distance(_player.position, _follow.position);
                    show = d <= _maxDistance;
                }

                ApplyVisibility(show);
            }
            else if (_mode == LabelShowMode.Manual)
            {
                ApplyVisibility(_manualVisible);
            }
            else
            {
                ApplyVisibility(_manualVisible);
            }

            ResolveStacks();

            // 부모 로컬 오프셋 + 겹침 시 위로 밀어냄
            transform.localPosition = _followOffset + Vector3.up * _stackBoost;

            var cam = Camera.main;
            if (cam != null)
            {
                transform.rotation = cam.transform.rotation;
            }
        }

        /// <summary>XZ상 가까운 라벨끼리 높이를 나눠 겹침을 피합니다.</summary>
        private static void ResolveStacks()
        {
            for (var i = 0; i < Active.Count; i++)
            {
                if (Active[i] != null)
                {
                    Active[i]._stackBoost = 0f;
                }
            }

            for (var i = 0; i < Active.Count; i++)
            {
                var a = Active[i];
                if (a == null || !a.IsShown() || a._follow == null)
                {
                    continue;
                }

                var stack = 0;
                var aPos = a._follow.position;
                for (var j = 0; j < Active.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    var b = Active[j];
                    if (b == null || !b.IsShown() || b._follow == null)
                    {
                        continue;
                    }

                    var bPos = b._follow.position;
                    var dx = aPos.x - bPos.x;
                    var dz = aPos.z - bPos.z;
                    if (dx * dx + dz * dz > 2.8f * 2.8f)
                    {
                        continue;
                    }

                    // 우선순위 낮거나, 같으면 인스턴스ID로 아래쪽
                    var aBelow = a._priority < b._priority
                                 || (a._priority == b._priority && a.GetHashCode() < b.GetHashCode());
                    if (aBelow)
                    {
                        stack++;
                    }
                }

                a._stackBoost = stack * 0.55f;
            }
        }
    }
}
