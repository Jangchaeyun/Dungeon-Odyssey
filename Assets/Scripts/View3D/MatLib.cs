using UnityEngine;

namespace DungeonOdyssey.View3D
{
    /// <summary>
    /// 다크 판타지 · 성숙한 톤 팔레트 (채도 낮고 금속·그림자 위주).
    /// </summary>
    public static class MatLib
    {
        private static readonly System.Collections.Generic.Dictionary<Color, Material> Cache = new();

        public static Material Get(Color color, float smoothness = 0.28f, float metallic = 0f)
        {
            if (Cache.TryGetValue(color, out var mat) && mat != null)
            {
                return mat;
            }

            // Built-in 우선. 분홍(에러 셰이더) 방지용 폴백.
            var shader = Shader.Find("Standard")
                         ?? Shader.Find("Legacy Shaders/Diffuse")
                         ?? Shader.Find("Diffuse")
                         ?? Shader.Find("Unlit/Color")
                         ?? Shader.Find("Sprites/Default")
                         ?? Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                // 최후: 기본 머티리얼이라도 반환
                mat = new Material(Shader.Find("Hidden/InternalErrorShader") ?? Shader.Find("Standard"));
            }
            else
            {
                mat = new Material(shader);
            }

            if (mat.HasProperty("_Color"))
            {
                mat.color = color;
            }

            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }

            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", smoothness);
            }
            else if (mat.HasProperty("_Glossiness"))
            {
                mat.SetFloat("_Glossiness", smoothness);
            }

            if (mat.HasProperty("_Metallic"))
            {
                mat.SetFloat("_Metallic", metallic > 0f ? metallic : (smoothness > 0.5f ? 0.45f : 0.05f));
            }

            Cache[color] = mat;
            return mat;
        }

        /// <summary>
        /// 3D 이펙트용 머티리얼. Sprites/Default는 MeshRenderer에서 안 보이는 경우가 많아
        /// Unlit/Color · Standard 등 불투명+발광으로 만든다 (인스턴스마다 새로 생성).
        /// </summary>
        public static Material GetFx(Color color, float smoothness = 0.6f)
        {
            var c = color;
            if (c.a < 0.7f)
            {
                c.a = 1f;
            }

            var shader = Shader.Find("Unlit/Color")
                         ?? Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Standard")
                         ?? Shader.Find("Legacy Shaders/Diffuse")
                         ?? Shader.Find("Sprites/Default");

            Material mat;
            if (shader == null)
            {
                mat = new Material(Get(c, smoothness));
            }
            else
            {
                mat = new Material(shader);
            }

            if (mat.HasProperty("_Color"))
            {
                mat.color = c;
            }

            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", c);
            }

            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", c * 2.2f);
            }

            if (mat.HasProperty("_Smoothness"))
            {
                mat.SetFloat("_Smoothness", smoothness);
            }

            // URP Unlit 표면 타입 불투명
            if (mat.HasProperty("_Surface"))
            {
                mat.SetFloat("_Surface", 0f);
            }

            mat.renderQueue = 2450;
            return mat;
        }

        /// <summary>약한 발광(횃불·룬·포탈용). 캐시는 색+강도 키.</summary>
        public static Material GetEmissive(Color color, Color emission, float emissionIntensity = 1.2f,
            float smoothness = 0.45f)
        {
            var key = color * 0.7f + emission * (0.3f + emissionIntensity * 0.01f);
            if (Cache.TryGetValue(key, out var mat) && mat != null)
            {
                return mat;
            }

            mat = Get(color, smoothness);
            // Get은 캐시된 공유 머티리얼을 줄 수 있으니 복제
            mat = new Material(mat);
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission * emissionIntensity);
            }

            Cache[key] = mat;
            return mat;
        }

        // Characters
        public static readonly Color Skin = new(0.8f, 0.64f, 0.54f);
        public static readonly Color Hair = new(0.09f, 0.08f, 0.09f);
        public static readonly Color Cloth = new(0.13f, 0.15f, 0.19f);       // charcoal gambeson
        public static readonly Color Pants = new(0.11f, 0.11f, 0.13f);
        public static readonly Color Boot = new(0.15f, 0.11f, 0.09f);
        public static readonly Color Metal = new(0.58f, 0.6f, 0.64f);        // brushed steel
        public static readonly Color MetalDark = new(0.26f, 0.28f, 0.32f);
        public static readonly Color Belt = new(0.4f, 0.26f, 0.15f);         // aged leather
        public static readonly Color Cape = new(0.16f, 0.07f, 0.09f);         // deep wine
        public static readonly Color EyeSteel = new(0.58f, 0.68f, 0.72f);

        // Monster — void ichor, not candy green
        public static readonly Color Slime = new(0.22f, 0.12f, 0.28f);
        public static readonly Color SlimeDark = new(0.08f, 0.05f, 0.12f);
        public static readonly Color SlimeGlow = new(0.55f, 0.22f, 0.35f);
        public static readonly Color Bone = new(0.72f, 0.68f, 0.58f);
        public static readonly Color EyeRed = new(0.85f, 0.35f, 0.2f);
        public static readonly Color EyeBlue = new(0.45f, 0.55f, 0.65f);

        // NPC
        public static readonly Color NpcRobe = new(0.22f, 0.18f, 0.28f);     // indigo dusk
        public static readonly Color NpcTrim = new(0.62f, 0.52f, 0.38f);     // muted brass

        // World — dusk / ash
        public static readonly Color Grass = new(0.18f, 0.26f, 0.17f);
        public static readonly Color GrassDark = new(0.11f, 0.15f, 0.11f);
        public static readonly Color Path = new(0.38f, 0.34f, 0.28f);          // packed dirt
        public static readonly Color Stone = new(0.42f, 0.41f, 0.43f);
        public static readonly Color StoneDark = new(0.22f, 0.22f, 0.24f);
        public static readonly Color Wood = new(0.3f, 0.21f, 0.14f);
        public static readonly Color Roof = new(0.18f, 0.16f, 0.2f);         // slate
        public static readonly Color Leaf = new(0.16f, 0.24f, 0.16f);
        public static readonly Color DungeonFloor = new(0.09f, 0.09f, 0.11f);
        public static readonly Color DungeonWall = new(0.15f, 0.15f, 0.18f);
        public static readonly Color AccentTeal = new(0.35f, 0.72f, 0.7f);   // ice accent
        public static readonly Color AccentCopper = new(0.55f, 0.42f, 0.32f);
        public static readonly Color AccentWine = new(0.55f, 0.18f, 0.28f);
        public static readonly Color GoldTrim = new(0.45f, 0.78f, 0.75f);    // cool trim (legacy name)
        public static readonly Color Fog = new(0.11f, 0.12f, 0.15f);
        public static readonly Color White = new(0.92f, 0.91f, 0.88f);
        public static readonly Color Black = new(0.04f, 0.04f, 0.05f);
    }
}
