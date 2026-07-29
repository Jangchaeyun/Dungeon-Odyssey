using UnityEngine;

namespace DungeonOdyssey.Art
{
    public static class TilePainter
    {
        /// <summary>
        /// 타일을 하나씩 찍지 않고, 하나의 스프라이트를 스케일해 바닥을 깔아 GC/드로우를 줄입니다.
        /// </summary>
        public static GameObject PaintFloor(Transform parent, Sprite tile, Vector2 center, Vector2 worldSize,
            int sortingOrder, string name, Color? tint = null)
        {
            if (tile == null)
            {
                return null;
            }

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(center.x, center.y, 0f);
            // PPU 16, 타일 16px => 기본 1월드유닛. 스케일로 크기 맞춤.
            go.transform.localScale = new Vector3(worldSize.x, worldSize.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = tile;
            sr.sortingOrder = sortingOrder;
            sr.color = tint ?? Color.white;
            sr.drawMode = SpriteDrawMode.Simple;
            return go;
        }

        public static void PaintRect(Transform parent, Sprite tileA, Sprite tileB, Vector2 center, Vector2Int sizeInTiles,
            float tileWorldSize, int sortingOrder, string namePrefix)
        {
            // 성능: 큰 영역은 단일 바닥으로 대체
            if (sizeInTiles.x * sizeInTiles.y >= 16)
            {
                PaintFloor(parent, tileA, center,
                    new Vector2(sizeInTiles.x * tileWorldSize, sizeInTiles.y * tileWorldSize),
                    sortingOrder, namePrefix);
                return;
            }

            if (tileA == null)
            {
                return;
            }

            var origin = new Vector3(
                center.x - (sizeInTiles.x - 1) * tileWorldSize * 0.5f,
                center.y - (sizeInTiles.y - 1) * tileWorldSize * 0.5f,
                0f);

            for (var y = 0; y < sizeInTiles.y; y++)
            for (var x = 0; x < sizeInTiles.x; x++)
            {
                var useB = tileB != null && ((x + y) % 3 == 0);
                var go = new GameObject($"{namePrefix}_{x}_{y}");
                go.transform.SetParent(parent, false);
                go.transform.position = origin + new Vector3(x * tileWorldSize, y * tileWorldSize, 0f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = useB ? tileB : tileA;
                sr.sortingOrder = sortingOrder;
            }
        }

        public static void PaintRect(Transform parent, Sprite tile, Vector2 center, Vector2Int sizeInTiles,
            float tileWorldSize, int sortingOrder, string namePrefix)
        {
            PaintRect(parent, tile, null, center, sizeInTiles, tileWorldSize, sortingOrder, namePrefix);
        }

        public static GameObject SpawnSprite(string name, Sprite sprite, Vector3 position, Transform parent,
            int sortingOrder, float scale = 1f)
        {
            var go = new GameObject(name);
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            go.transform.position = position;
            go.transform.localScale = Vector3.one * scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            return go;
        }
    }
}
