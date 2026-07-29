using UnityEngine;

namespace DungeonOdyssey.Art
{
    /// <summary>
    /// 2D 라이트 패키지 없이도 동작하는 원형 빛 오버레이.
    /// </summary>
    public class PlayerLight : MonoBehaviour
    {
        public static void Attach(Transform player, Color color, float scale = 8f)
        {
            var go = new GameObject("PlayerLight");
            go.transform.SetParent(player, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one * scale;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = BuildRadialSprite();
            sr.color = color;
            sr.sortingOrder = 8;
            go.AddComponent<PlayerLight>();
        }

        private static Sprite _radial;

        private static Sprite BuildRadialSprite()
        {
            if (_radial != null)
            {
                return _radial;
            }

            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            var center = (size - 1) * 0.5f;
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dx = (x - center) / center;
                var dy = (y - center) / center;
                var d = Mathf.Sqrt(dx * dx + dy * dy);
                var a = Mathf.Clamp01(1f - d);
                a = a * a;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a * 0.55f));
            }

            tex.Apply();
            _radial = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64f);
            return _radial;
        }
    }
}
