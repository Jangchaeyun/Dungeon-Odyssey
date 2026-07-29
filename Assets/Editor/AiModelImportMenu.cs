#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DungeonOdyssey.EditorTools
{
    /// <summary>
    /// Assets/Art/AI_Raw 아래 FBX/GLB/프리팹을 Resources/Models 프리팹으로 복사·등록합니다.
    /// 파일명 규칙 (확장자 제외):
    ///   Player, Monster_Slime, Npc_Mira, Town_House, Town_Tree, Town_Stall,
    ///   Dungeon_Pillar, Dungeon_Torch, Dungeon_Chest, Dungeon_Altar, Dungeon_Door, Dungeon_Gate, Exit_Portal
    /// </summary>
    public static class AiModelImportMenu
    {
        private const string RawFolder = "Assets/Art/AI_Raw";
        private const string OutFolder = "Assets/Resources/Models";

        [MenuItem("Dungeon Odyssey/3. Apply AI Models (AI_Raw → Resources)")]
        public static void ApplyAiModels()
        {
            Directory.CreateDirectory(RawFolder);
            Directory.CreateDirectory(OutFolder);
            AssetDatabase.Refresh();

            var count = 0;
            var guids = AssetDatabase.FindAssets("t:GameObject t:Model", new[] { RawFolder });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (model == null)
                {
                    continue;
                }

                var baseName = Path.GetFileNameWithoutExtension(path);
                // Npc_Mira 등 → 그대로 Resources 키로 사용
                var outPath = $"{OutFolder}/{baseName}.prefab";

                var instance = Object.Instantiate(model);
                instance.name = baseName;
                NormalizeRoot(instance);

                PrefabUtility.SaveAsPrefabAsset(instance, outPath);
                Object.DestroyImmediate(instance);
                count++;
                Debug.Log($"[AI Models] Prefab → {outPath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Dungeon Odyssey",
                count > 0
                    ? $"{count}개 모델을 Resources/Models 에 적용했습니다.\nPlay 모드에서 확인하세요."
                    : $"적용할 모델이 없습니다.\n{RawFolder} 에 FBX/Prefab 을 넣고 다시 실행하세요.\n\n" +
                      "또는 Tools/meshy_generate.py 로 AI 생성 후 이 메뉴를 누르세요.",
                "OK");
        }

        [MenuItem("Dungeon Odyssey/Open AI_Raw Folder")]
        public static void OpenRawFolder()
        {
            Directory.CreateDirectory(RawFolder);
            EditorUtility.RevealInFinder(RawFolder);
        }

        private static void NormalizeRoot(GameObject root)
        {
            // 렌더러 기준으로 바닥(y=0)에 맞추고 키를 ~1.8 로 스케일
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
            var targetHeight = root.name.StartsWith("Monster") ? 1.2f
                : root.name.StartsWith("Town_") || root.name.StartsWith("Dungeon_") ? height
                : 1.8f;

            if (root.name.StartsWith("Town_") || root.name.StartsWith("Dungeon_") || root.name.StartsWith("Exit"))
            {
                // 환경 프롭은 스케일만 살짝 정리
                return;
            }

            var scale = targetHeight / height;
            root.transform.localScale *= scale;

            // 재계산 후 바닥 정렬
            renderers = root.GetComponentsInChildren<Renderer>();
            bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            root.transform.position -= new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        }
    }
}
#endif
