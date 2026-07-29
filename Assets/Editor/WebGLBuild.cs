using System.IO;
using UnityEditor;
using UnityEngine;

namespace DungeonOdyssey.EditorTools
{
    /// <summary>배치/메뉴용 WebGL 빌드 — GitHub Pages 배포용.</summary>
    public static class WebGLBuild
    {
        private const string OutputDir = "Builds/WebGL";

        [MenuItem("Dungeon Odyssey/Build WebGL (GitHub Pages)")]
        public static void Build()
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes == null || scenes.Length == 0)
            {
                Debug.LogError("Build Settings에 씬이 없습니다. Setup Project를 먼저 실행하세요.");
                EditorApplication.Exit(1);
                return;
            }

            var enabled = new System.Collections.Generic.List<string>();
            foreach (var s in scenes)
            {
                if (s.enabled && !string.IsNullOrEmpty(s.path))
                {
                    enabled.Add(s.path);
                }
            }

            if (enabled.Count == 0)
            {
                Debug.LogError("활성화된 빌드 씬이 없습니다.");
                EditorApplication.Exit(1);
                return;
            }

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildPipeline.GetBuildTargetGroup(BuildTarget.WebGL), BuildTarget.WebGL))
            {
                Debug.LogError("WebGL 빌드 타깃으로 전환하지 못했습니다. Unity Hub에서 WebGL 모듈을 설치하세요.");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }

                return;
            }

            if (Directory.Exists(OutputDir))
            {
                Directory.Delete(OutputDir, true);
            }

            Directory.CreateDirectory(OutputDir);

            // GitHub Pages는 커스텀 Content-Encoding 헤더가 없어 fallback 필요
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;

            var opts = new BuildPlayerOptions
            {
                scenes = enabled.ToArray(),
                locationPathName = OutputDir,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(opts);
            var ok = report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded;
            if (ok)
            {
                File.WriteAllText(Path.Combine(OutputDir, ".nojekyll"), "");
                Debug.Log($"WebGL 빌드 성공 → {Path.GetFullPath(OutputDir)}");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            else
            {
                Debug.LogError($"WebGL 빌드 실패: {report.summary.result}");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }
    }
}
