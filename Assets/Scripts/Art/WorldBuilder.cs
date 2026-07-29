using DungeonOdyssey.Dungeon;
using UnityEngine;

namespace DungeonOdyssey.Art
{
    public static class WorldBuilder
    {
        public static void BuildTown(Transform root)
        {
            ArtCatalog.EnsureLoaded();

            // 레이어드 지형 — 세이지 잔디 + 따뜻한 로드 + 쿨 톤 광장
            TilePainter.PaintFloor(root, ArtCatalog.TileGrass, Vector2.zero, new Vector2(36f, 24f), -22, "Grass",
                StylePalette.TownGrass);
            TilePainter.PaintFloor(root, ArtCatalog.TilePath, new Vector2(2.5f, 0f), new Vector2(22f, 2.6f), -16, "Road",
                StylePalette.TownPath);
            TilePainter.PaintFloor(root, ArtCatalog.TileStone, new Vector2(-2.5f, 0.4f), new Vector2(11f, 8f), -15, "Plaza",
                StylePalette.TownPlaza);

            // 광장 포인트 데칼
            Spawn(root, "PlazaDecal", ArtCatalog.PropPlazaDecal, new Vector3(-2.5f, 0.3f, 0f), -12, 1.4f);

            // 좌측 주거/카페 존
            Spawn(root, "House", ArtCatalog.House, new Vector3(-10f, 3.6f, 0f), -5, 1.05f);
            Spawn(root, "Banner", ArtCatalog.PropBanner, new Vector3(-7.2f, 2.8f, 0f), 4, 1.1f);
            Spawn(root, "Stall", ArtCatalog.PropStall, new Vector3(-6.2f, -0.2f, 0f), 3, 1.15f);
            Spawn(root, "Bench", ArtCatalog.PropBench, new Vector3(-4.8f, -2.2f, 0f), 3, 1.1f);
            Spawn(root, "Well", ArtCatalog.PropWell, new Vector3(-1.2f, -1.6f, 0f), -2, 1.05f);

            // 가로등 / 랜턴 리듬
            Spawn(root, "LampA", ArtCatalog.PropLamp, new Vector3(-8.5f, 1.2f, 0f), 5, 1.1f);
            Spawn(root, "LampB", ArtCatalog.PropLamp, new Vector3(1.5f, 2.4f, 0f), 5, 1.1f);
            Spawn(root, "LampC", ArtCatalog.PropLamp, new Vector3(8.5f, -1.5f, 0f), 5, 1.1f);
            Spawn(root, "Lantern", ArtCatalog.PropLantern, new Vector3(-3.5f, 2.6f, 0f), 6, 1.2f);

            // 시장 박스로 길 가장자리 정리
            Spawn(root, "CrateA", ArtCatalog.PropCrate, new Vector3(5.8f, -1.5f, 0f), 3);
            Spawn(root, "BarrelA", ArtCatalog.PropBarrel, new Vector3(6.6f, -1.4f, 0f), 3);
            Spawn(root, "StallB", ArtCatalog.PropStall, new Vector3(4.2f, 2.2f, 0f), 3, 1.05f);
            Spawn(root, "FlowerA", ArtCatalog.PropFlower, new Vector3(-5.5f, 1.8f, 0f), 4, 1.2f);
            Spawn(root, "FlowerB", ArtCatalog.PropFlower, new Vector3(0.8f, -2.4f, 0f), 4, 1.15f);

            // 수목 프레임 (비대칭 배치)
            Spawn(root, "TreeA", ArtCatalog.Tree, new Vector3(-13f, -2.2f, 0f), -4, 1.05f);
            Spawn(root, "TreeB", ArtCatalog.Tree, new Vector3(-9.5f, 5.6f, 0f), -4, 0.95f);
            Spawn(root, "TreeC", ArtCatalog.Tree, new Vector3(3.5f, 5.2f, 0f), -4, 1.1f);
            Spawn(root, "TreeD", ArtCatalog.Tree, new Vector3(12.5f, 2.8f, 0f), -4, 1f);
            Spawn(root, "TreeE", ArtCatalog.Tree, new Vector3(11f, -3.6f, 0f), -4, 0.9f);

            // 짧은 펜스로 존 구분
            for (var x = -6; x <= 0; x += 2)
            {
                Spawn(root, "FenceN", ArtCatalog.PropFence, new Vector3(x, 4.2f, 0f), -6, 1.05f);
            }

            // 마을 타이틀 플래그
            WorldLabel.Attach(root, "HAVEN · 모험가의 쉼터", new Vector3(-2.5f, 5.4f, 0f),
                new Color(0.15f, 0.25f, 0.28f, 0.9f), 0.12f);
        }

        public static Transform BuildDungeonRoom(Transform parent, DungeonRoomData room, bool doorsLocked)
        {
            ArtCatalog.EnsureLoaded();

            var root = new GameObject($"Room_{room.Id}_{room.Type}").transform;
            root.SetParent(parent, false);
            root.position = Vector3.zero;

            var floorTint = room.Type switch
            {
                RoomType.Start => StylePalette.DungeonStart,
                RoomType.Exit => StylePalette.DungeonExit,
                RoomType.Treasure => StylePalette.DungeonTreasure,
                _ => StylePalette.DungeonCombat
            };

            // 이중 바닥: 외곽 프레임 + 이너 코트
            TilePainter.PaintFloor(root, ArtCatalog.TileDungeon, Vector2.zero, new Vector2(15f, 11f), -12, "FloorOuter",
                new Color(floorTint.r * 0.75f, floorTint.g * 0.75f, floorTint.b * 0.8f, 1f));
            TilePainter.PaintFloor(root, ArtCatalog.TileDungeon, Vector2.zero, new Vector2(12.5f, 8.5f), -11, "FloorInner",
                floorTint);
            Spawn(root, "Accent", ArtCatalog.PropFloorAccent, Vector3.zero, -9, 2.2f);

            // 벽 — 리듬감 있게
            for (var x = -6; x <= 6; x += 2)
            {
                var wallN = Spawn(root, "WallN", ArtCatalog.WallDungeon, new Vector3(x, 4.55f, 0f), -4, 1.05f);
                var wallS = Spawn(root, "WallS", ArtCatalog.WallDungeon, new Vector3(x, -4.55f, 0f), -4, 1.05f);
                Tint(wallN, new Color(0.75f, 0.78f, 0.85f));
                Tint(wallS, new Color(0.55f, 0.58f, 0.65f));
            }

            for (var y = -3; y <= 3; y += 2)
            {
                Spawn(root, "WallW", ArtCatalog.WallSide, new Vector3(-7f, y, 0f), -5, 1.35f);
                Spawn(root, "WallE", ArtCatalog.WallSide, new Vector3(7f, y, 0f), -5, 1.35f);
            }

            // 분위기 안개
            var mistA = Spawn(root, "MistA", ArtCatalog.PropMist, new Vector3(0f, -3.2f, 0f), 8, 2.4f);
            var mistB = Spawn(root, "MistB", ArtCatalog.PropMist, new Vector3(0f, 3.4f, 0f), 8, 2.2f);
            Tint(mistA, StylePalette.DungeonMist);
            Tint(mistB, StylePalette.DungeonMist);

            // 공용 구조물
            Spawn(root, "PillarL", ArtCatalog.PropPillar, new Vector3(-4.8f, 2.2f, 0f), 2, 1.1f);
            Spawn(root, "PillarR", ArtCatalog.PropPillar, new Vector3(4.8f, 2.2f, 0f), 2, 1.1f);
            Spawn(root, "TorchL", ArtCatalog.Torch, new Vector3(-5.5f, 2.9f, 0f), 7, 1.15f);
            Spawn(root, "TorchR", ArtCatalog.Torch, new Vector3(5.5f, 2.9f, 0f), 7, 1.15f);
            Spawn(root, "RuneL", ArtCatalog.PropRune, new Vector3(-5.2f, -2.6f, 0f), 3, 1.1f);
            Spawn(root, "RuneR", ArtCatalog.PropRune, new Vector3(5.2f, -2.6f, 0f), 3, 1.1f);

            switch (room.Type)
            {
                case RoomType.Start:
                    Spawn(root, "Carpet", ArtCatalog.PropCarpet, new Vector3(0f, -0.6f, 0f), -8, 1.15f);
                    Spawn(root, "Books", ArtCatalog.PropBookshelf, new Vector3(-3.4f, -2.2f, 0f), 4, 1.15f);
                    Spawn(root, "Crate", ArtCatalog.PropCrate, new Vector3(3.2f, -2.4f, 0f), 3);
                    Spawn(root, "Banner", ArtCatalog.PropBanner, new Vector3(0f, 2.8f, 0f), 5, 1.2f);
                    break;
                case RoomType.Exit:
                    Spawn(root, "Altar", ArtCatalog.PropAltar, new Vector3(0f, 1.4f, 0f), 5, 1.25f);
                    Spawn(root, "Carpet", ArtCatalog.PropCarpet, new Vector3(0f, -0.2f, 0f), -8, 1.2f);
                    Spawn(root, "Pillar", ArtCatalog.PropPillar, new Vector3(-2.2f, -1.2f, 0f), 2);
                    Spawn(root, "Pillar", ArtCatalog.PropPillar, new Vector3(2.2f, -1.2f, 0f), 2);
                    break;
                case RoomType.Treasure:
                    Spawn(root, "Chest", ArtCatalog.PropChest, new Vector3(0f, 1.1f, 0f), 6, 1.25f);
                    Spawn(root, "Rug", ArtCatalog.PropRug, new Vector3(0f, -1.2f, 0f), -8, 1.3f);
                    Spawn(root, "Crate", ArtCatalog.PropCrate, new Vector3(-2.8f, 0.8f, 0f), 4);
                    Spawn(root, "Barrel", ArtCatalog.PropBarrel, new Vector3(2.8f, 0.8f, 0f), 4);
                    Spawn(root, "Lantern", ArtCatalog.PropLantern, new Vector3(0f, 2.6f, 0f), 7, 1.3f);
                    break;
                default:
                    Spawn(root, "Bones", ArtCatalog.PropBones, new Vector3(-2.8f, -1.6f, 0f), 3, 1.15f);
                    Spawn(root, "Crate", ArtCatalog.PropCrate, new Vector3(3.4f, -2f, 0f), 3);
                    Spawn(root, "Barrel", ArtCatalog.PropBarrel, new Vector3(-3.6f, 1.6f, 0f), 3);
                    break;
            }

            if (room.ConnectedNorth)
            {
                CreateDoor(root, DoorDir.North, new Vector3(0f, 3.7f, 0f), doorsLocked && !room.Cleared);
            }

            if (room.ConnectedSouth)
            {
                CreateDoor(root, DoorDir.South, new Vector3(0f, -3.7f, 0f), doorsLocked && !room.Cleared);
            }

            if (room.ConnectedEast)
            {
                CreateDoor(root, DoorDir.East, new Vector3(6.1f, 0f, 0f), doorsLocked && !room.Cleared);
            }

            if (room.ConnectedWest)
            {
                CreateDoor(root, DoorDir.West, new Vector3(-6.1f, 0f, 0f), doorsLocked && !room.Cleared);
            }

            return root;
        }

        private static void CreateDoor(Transform root, DoorDir dir, Vector3 pos, bool locked)
        {
            var doorGo = Spawn(root, $"Door_{dir}", ArtCatalog.DungeonDoor, pos, 12, 0.9f);
            if (doorGo == null)
            {
                return;
            }

            if (dir == DoorDir.East)
            {
                doorGo.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
            }
            else if (dir == DoorDir.West)
            {
                doorGo.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            }
            else if (dir == DoorDir.South)
            {
                doorGo.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
            }

            Tint(doorGo, locked ? new Color(0.55f, 0.45f, 0.5f) : StylePalette.DungeonAccent);

            var door = doorGo.AddComponent<RoomDoor>();
            door.Setup(dir, locked);
            var col = doorGo.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.85f;

            var label = locked ? $"{DirLabel(dir)} · LOCKED" : $"{DirLabel(dir)} · ENTER [E]";
            WorldLabel.Attach(doorGo.transform, label, new Vector3(0f, 1.55f, 0f),
                locked ? new Color(1f, 0.55f, 0.5f) : StylePalette.DungeonAccent, 0.078f);
        }

        private static string DirLabel(DoorDir dir) => dir switch
        {
            DoorDir.North => "N",
            DoorDir.South => "S",
            DoorDir.East => "E",
            _ => "W"
        };

        private static void Tint(GameObject go, Color color)
        {
            if (go == null)
            {
                return;
            }

            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = color;
            }
        }

        private static GameObject Spawn(Transform parent, string name, Sprite sprite, Vector3 localPos, int order,
            float scale = 1f)
        {
            if (sprite == null)
            {
                return null;
            }

            var go = TilePainter.SpawnSprite(name, sprite, parent.position + localPos, parent, order, scale);
            go.transform.localPosition = localPos;
            return go;
        }
    }
}
