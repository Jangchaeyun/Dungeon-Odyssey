using UnityEngine;

namespace DungeonOdyssey.Art
{
    public class ShadowFollower : MonoBehaviour
    {
        public static void Attach(Transform owner, float scale = 1f)
        {
            ArtCatalog.EnsureLoaded();
            if (ArtCatalog.Shadow == null || owner == null)
            {
                return;
            }

            var go = new GameObject("Shadow");
            go.transform.SetParent(owner, false);
            go.transform.localPosition = new Vector3(0f, -0.55f, 0f);
            go.transform.localScale = Vector3.one * scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ArtCatalog.Shadow;
            sr.sortingOrder = 1;
            sr.color = new Color(0f, 0f, 0f, 0.45f);
        }
    }
}
