#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace DungeonOdyssey.EditorTools
{
    public class PixelArtImportSettings : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.Replace('\\', '/').Contains("/Resources/Art/"))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.spritePivot = new Vector2(0.5f, 0.5f);
        }

        [MenuItem("Dungeon Odyssey/2. Reimport Pixel Art")]
        public static void ReimportArt()
        {
            var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/Art" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Dungeon Odyssey", "픽셀 아트 재임포트 완료", "확인");
        }
    }
}
#endif
