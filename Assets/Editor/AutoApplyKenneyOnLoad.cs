#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DungeonOdyssey.EditorTools
{
    /// <summary>
    /// Selected FBX가 있는데 Resources 프리팹이 없으면 자동 적용.
    /// </summary>
    [InitializeOnLoad]
    public static class AutoApplyKenneyOnLoad
    {
        private const string FlagKey = "DungeonOdyssey.KenneyAutoApplied";

        static AutoApplyKenneyOnLoad()
        {
            EditorApplication.delayCall += TryApplyOnce;
        }

        private static void TryApplyOnce()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (!System.IO.File.Exists("Assets/Art/Kenney/Selected/Player.fbx"))
            {
                return;
            }

            if (System.IO.File.Exists("Assets/Resources/Models/Player.prefab"))
            {
                return;
            }

            if (SessionState.GetBool(FlagKey, false))
            {
                return;
            }

            SessionState.SetBool(FlagKey, true);
            Debug.Log("[Kenney] Resources 프리팹이 없어 자동 적용합니다…");
            KenneyModelImportMenu.Apply();
        }
    }
}
#endif
