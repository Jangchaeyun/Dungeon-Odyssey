using System.Collections.Generic;
using DungeonOdyssey.Player;
using UnityEngine;

namespace DungeonOdyssey.UI
{
    /// <summary>상점·가방용 절차적 아이템/무기 아이콘 (큰 실루엣).</summary>
    public static class UiItemIcon
    {
        private static readonly Dictionary<string, Sprite> Cache = new();
        private const int Size = 96;

        public static Sprite ForShopKind(string kind, WeaponId weapon = default)
        {
            if (kind == "weapon")
            {
                return ForWeapon(weapon);
            }

            return kind switch
            {
                "heal" => GetOrCreate("v3_heal", DrawHeal),
                "potion" => GetOrCreate("v3_potion", DrawPotion),
                "luck" => GetOrCreate("v3_luck", DrawLuck),
                "forge" => GetOrCreate("v3_forge", DrawForge),
                "precision" => GetOrCreate("v3_precision", DrawPrecision),
                "sigil" => GetOrCreate("v3_sigil", DrawSigil),
                "skill" => GetOrCreate("v3_skill", DrawSkill),
                "gold" => GetOrCreate("v3_gold", DrawGold),
                "shop" => GetOrCreate("v3_shop", DrawShop),
                "bag" => GetOrCreate("v3_bag", DrawBag),
                "quest" => GetOrCreate("v3_quest", DrawQuest),
                _ => GetOrCreate("v3_box", DrawBox)
            };
        }

        public static Sprite ForWeapon(WeaponId id) =>
            GetOrCreate($"v3_wep_{(int)id}", () => DrawWeapon(id));

        private static Sprite GetOrCreate(string key, System.Func<Texture2D> bake)
        {
            if (Cache.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            var tex = bake();
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply(false, false);
            var sprite = Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
            Cache[key] = sprite;
            return sprite;
        }

        private static Texture2D DrawWeapon(WeaponId id)
        {
            var tex = Blank();
            var def = WeaponCatalog.Get(id);
            var blade = ElementColor(def.Element);
            var edge = Color.Lerp(blade, Color.white, 0.55f);
            var shade = Color.Lerp(blade, Color.black, 0.35f);
            var grip = new Color(0.38f, 0.22f, 0.12f, 1f);
            var gripHi = new Color(0.55f, 0.35f, 0.18f, 1f);
            var guard = Color.Lerp(blade, new Color(0.65f, 0.58f, 0.45f), 0.4f);

            switch (WeaponShape(id))
            {
                case Shape.Spear:
                    FillRect(tex, 44, 8, 8, 58, grip);
                    FillRect(tex, 46, 10, 4, 54, gripHi);
                    FillRect(tex, 40, 62, 16, 8, guard);
                    FillTri(tex, 48, 92, 34, 64, 62, 64, blade);
                    FillTri(tex, 48, 88, 40, 68, 56, 68, edge);
                    break;
                case Shape.Axe:
                    FillRect(tex, 44, 10, 8, 50, grip);
                    FillRect(tex, 46, 12, 4, 46, gripHi);
                    FillRect(tex, 18, 52, 42, 22, blade);
                    FillRect(tex, 18, 54, 14, 18, edge);
                    FillRect(tex, 52, 56, 10, 14, shade);
                    FillRect(tex, 36, 14, 24, 8, guard);
                    break;
                case Shape.Hammer:
                    FillRect(tex, 44, 8, 8, 48, grip);
                    FillRect(tex, 46, 10, 4, 44, gripHi);
                    FillRect(tex, 20, 54, 56, 26, blade);
                    FillRect(tex, 24, 58, 48, 18, edge);
                    FillRect(tex, 28, 62, 40, 10, Color.Lerp(edge, Color.white, 0.3f));
                    break;
                case Shape.Dagger:
                    FillRect(tex, 44, 14, 8, 22, grip);
                    FillRect(tex, 34, 34, 28, 6, guard);
                    FillRect(tex, 42, 38, 12, 36, blade);
                    FillRect(tex, 44, 40, 8, 30, edge);
                    FillTri(tex, 48, 88, 40, 72, 56, 72, edge);
                    break;
                case Shape.Rapier:
                    FillRect(tex, 45, 10, 6, 20, grip);
                    FillCircle(tex, 48, 32, 10, guard);
                    FillRect(tex, 46, 36, 4, 48, blade);
                    FillRect(tex, 47, 40, 2, 40, edge);
                    FillTri(tex, 48, 90, 44, 82, 52, 82, edge);
                    break;
                case Shape.Scythe:
                    FillRect(tex, 28, 10, 8, 60, grip);
                    FillRect(tex, 30, 12, 4, 56, gripHi);
                    // 낫날
                    for (var a = -10; a <= 100; a += 2)
                    {
                        var rad = a * Mathf.Deg2Rad;
                        for (var t = 0; t < 28; t++)
                        {
                            var r = 18 + t * 0.55f;
                            var px = 36 + Mathf.RoundToInt(Mathf.Cos(rad) * r);
                            var py = 58 + Mathf.RoundToInt(Mathf.Sin(rad) * r * 0.85f);
                            Set(tex, px, py, t > 18 ? edge : blade);
                            if (t > 10 && t < 16)
                            {
                                Set(tex, px, py - 1, shade);
                            }
                        }
                    }

                    break;
                default:
                    var thick = id is WeaponId.ObsidianGreatsword or WeaponId.StormCleaver or WeaponId.SolarCleaver
                        ? 14
                        : 10;
                    FillRect(tex, 44, 6, 8, 20, grip);
                    FillRect(tex, 46, 8, 4, 16, gripHi);
                    FillRect(tex, 28, 24, 40, 8, guard);
                    FillRect(tex, 48 - thick / 2, 30, thick, 48, blade);
                    FillRect(tex, 48 - thick / 2 + 2, 32, thick - 4, 42, edge);
                    FillRect(tex, 48 - thick / 2 + 3, 34, 2, 36, Color.Lerp(edge, Color.white, 0.4f));
                    FillTri(tex, 48, 92, 48 - thick / 2 - 2, 76, 48 + thick / 2 + 2, 76, edge);
                    break;
            }

            // 속성 보석
            if (def.Element != WeaponElement.None)
            {
                FillCircle(tex, 16, 16, 8, Color.Lerp(blade, Color.black, 0.2f));
                FillCircle(tex, 16, 16, 5, blade);
                FillCircle(tex, 14, 18, 2, Color.white);
            }

            return tex;
        }

        private enum Shape
        {
            Sword,
            Spear,
            Axe,
            Hammer,
            Dagger,
            Rapier,
            Scythe
        }

        private static Shape WeaponShape(WeaponId id) => id switch
        {
            WeaponId.HunterSpear or WeaponId.EmberPike => Shape.Spear,
            WeaponId.CrimsonAxe or WeaponId.GlacierAxe => Shape.Axe,
            WeaponId.StoneMaul or WeaponId.TitanHammer => Shape.Hammer,
            WeaponId.ShadowDagger => Shape.Dagger,
            WeaponId.SilverRapier => Shape.Rapier,
            WeaponId.VoidReaper or WeaponId.MoonScythe => Shape.Scythe,
            _ => Shape.Sword
        };

        private static Color ElementColor(WeaponElement el) => el switch
        {
            WeaponElement.Flame => new Color(1f, 0.42f, 0.18f),
            WeaponElement.Frost => new Color(0.45f, 0.82f, 1f),
            WeaponElement.Storm => new Color(0.65f, 0.8f, 1f),
            WeaponElement.Void => new Color(0.72f, 0.38f, 0.95f),
            WeaponElement.Holy => new Color(1f, 0.9f, 0.5f),
            WeaponElement.Nature => new Color(0.4f, 0.85f, 0.45f),
            WeaponElement.Blood => new Color(0.92f, 0.22f, 0.32f),
            WeaponElement.Earth => new Color(0.78f, 0.58f, 0.35f),
            WeaponElement.Steel => new Color(0.72f, 0.8f, 0.92f),
            _ => new Color(0.85f, 0.88f, 0.94f)
        };

        private static Texture2D DrawHeal()
        {
            var tex = Blank();
            var wood = new Color(0.58f, 0.38f, 0.2f);
            var woodD = new Color(0.35f, 0.22f, 0.12f);
            var cloth = new Color(0.9f, 0.3f, 0.3f);
            FillRect(tex, 18, 22, 60, 48, wood);
            FillRect(tex, 22, 26, 52, 40, woodD);
            FillRect(tex, 28, 30, 40, 32, new Color(0.45f, 0.3f, 0.16f));
            // 십자
            FillRect(tex, 42, 28, 12, 40, cloth);
            FillRect(tex, 28, 42, 40, 12, cloth);
            FillRect(tex, 44, 30, 8, 36, Color.Lerp(cloth, Color.white, 0.25f));
            return tex;
        }

        private static Texture2D DrawPotion()
        {
            var tex = Blank();
            var glass = new Color(0.45f, 0.9f, 0.65f);
            var liquid = new Color(0.15f, 0.7f, 0.4f);
            var cork = new Color(0.6f, 0.38f, 0.18f);
            var shine = new Color(1f, 1f, 1f, 0.45f);
            FillRect(tex, 38, 72, 20, 12, cork);
            FillRect(tex, 42, 58, 12, 16, glass);
            FillCircle(tex, 48, 36, 26, glass);
            FillCircle(tex, 48, 32, 20, liquid);
            FillCircle(tex, 48, 28, 12, Color.Lerp(liquid, Color.black, 0.15f));
            FillRect(tex, 40, 44, 6, 16, shine);
            FillCircle(tex, 38, 42, 4, shine);
            return tex;
        }

        private static Texture2D DrawLuck()
        {
            var tex = Blank();
            var gold = new Color(1f, 0.8f, 0.35f);
            var gem = new Color(0.7f, 0.4f, 1f);
            FillCircle(tex, 48, 44, 26, gold);
            FillCircle(tex, 48, 44, 18, gem);
            FillCircle(tex, 48, 44, 10, Color.Lerp(gem, Color.white, 0.35f));
            FillCircle(tex, 42, 50, 4, Color.white);
            FillRect(tex, 44, 10, 8, 18, gold);
            FillCircle(tex, 48, 10, 6, gold);
            return tex;
        }

        private static Texture2D DrawForge()
        {
            var tex = Blank();
            var iron = new Color(0.5f, 0.48f, 0.45f);
            var ironD = new Color(0.3f, 0.28f, 0.26f);
            var ember = new Color(1f, 0.45f, 0.15f);
            var wood = new Color(0.42f, 0.26f, 0.12f);
            // 모루
            FillRect(tex, 16, 16, 56, 24, iron);
            FillRect(tex, 24, 36, 40, 16, ironD);
            FillRect(tex, 20, 18, 48, 8, Color.Lerp(iron, Color.white, 0.2f));
            // 망치
            FillRect(tex, 58, 40, 10, 40, wood);
            FillRect(tex, 48, 70, 30, 14, iron);
            FillRect(tex, 50, 72, 26, 10, Color.Lerp(iron, Color.white, 0.25f));
            FillCircle(tex, 28, 28, 5, ember);
            FillCircle(tex, 36, 22, 3, new Color(1f, 0.8f, 0.3f));
            return tex;
        }

        private static Texture2D DrawPrecision()
        {
            var tex = Blank();
            var steel = new Color(0.75f, 0.8f, 0.9f);
            var wood = new Color(0.42f, 0.26f, 0.12f);
            var spark = new Color(1f, 0.85f, 0.35f);
            FillRect(tex, 44, 12, 8, 40, wood);
            FillRect(tex, 24, 52, 48, 22, steel);
            FillRect(tex, 28, 56, 40, 14, Color.Lerp(steel, Color.white, 0.35f));
            FillCircle(tex, 72, 72, 6, spark);
            FillCircle(tex, 20, 68, 4, spark);
            FillCircle(tex, 78, 58, 3, Color.white);
            return tex;
        }

        private static Texture2D DrawSigil()
        {
            var tex = Blank();
            var rune = new Color(1f, 0.78f, 0.3f);
            var dark = new Color(0.22f, 0.15f, 0.08f);
            FillCircle(tex, 48, 48, 36, dark);
            FillCircle(tex, 48, 48, 28, new Color(0.35f, 0.22f, 0.1f));
            FillCircle(tex, 48, 48, 20, dark);
            FillRect(tex, 44, 22, 8, 52, rune);
            FillRect(tex, 22, 44, 52, 8, rune);
            FillCircle(tex, 48, 48, 8, rune);
            FillCircle(tex, 48, 48, 4, Color.white);
            return tex;
        }

        private static Texture2D DrawSkill()
        {
            var tex = Blank();
            var blue = new Color(0.35f, 0.6f, 1f);
            var glow = new Color(0.7f, 0.85f, 1f);
            FillCircle(tex, 48, 48, 32, new Color(0.12f, 0.18f, 0.32f));
            FillCircle(tex, 48, 48, 24, blue);
            FillCircle(tex, 48, 48, 14, glow);
            FillCircle(tex, 48, 48, 7, Color.white);
            FillTri(tex, 48, 88, 32, 62, 64, 62, blue);
            return tex;
        }

        private static Texture2D DrawGold()
        {
            var tex = Blank();
            var gold = new Color(1f, 0.82f, 0.3f);
            var dark = new Color(0.75f, 0.52f, 0.12f);
            FillCircle(tex, 34, 40, 20, dark);
            FillCircle(tex, 62, 40, 20, dark);
            FillCircle(tex, 48, 50, 24, gold);
            FillCircle(tex, 48, 50, 16, Color.Lerp(gold, Color.white, 0.35f));
            FillCircle(tex, 40, 56, 5, Color.white);
            return tex;
        }

        private static Texture2D DrawBox()
        {
            var tex = Blank();
            var wood = new Color(0.58f, 0.4f, 0.2f);
            FillRect(tex, 18, 22, 60, 52, wood);
            FillRect(tex, 22, 26, 52, 44, new Color(0.4f, 0.28f, 0.14f));
            FillRect(tex, 18, 44, 60, 8, new Color(0.7f, 0.5f, 0.25f));
            return tex;
        }

        private static Texture2D DrawShop()
        {
            var tex = Blank();
            var iron = new Color(0.45f, 0.42f, 0.4f);
            var ironHi = new Color(0.7f, 0.68f, 0.62f);
            var ember = new Color(1f, 0.55f, 0.18f);
            var wood = new Color(0.42f, 0.26f, 0.12f);
            // 모루
            FillRect(tex, 22, 28, 52, 14, iron);
            FillRect(tex, 26, 32, 44, 6, ironHi);
            FillRect(tex, 36, 14, 24, 16, iron);
            FillRect(tex, 30, 10, 36, 8, new Color(0.28f, 0.26f, 0.24f));
            // 망치
            FillRect(tex, 58, 48, 8, 28, wood);
            FillRect(tex, 48, 70, 28, 14, ember);
            FillRect(tex, 52, 74, 20, 6, Color.Lerp(ember, Color.white, 0.35f));
            return tex;
        }

        private static Texture2D DrawBag()
        {
            var tex = Blank();
            var leather = new Color(0.28f, 0.42f, 0.52f);
            var leatherHi = new Color(0.42f, 0.62f, 0.72f);
            var strap = new Color(0.75f, 0.62f, 0.35f);
            var dark = new Color(0.14f, 0.2f, 0.26f);
            // 가방 본체
            FillRect(tex, 22, 18, 52, 48, leather);
            FillRect(tex, 26, 22, 44, 40, dark);
            FillRect(tex, 28, 26, 40, 32, leatherHi);
            // 덮개
            FillRect(tex, 20, 58, 56, 16, leather);
            FillRect(tex, 24, 62, 48, 8, leatherHi);
            // 버클
            FillRect(tex, 42, 48, 12, 10, strap);
            FillRect(tex, 44, 50, 8, 6, Color.Lerp(strap, Color.white, 0.25f));
            return tex;
        }

        private static Texture2D DrawQuest()
        {
            var tex = Blank();
            var paper = new Color(0.78f, 0.84f, 0.82f);
            var paperHi = new Color(0.9f, 0.94f, 0.92f);
            var ink = new Color(0.18f, 0.28f, 0.32f);
            var seal = new Color(0.35f, 0.75f, 0.7f);
            // 두루마리
            FillRect(tex, 28, 16, 40, 64, paper);
            FillRect(tex, 32, 20, 32, 56, paperHi);
            FillRect(tex, 24, 14, 48, 10, seal);
            FillRect(tex, 24, 72, 48, 10, seal);
            // 글줄
            FillRect(tex, 36, 34, 24, 3, ink);
            FillRect(tex, 36, 44, 20, 3, ink);
            FillRect(tex, 36, 54, 26, 3, ink);
            return tex;
        }

        private static Texture2D Blank()
        {
            var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            var clear = new Color(0f, 0f, 0f, 0f);
            var px = new Color[Size * Size];
            for (var i = 0; i < px.Length; i++)
            {
                px[i] = clear;
            }

            tex.SetPixels(px);
            return tex;
        }

        private static void FillRect(Texture2D tex, int x, int y, int w, int h, Color c)
        {
            for (var py = y; py < y + h; py++)
            for (var px = x; px < x + w; px++)
            {
                Set(tex, px, py, c);
            }
        }

        private static void FillCircle(Texture2D tex, int cx, int cy, int r, Color c)
        {
            var r2 = r * r;
            for (var py = cy - r; py <= cy + r; py++)
            for (var px = cx - r; px <= cx + r; px++)
            {
                var dx = px - cx;
                var dy = py - cy;
                if (dx * dx + dy * dy <= r2)
                {
                    Set(tex, px, py, c);
                }
            }
        }

        private static void FillTri(Texture2D tex, int x1, int y1, int x2, int y2, int x3, int y3, Color c)
        {
            var minX = Mathf.Min(x1, Mathf.Min(x2, x3));
            var maxX = Mathf.Max(x1, Mathf.Max(x2, x3));
            var minY = Mathf.Min(y1, Mathf.Min(y2, y3));
            var maxY = Mathf.Max(y1, Mathf.Max(y2, y3));
            for (var py = minY; py <= maxY; py++)
            for (var px = minX; px <= maxX; px++)
            {
                if (PointInTri(px, py, x1, y1, x2, y2, x3, y3))
                {
                    Set(tex, px, py, c);
                }
            }
        }

        private static bool PointInTri(int px, int py, int x1, int y1, int x2, int y2, int x3, int y3)
        {
            float Sign(int ax, int ay, int bx, int by, int cx, int cy) =>
                (ax - cx) * (by - cy) - (bx - cx) * (ay - cy);

            var b1 = Sign(px, py, x1, y1, x2, y2) < 0f;
            var b2 = Sign(px, py, x2, y2, x3, y3) < 0f;
            var b3 = Sign(px, py, x3, y3, x1, y1) < 0f;
            return b1 == b2 && b2 == b3;
        }

        private static void Set(Texture2D tex, int x, int y, Color c)
        {
            if (x < 0 || y < 0 || x >= Size || y >= Size)
            {
                return;
            }

            tex.SetPixel(x, Size - 1 - y, c);
        }
    }
}
