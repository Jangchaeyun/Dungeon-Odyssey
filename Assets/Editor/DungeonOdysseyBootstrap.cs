#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using DungeonOdyssey.Core;
using DungeonOdyssey.Dungeon;
using DungeonOdyssey.Town;

namespace DungeonOdyssey.EditorTools
{
    public static class DungeonOdysseyBootstrap
    {
        private const string ScenesFolder = "Assets/Scenes";

        [MenuItem("Dungeon Odyssey/1. Setup Project (Scenes + Build Settings)")]
        public static void SetupProject()
        {
            EnsureFolders();
            EnsureTag("Player");
            EnsureLayer("Enemy");

            var title = CreateScene("Title", SetupTitleScene);
            var town = CreateScene("Town", SetupTownScene);
            var dungeon = CreateScene("Dungeon", SetupDungeonScene);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(title, true),
                new EditorBuildSettingsScene(town, true),
                new EditorBuildSettingsScene(dungeon, true)
            };

            EditorSceneManager.OpenScene(title);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Dungeon Odyssey",
                "프로젝트 세팅이 완료되었습니다.\n\n" +
                "1) Title 씬이 열려 있습니다.\n" +
                "2) Play를 눌러 새 게임 → 마을 → 던전 흐름을 확인하세요.\n" +
                "3) 조작: WASD 이동, Space/J 공격, E 상호작용",
                "확인");
        }

        [MenuItem("Dungeon Odyssey/Open Title Scene")]
        public static void OpenTitle()
        {
            var path = $"{ScenesFolder}/Title.unity";
            if (File.Exists(path))
            {
                EditorSceneManager.OpenScene(path);
            }
            else
            {
                SetupProject();
            }
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Scripts"))
            {
                AssetDatabase.CreateFolder("Assets", "Scripts");
            }
        }

        private static string CreateScene(string name, System.Action setup)
        {
            var path = $"{ScenesFolder}/{name}.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.14f);
            cameraGo.tag = "MainCamera";
            cameraGo.AddComponent<AudioListener>();
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);

            setup?.Invoke();

            EditorSceneManager.SaveScene(scene, path);
            return path;
        }

        private static void SetupTitleScene()
        {
            var entry = new GameObject("TitleSceneEntry");
            entry.AddComponent<TitleSceneEntry>();
        }

        private static void SetupTownScene()
        {
            var town = new GameObject("TownController");
            town.AddComponent<TownController>();
        }

        private static void SetupDungeonScene()
        {
            var dungeon = new GameObject("DungeonManager");
            dungeon.AddComponent<DungeonGenerator>();
            dungeon.AddComponent<DungeonManager>();
        }

        private static void EnsureTag(string tag)
        {
            var asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (asset == null || asset.Length == 0)
            {
                return;
            }

            var so = new SerializedObject(asset[0]);
            var tags = so.FindProperty("tags");
            for (var i = 0; i < tags.arraySize; i++)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    return;
                }
            }

            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            so.ApplyModifiedProperties();
        }

        private static void EnsureLayer(string layerName)
        {
            var asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (asset == null || asset.Length == 0)
            {
                return;
            }

            var so = new SerializedObject(asset[0]);
            var layers = so.FindProperty("layers");
            for (var i = 8; i < layers.arraySize; i++)
            {
                var prop = layers.GetArrayElementAtIndex(i);
                if (prop.stringValue == layerName)
                {
                    return;
                }
            }

            for (var i = 8; i < layers.arraySize; i++)
            {
                var prop = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(prop.stringValue))
                {
                    prop.stringValue = layerName;
                    so.ApplyModifiedProperties();
                    return;
                }
            }
        }
    }
}
#endif
