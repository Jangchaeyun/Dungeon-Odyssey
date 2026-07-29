using UnityEngine;

namespace DungeonOdyssey.View3D
{
    /// <summary>
    /// Resources/Models 프리팹 로드. 머티리얼 파손(분홍)과 스케일 이상을 자동 교정합니다.
    /// </summary>
    public static class ModelCatalog
    {
        public const string Player = "Models/Player";
        public const string MonsterSlime = "Models/Monster_Slime";
        public const string NpcMira = "Models/Npc_Mira";
        public const string TownHouse = "Models/Town_House";
        public const string TownTree = "Models/Town_Tree";
        public const string TownStall = "Models/Town_Stall";
        public const string TownFountain = "Models/Town_Fountain";
        public const string DungeonPillar = "Models/Dungeon_Pillar";
        public const string DungeonTorch = "Models/Dungeon_Torch";
        public const string DungeonChest = "Models/Dungeon_Chest";
        public const string DungeonAltar = "Models/Dungeon_Altar";
        public const string DungeonDoor = "Models/Dungeon_Door";
        public const string DungeonGate = "Models/Dungeon_Gate";
        public const string ExitPortal = "Models/Exit_Portal";

        /// <summary>깨진 Kenney 프리팹 대신 프로시저럴을 쓰려면 true.</summary>
        public static bool PreferProcedural = true;

        public static bool Has(string resourcePath)
        {
            if (PreferProcedural)
            {
                return false;
            }

            return Resources.Load<GameObject>(resourcePath) != null;
        }

        public static GameObject TrySpawn(string resourcePath, Vector3 worldPos, Transform parent = null,
            float uniformScale = 1f)
        {
            if (PreferProcedural)
            {
                return null;
            }

            var prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null)
            {
                return null;
            }

            var go = Object.Instantiate(prefab, worldPos, Quaternion.identity, parent);
            go.name = prefab.name;
            if (Mathf.Abs(uniformScale - 1f) > 0.001f)
            {
                go.transform.localScale = Vector3.one * uniformScale;
            }

            StripCamerasAndLights(go);
            SanitizeVisual(go, resourcePath);
            return go;
        }

        public static GameObject TrySpawnLocal(string resourcePath, Transform parent, Vector3 localPos,
            float uniformScale = 1f)
        {
            var go = TrySpawn(resourcePath, Vector3.zero, parent, uniformScale);
            if (go == null)
            {
                return null;
            }

            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.identity;
            return go;
        }

        private static void SanitizeVisual(GameObject root, string resourcePath)
        {
            FitHeight(root, TargetHeight(resourcePath));
            Recolor(root, TintFor(resourcePath));
        }

        private static float TargetHeight(string path)
        {
            if (path.Contains("Player") || path.Contains("Npc"))
            {
                return 1.85f;
            }

            if (path.Contains("Monster"))
            {
                return 1.4f;
            }

            if (path.Contains("Town_Tree"))
            {
                return 4f;
            }

            if (path.Contains("Town_House") || path.Contains("Dungeon_Gate"))
            {
                return 3.2f;
            }

            if (path.Contains("Town_Fountain"))
            {
                return 1.6f;
            }

            if (path.Contains("Town_Stall"))
            {
                return 2f;
            }

            if (path.Contains("Pillar") || path.Contains("Altar"))
            {
                return 2.8f;
            }

            if (path.Contains("Door") || path.Contains("Portal") || path.Contains("Gate"))
            {
                return 2.5f;
            }

            if (path.Contains("Chest") || path.Contains("Torch"))
            {
                return 1.2f;
            }

            return 2f;
        }

        private static Color TintFor(string path)
        {
            if (path.Contains("Player"))
            {
                return MatLib.Metal;
            }

            if (path.Contains("Monster"))
            {
                return MatLib.Slime;
            }

            if (path.Contains("Npc"))
            {
                return MatLib.NpcRobe;
            }

            if (path.Contains("Tree"))
            {
                return MatLib.Leaf;
            }

            if (path.Contains("House") || path.Contains("Stall") || path.Contains("Fountain"))
            {
                return MatLib.Stone;
            }

            if (path.Contains("Torch"))
            {
                return MatLib.AccentCopper;
            }

            if (path.Contains("Chest"))
            {
                return MatLib.NpcTrim;
            }

            return MatLib.StoneDark;
        }

        private static void FitHeight(GameObject root, float targetHeight)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return;
            }

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            var height = Mathf.Max(0.01f, bounds.size.y);
            // 비정상적으로 크거나 작으면 강제 맞춤
            if (height > targetHeight * 1.35f || height < targetHeight * 0.45f ||
                bounds.size.x > 12f || bounds.size.z > 12f)
            {
                root.transform.localScale *= targetHeight / height;
            }

            renderers = root.GetComponentsInChildren<Renderer>();
            bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            var delta = new Vector3(bounds.center.x - root.transform.position.x, bounds.min.y - root.transform.position.y,
                bounds.center.z - root.transform.position.z);
            root.transform.position -= delta;
        }

        private static void Recolor(GameObject root, Color tint)
        {
            foreach (var r in root.GetComponentsInChildren<Renderer>())
            {
                var mats = r.sharedMaterials;
                if (mats == null || mats.Length == 0)
                {
                    r.sharedMaterial = MatLib.Get(tint);
                    continue;
                }

                var replaced = new Material[mats.Length];
                for (var i = 0; i < mats.Length; i++)
                {
                    var src = mats[i];
                    var c = tint;
                    if (src != null && src.shader != null && src.shader.name != "Hidden/InternalErrorShader")
                    {
                        if (src.HasProperty("_Color"))
                        {
                            c = Color.Lerp(src.color, tint, 0.35f);
                        }
                        else if (src.HasProperty("_BaseColor"))
                        {
                            c = Color.Lerp(src.GetColor("_BaseColor"), tint, 0.35f);
                        }
                    }

                    // 깨진/분홍 머티리얼은 MatLib로 교체
                    if (src == null || src.shader == null || src.shader.name.Contains("Error") ||
                        src.shader.name.Contains("InternalError"))
                    {
                        replaced[i] = MatLib.Get(tint);
                    }
                    else
                    {
                        // 셰이더는 살리고 색만 보정 — 실패 시 MatLib
                        try
                        {
                            var copy = new Material(src);
                            if (copy.HasProperty("_Color"))
                            {
                                copy.color = c;
                            }

                            if (copy.HasProperty("_BaseColor"))
                            {
                                copy.SetColor("_BaseColor", c);
                            }

                            replaced[i] = copy.shader == null || copy.shader.name.Contains("Error")
                                ? MatLib.Get(tint)
                                : copy;
                        }
                        catch
                        {
                            replaced[i] = MatLib.Get(tint);
                        }
                    }
                }

                r.sharedMaterials = replaced;
            }
        }

        private static void StripCamerasAndLights(GameObject root)
        {
            foreach (var cam in root.GetComponentsInChildren<Camera>(true))
            {
                Object.Destroy(cam.gameObject);
            }
        }
    }
}
