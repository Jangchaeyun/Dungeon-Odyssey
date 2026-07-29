#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DungeonOdyssey.EditorTools
{
    /// <summary>
    /// Unity용 FBX(Kenney Selected)를 Resources/Models 프리팹으로 등록합니다.
    /// 메뉴: Dungeon Odyssey → 4. Apply Unity 3D Models
    /// </summary>
    public static class KenneyModelImportMenu
    {
        private const string SelectedFolder = "Assets/Art/Kenney/Selected";
        private const string OutFolder = "Assets/Resources/Models";

        private static readonly string[] DirectModels =
        {
            "Player", "Npc_Mira", "Monster_Slime",
            "Town_Stall", "Town_Tree", "Town_Fountain",
            "Dungeon_Pillar", "Dungeon_Torch", "Dungeon_Chest",
            "Dungeon_Altar", "Dungeon_Door", "Dungeon_Gate", "Exit_Portal"
        };

        [MenuItem("Dungeon Odyssey/4. Apply Free 3D Assets (Selected → Resources)")]
        public static void Apply()
        {
            if (!Directory.Exists(SelectedFolder))
            {
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("Dungeon Odyssey",
                        $"폴더가 없습니다:\n{SelectedFolder}", "OK");
                }

                return;
            }

            Directory.CreateDirectory(OutFolder);
            AssetDatabase.Refresh();

            var count = 0;
            foreach (var name in DirectModels)
            {
                var path = $"{SelectedFolder}/{name}.fbx";
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"[Kenney] missing {path}");
                    continue;
                }

                ConfigureModelImporter(path, name);
                if (BuildPrefab(path, name))
                {
                    count++;
                }
            }

            // Compose town house from modular walls + roof
            if (BuildTownHouse())
            {
                count++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Kenney] Applied {count} models to Resources/Models");
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Dungeon Odyssey",
                    count > 0
                        ? $"{count}개 3D 모델을 Resources/Models 에 적용했습니다.\nPlay 로 확인하세요."
                        : "적용된 모델이 없습니다. Selected FBX 를 확인하세요.",
                    "OK");
            }
        }

        /// <summary>Unity -batchmode -executeMethod 용 엔트리.</summary>
        public static void ApplyBatch()
        {
            Apply();
        }

        private static void ConfigureModelImporter(string path, string name)
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                return;
            }

            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.animationType = ModelImporterAnimationType.None;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            importer.SaveAndReimport();
        }

        private static bool BuildPrefab(string modelPath, string name)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null)
            {
                return false;
            }

            var instance = Object.Instantiate(model);
            instance.name = name;
            NormalizeRoot(instance, name);
            TintIfCharacter(instance, name);

            var outPath = $"{OutFolder}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(instance, outPath);
            Object.DestroyImmediate(instance);
            Debug.Log($"[Kenney] Prefab → {outPath}");
            return true;
        }

        private static bool BuildTownHouse()
        {
            var wallDoor = AssetDatabase.LoadAssetAtPath<GameObject>($"{SelectedFolder}/House_WallDoor.fbx");
            var wall = AssetDatabase.LoadAssetAtPath<GameObject>($"{SelectedFolder}/House_Wall.fbx");
            var roof = AssetDatabase.LoadAssetAtPath<GameObject>($"{SelectedFolder}/House_Roof.fbx");
            if (wallDoor == null || wall == null || roof == null)
            {
                return false;
            }

            var root = new GameObject("Town_House");
            PlacePart(wallDoor, root.transform, new Vector3(0f, 0f, 0.5f), Vector3.zero, 1.2f);
            PlacePart(wall, root.transform, new Vector3(0f, 0f, -0.5f), new Vector3(0f, 180f, 0f), 1.2f);
            PlacePart(wall, root.transform, new Vector3(0.55f, 0f, 0f), new Vector3(0f, 90f, 0f), 1.2f);
            PlacePart(wall, root.transform, new Vector3(-0.55f, 0f, 0f), new Vector3(0f, -90f, 0f), 1.2f);
            PlacePart(roof, root.transform, new Vector3(0f, 1.15f, 0f), Vector3.zero, 1.35f);

            NormalizeRoot(root, "Town_House");
            var outPath = $"{OutFolder}/Town_House.prefab";
            PrefabUtility.SaveAsPrefabAsset(root, outPath);
            Object.DestroyImmediate(root);
            Debug.Log($"[Kenney] Prefab → {outPath}");
            return true;
        }

        private static void PlacePart(GameObject prefab, Transform parent, Vector3 localPos, Vector3 euler,
            float scale)
        {
            var go = Object.Instantiate(prefab, parent);
            go.name = prefab.name;
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.Euler(euler);
            go.transform.localScale = Vector3.one * scale;
        }

        private static void NormalizeRoot(GameObject root, string name)
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
            var targetHeight = name switch
            {
                "Monster_Slime" => 1.35f,
                "Player" or "Npc_Mira" => 1.85f,
                "Town_Tree" => 4.2f,
                "Town_House" => 3.6f,
                "Dungeon_Gate" => 3.8f,
                "Dungeon_Pillar" => 3.2f,
                "Dungeon_Door" or "Exit_Portal" => 2.6f,
                "Town_Stall" => 2.2f,
                "Town_Fountain" => 1.8f,
                "Dungeon_Torch" => 1.5f,
                "Dungeon_Chest" => 0.9f,
                "Dungeon_Altar" => 2.4f,
                _ => height
            };

            if (!Mathf.Approximately(targetHeight, height))
            {
                root.transform.localScale *= targetHeight / height;
            }

            renderers = root.GetComponentsInChildren<Renderer>();
            bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            root.transform.position -= new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        }

        private static void TintIfCharacter(GameObject root, string name)
        {
            // Kenney 원색을 살짝 눌러 다크 판타지 톤에 맞춤
            Color? mul = name switch
            {
                "Player" => new Color(0.55f, 0.58f, 0.65f, 1f),
                "Npc_Mira" => new Color(0.55f, 0.5f, 0.7f, 1f),
                "Monster_Slime" => new Color(0.45f, 0.28f, 0.5f, 1f),
                _ => null
            };

            if (mul == null)
            {
                return;
            }

            foreach (var r in root.GetComponentsInChildren<Renderer>())
            {
                var mats = r.sharedMaterials;
                for (var i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == null)
                    {
                        continue;
                    }

                    var mat = new Material(mats[i]);
                    if (mat.HasProperty("_Color"))
                    {
                        mat.color *= mul.Value;
                    }

                    if (mat.HasProperty("_BaseColor"))
                    {
                        mat.SetColor("_BaseColor", mat.GetColor("_BaseColor") * mul.Value);
                    }

                    mats[i] = mat;
                }

                r.sharedMaterials = mats;
            }
        }
    }
}
#endif
