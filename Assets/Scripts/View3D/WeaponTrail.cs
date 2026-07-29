using UnityEngine;

namespace DungeonOdyssey.View3D
{
    /// <summary>
    /// 칼끝 잔상 — 휘두름 궤적을 촘촘히 남겨 검이 실제로 지나간 느낌을 준다.
    /// </summary>
    public class WeaponTrail : MonoBehaviour
    {
        private Transform _tip;
        private Transform _root;
        private bool _active;
        private float _timer;
        private Vector3 _lastTip;
        private bool _hasLast;
        private float _spawnCooldown;
        private int _combo;
        private Color _color = new(0.85f, 0.9f, 1f, 0.5f);

        public void Configure(Transform tip, Transform root = null)
        {
            _tip = tip != null ? tip : transform;
            _root = root;
        }

        public void Begin(float duration = 0.22f, int combo = 0)
        {
            _active = true;
            _timer = duration;
            _hasLast = false;
            _spawnCooldown = 0f;
            _combo = combo;
            _color = combo == 2
                ? new Color(1f, 0.82f, 0.48f, 0.62f)
                : new Color(0.88f, 0.92f, 1f, 0.55f);
        }

        private void LateUpdate()
        {
            if (!_active || _tip == null)
            {
                return;
            }

            _timer -= Time.unscaledDeltaTime;
            if (_timer <= 0f)
            {
                _active = false;
                return;
            }

            var tip = _tip.position;
            _spawnCooldown -= Time.unscaledDeltaTime;

            if (_hasLast && _spawnCooldown <= 0f)
            {
                var delta = tip - _lastTip;
                if (delta.sqrMagnitude > 0.0016f)
                {
                    SpawnSegment(_lastTip, tip);
                    _spawnCooldown = _combo == 2 ? 0.012f : 0.016f;
                }
            }

            _lastTip = tip;
            _hasLast = true;
        }

        private void SpawnSegment(Vector3 a, Vector3 b)
        {
            var mid = (a + b) * 0.5f;
            var dir = b - a;
            var len = dir.magnitude;
            if (len < 0.015f || len > 0.9f)
            {
                return;
            }

            var up = _tip != null ? _tip.up : Vector3.up;
            if (_root != null)
            {
                up = TipDirSafe(b - _root.position);
            }

            if (Mathf.Abs(Vector3.Dot(dir.normalized, up)) > 0.92f)
            {
                up = Vector3.up;
            }

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "SwordTrail";
            go.transform.position = mid;
            go.transform.rotation = Quaternion.LookRotation(dir.normalized, up);
            var thick = _combo == 2 ? 0.042f : 0.034f;
            go.transform.localScale = new Vector3(thick, thick * 0.45f, len);
            go.GetComponent<MeshRenderer>().sharedMaterial = MatLib.GetFx(_color, 0.75f);
            Object.Destroy(go.GetComponent<Collider>());

            var fade = go.AddComponent<TrailFade>();
            fade.Life = _combo == 2 ? 0.11f : 0.09f;
            Object.Destroy(go, fade.Life + 0.02f);
        }

        private static Vector3 TipDirSafe(Vector3 v)
        {
            return v.sqrMagnitude > 0.001f ? v.normalized : Vector3.up;
        }

        private class TrailFade : MonoBehaviour
        {
            public float Life = 0.08f;
            private float _t;
            private Material _mat;
            private Color _base;
            private Vector3 _scale0;

            private void Start()
            {
                _scale0 = transform.localScale;
                var r = GetComponent<MeshRenderer>();
                if (r != null)
                {
                    _mat = r.material;
                    _base = _mat.HasProperty("_BaseColor") ? _mat.GetColor("_BaseColor") : _mat.color;
                }
            }

            private void Update()
            {
                _t += Time.unscaledDeltaTime;
                var p = Mathf.Clamp01(_t / Life);
                transform.localScale = new Vector3(
                    _scale0.x * (1f - p * 0.35f),
                    _scale0.y * (1f - p * 0.5f),
                    _scale0.z);

                if (_mat != null)
                {
                    var c = _base;
                    c.a = _base.a * (1f - p);
                    _mat.color = c;
                    if (_mat.HasProperty("_BaseColor"))
                    {
                        _mat.SetColor("_BaseColor", c);
                    }
                }
            }
        }
    }
}
