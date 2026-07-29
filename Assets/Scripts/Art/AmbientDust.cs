using UnityEngine;

namespace DungeonOdyssey.Art
{
    /// <summary>
    /// 가벼운 먼지. 개수·갱신 주기를 낮춰 성능을 확보합니다.
    /// </summary>
    public class AmbientDust : MonoBehaviour
    {
        private struct Speck
        {
            public Transform Transform;
            public SpriteRenderer Renderer;
            public Vector3 Velocity;
            public float Life;
            public float MaxLife;
        }

        private Speck[] _specks;
        private Color _color;
        private float _tick;
        private static Sprite _dot;

        public static void Create(Transform parent, Color color, int count = 6)
        {
            count = Mathf.Clamp(count, 0, 8);
            if (count == 0)
            {
                return;
            }

            var go = new GameObject("AmbientDust");
            go.transform.SetParent(parent, false);
            go.AddComponent<AmbientDust>().Build(color, count);
        }

        private void Build(Color color, int count)
        {
            _color = color;
            _specks = new Speck[count];
            var dot = GetDotSprite();

            for (var i = 0; i < count; i++)
            {
                var speckGo = new GameObject($"Speck_{i}");
                speckGo.transform.SetParent(transform, false);
                var sr = speckGo.AddComponent<SpriteRenderer>();
                sr.sprite = dot;
                sr.sortingOrder = 30;
                _specks[i] = new Speck { Transform = speckGo.transform, Renderer = sr };
                Respawn(ref _specks[i], true);
            }
        }

        private void Update()
        {
            if (_specks == null)
            {
                return;
            }

            // 30fps 수준으로만 갱신
            _tick += Time.deltaTime;
            if (_tick < 0.033f)
            {
                return;
            }

            var dt = _tick;
            _tick = 0f;

            for (var i = 0; i < _specks.Length; i++)
            {
                ref var speck = ref _specks[i];
                speck.Life -= dt;
                speck.Transform.position += speck.Velocity * dt;
                var t = 1f - Mathf.Clamp01(speck.Life / speck.MaxLife);
                var alpha = Mathf.Sin(t * Mathf.PI) * _color.a * 0.55f;
                speck.Renderer.color = new Color(_color.r, _color.g, _color.b, alpha);
                if (speck.Life <= 0f)
                {
                    Respawn(ref speck, false);
                }
            }
        }

        private void Respawn(ref Speck speck, bool randomizeLife)
        {
            var origin = transform.position;
            // 3D 탑뷰: XZ 평면에서 떠다니고 Y는 높이
            speck.Transform.position = origin + new Vector3(
                Random.Range(-7f, 7f), Random.Range(0.4f, 3.2f), Random.Range(-6f, 6f));
            speck.Velocity = new Vector3(
                Random.Range(-0.2f, 0.2f), Random.Range(0.05f, 0.22f), Random.Range(-0.2f, 0.2f));
            speck.MaxLife = Random.Range(1.5f, 2.8f);
            speck.Life = randomizeLife ? Random.Range(0f, speck.MaxLife) : speck.MaxLife;
            speck.Transform.localScale = Vector3.one * Random.Range(0.05f, 0.1f);
        }

        private static Sprite GetDotSprite()
        {
            if (_dot != null)
            {
                return _dot;
            }

            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };
            for (var y = 0; y < 4; y++)
            for (var x = 0; x < 4; x++)
            {
                var dx = x - 1.5f;
                var dy = y - 1.5f;
                tex.SetPixel(x, y, dx * dx + dy * dy <= 2.2f ? Color.white : Color.clear);
            }

            tex.Apply();
            _dot = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            return _dot;
        }
    }
}
