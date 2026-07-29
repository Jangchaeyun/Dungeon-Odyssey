using UnityEngine;

namespace DungeonOdyssey.Dungeon
{
    /// <summary>
    /// 간단한 픽셀 스프라이트를 런타임 생성합니다.
    /// </summary>
    public static class SpriteFactory
    {
        private static Sprite _circle;
        private static Sprite _rounded;
        private static Sprite _tile;

        public static Sprite Circle => _circle ??= BuildCircle(32);
        public static Sprite Rounded => _rounded ??= BuildRoundedRect(32, 32, 6);
        public static Sprite Tile => _tile ??= BuildFilled(16, 16, Color.white);

        public static Sprite BuildFilled(int w, int h, Color color)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            var pixels = new Color[w * h];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
        }

        public static Sprite BuildCircle(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            var center = (size - 1) * 0.5f;
            var radius = size * 0.45f;
            var outline = radius - 1.5f;

            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dx = x - center;
                var dy = y - center;
                var dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist <= radius)
                {
                    // 외곽선 + 본체는 흰색(색은 SpriteRenderer에서 칠함)
                    tex.SetPixel(x, y, dist >= outline ? new Color(0f, 0f, 0f, 1f) : Color.white);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }

            // 눈
            var eyeY = Mathf.RoundToInt(center + size * 0.08f);
            var eyeLX = Mathf.RoundToInt(center - size * 0.12f);
            var eyeRX = Mathf.RoundToInt(center + size * 0.12f);
            SetPixelSafe(tex, eyeLX, eyeY, Color.black);
            SetPixelSafe(tex, eyeRX, eyeY, Color.black);

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
        }

        public static Sprite BuildRoundedRect(int w, int h, int radius)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
            {
                var inside = IsInsideRounded(x, y, w, h, radius);
                if (!inside)
                {
                    tex.SetPixel(x, y, Color.clear);
                    continue;
                }

                var edge = !IsInsideRounded(x, y, w, h, radius - 1);
                tex.SetPixel(x, y, edge ? Color.black : Color.white);
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
        }

        public static Sprite BuildSlime(int size = 32)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            var cx = (size - 1) * 0.5f;
            var cy = size * 0.42f;
            var rx = size * 0.42f;
            var ry = size * 0.36f;

            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var nx = (x - cx) / rx;
                var ny = (y - cy) / ry;
                var d = nx * nx + ny * ny;
                if (d <= 1f)
                {
                    var edge = d > 0.78f;
                    tex.SetPixel(x, y, edge ? Color.black : Color.white);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }

            SetPixelSafe(tex, Mathf.RoundToInt(cx - 4), Mathf.RoundToInt(cy + 3), Color.black);
            SetPixelSafe(tex, Mathf.RoundToInt(cx + 4), Mathf.RoundToInt(cy + 3), Color.black);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16f);
        }

        public static Sprite BuildDoor(int w = 24, int h = 32)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
            {
                var border = x == 0 || y == 0 || x == w - 1 || y == h - 1;
                tex.SetPixel(x, y, border ? Color.black : Color.white);
            }

            // 손잡이
            SetPixelSafe(tex, w - 6, h / 2, Color.black);
            SetPixelSafe(tex, w - 5, h / 2, Color.black);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
        }

        private static bool IsInsideRounded(int x, int y, int w, int h, int r)
        {
            if (r <= 0)
            {
                return x >= 0 && y >= 0 && x < w && y < h;
            }

            var cx = Mathf.Clamp(x, r, w - 1 - r);
            var cy = Mathf.Clamp(y, r, h - 1 - r);
            var dx = x - cx;
            var dy = y - cy;
            return dx * dx + dy * dy <= r * r;
        }

        private static void SetPixelSafe(Texture2D tex, int x, int y, Color c)
        {
            if (x < 0 || y < 0 || x >= tex.width || y >= tex.height)
            {
                return;
            }

            tex.SetPixel(x, y, c);
        }
    }
}
