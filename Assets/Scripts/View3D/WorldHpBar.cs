using DungeonOdyssey.Combat;
using UnityEngine;

namespace DungeonOdyssey.View3D
{
    public class WorldHpBar : MonoBehaviour
    {
        private Health _health;
        private Transform _fill;
        private Transform _root;
        private float _width = 0.9f;

        public static void Attach(GameObject host, float yOffset = 2.15f, float width = 0.9f)
        {
            var bar = host.AddComponent<WorldHpBar>();
            bar._health = host.GetComponent<Health>();
            bar._width = width;
            bar.Build(yOffset);
        }

        private void Build(float yOffset)
        {
            _root = new GameObject("HpBar").transform;
            _root.SetParent(transform, false);
            _root.localPosition = new Vector3(0f, yOffset, 0f);

            var bg = Prim.Cube("Bg", _root, Vector3.zero, new Vector3(_width, 0.08f, 0.08f),
                new Color(0.1f, 0.1f, 0.12f));
            Object.Destroy(bg.GetComponent<Collider>());

            var fillGo = Prim.Cube("Fill", _root, new Vector3(0f, 0f, -0.01f),
                new Vector3(_width * 0.96f, 0.05f, 0.05f), MatLib.AccentWine);
            Object.Destroy(fillGo.GetComponent<Collider>());
            _fill = fillGo.transform;

            if (_health != null)
            {
                _health.OnHealthChanged += OnHp;
                OnHp(_health.CurrentHp, _health.MaxHp);
            }
        }

        private void OnHp(int cur, int max)
        {
            if (_fill == null || max <= 0)
            {
                return;
            }

            var p = Mathf.Clamp01(cur / (float)max);
            _fill.localScale = new Vector3(_width * 0.96f * p, 0.06f, 0.06f);
            _fill.localPosition = new Vector3((-_width * 0.96f * (1f - p)) * 0.5f, 0f, -0.01f);
            // 초록(안전) → 노랑 → 빨강(위험)
            Color c;
            if (p > 0.55f)
            {
                c = Color.Lerp(new Color(1f, 0.85f, 0.2f), new Color(0.35f, 0.9f, 0.4f), (p - 0.55f) / 0.45f);
            }
            else
            {
                c = Color.Lerp(new Color(0.95f, 0.2f, 0.2f), new Color(1f, 0.85f, 0.2f), p / 0.55f);
            }

            _fill.GetComponent<MeshRenderer>().sharedMaterial = MatLib.Get(c);
        }

        private void LateUpdate()
        {
            if (_root == null)
            {
                return;
            }

            var cam = Camera.main;
            if (cam != null)
            {
                _root.rotation = cam.transform.rotation;
            }
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.OnHealthChanged -= OnHp;
            }
        }
    }
}
