using System.Collections.Generic;
using UnityEngine;

namespace DungeonOdyssey.Dungeon
{
    public class DungeonGenerator : MonoBehaviour
    {
        [SerializeField] private int gridWidth = 5;
        [SerializeField] private int gridHeight = 5;
        [SerializeField] private int roomCount = 8;
        [SerializeField] private int seed;

        public IReadOnlyList<DungeonRoomData> Rooms => _rooms;
        public DungeonRoomData StartRoom { get; private set; }
        public DungeonRoomData ExitRoom { get; private set; }
        public IReadOnlyDictionary<Vector2Int, DungeonRoomData> Map => _map;

        private readonly List<DungeonRoomData> _rooms = new();
        private readonly Dictionary<Vector2Int, DungeonRoomData> _map = new();

        private static readonly string[] Themes =
        {
            "잿빛 회랑", "침묵의 납골당", "흑요 예배당", "뼈의 성소", "공허 갱도", "심연의 문턱"
        };

        public void Generate(int overrideRoomCount = 0)
        {
            _rooms.Clear();
            _map.Clear();

            if (seed == 0)
            {
                seed = Random.Range(1, int.MaxValue);
            }

            Random.InitState(seed);

            var countGoal = overrideRoomCount > 0 ? overrideRoomCount : roomCount;
            var targetCount = Mathf.Clamp(countGoal, 5, gridWidth * gridHeight);
            var current = new Vector2Int(gridWidth / 2, gridHeight / 2);
            AddRoom(current, RoomType.Start);

            var directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            var safety = 0;
            while (_rooms.Count < targetCount && safety < 500)
            {
                safety++;
                var dir = directions[Random.Range(0, directions.Length)];
                var next = current + dir;
                if (next.x < 0 || next.y < 0 || next.x >= gridWidth || next.y >= gridHeight)
                {
                    continue;
                }

                if (!_map.ContainsKey(next))
                {
                    AddRoom(next, RoomType.Combat);
                }

                Connect(_map[current], _map[next]);
                current = next;
            }

            StartRoom = _rooms[0];
            ExitRoom = StartRoom;
            var maxDist = -1;
            foreach (var room in _rooms)
            {
                if (room == StartRoom)
                {
                    continue;
                }

                var dist = Mathf.Abs(room.GridPosition.x - StartRoom.GridPosition.x)
                           + Mathf.Abs(room.GridPosition.y - StartRoom.GridPosition.y);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    ExitRoom = room;
                }
            }

            ExitRoom.Type = RoomType.Exit;
            StartRoom.Cleared = true;

            // 출구 직전 경로에 보스방 (출구와 이웃한 전투방 우선)
            PlaceSpecialRoom(RoomType.Boss, preferNearExit: true);
            PlaceSpecialRoom(RoomType.Treasure, preferNearExit: false);
            PlaceSpecialRoom(RoomType.Event, preferNearExit: false);

            // 런 전체 테마 통일 — 층 분위기 일관성
            var theme = Themes[Mathf.Abs(seed) % Themes.Length];
            foreach (var room in _rooms)
            {
                room.ThemeName = theme;
                room.WorldCenter = Vector3.zero;
            }

            Debug.Log($"[Dungeon] seed={seed}, rooms={_rooms.Count}, theme={theme}");
        }

        private void PlaceSpecialRoom(RoomType type, bool preferNearExit)
        {
            DungeonRoomData best = null;
            var bestScore = -1;
            foreach (var room in _rooms)
            {
                if (room.Type != RoomType.Combat)
                {
                    continue;
                }

                var distExit = Mathf.Abs(room.GridPosition.x - ExitRoom.GridPosition.x)
                               + Mathf.Abs(room.GridPosition.y - ExitRoom.GridPosition.y);
                var distStart = Mathf.Abs(room.GridPosition.x - StartRoom.GridPosition.x)
                                + Mathf.Abs(room.GridPosition.y - StartRoom.GridPosition.y);
                var score = preferNearExit ? (20 - distExit) + distStart : distStart + Random.Range(0, 3);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = room;
                }
            }

            if (best != null)
            {
                best.Type = type;
            }
        }

        public DungeonRoomData GetNeighbor(DungeonRoomData room, DoorDir dir)
        {
            var offset = dir switch
            {
                DoorDir.North => Vector2Int.up,
                DoorDir.South => Vector2Int.down,
                DoorDir.East => Vector2Int.right,
                _ => Vector2Int.left
            };

            _map.TryGetValue(room.GridPosition + offset, out var neighbor);
            return neighbor;
        }

        private void AddRoom(Vector2Int pos, RoomType type)
        {
            var room = new DungeonRoomData
            {
                Id = _rooms.Count,
                GridPosition = pos,
                Type = type,
                Cleared = type == RoomType.Start
            };
            _rooms.Add(room);
            _map[pos] = room;
        }

        private static void Connect(DungeonRoomData a, DungeonRoomData b)
        {
            var delta = b.GridPosition - a.GridPosition;
            if (delta == Vector2Int.up)
            {
                a.ConnectedNorth = true;
                b.ConnectedSouth = true;
            }
            else if (delta == Vector2Int.down)
            {
                a.ConnectedSouth = true;
                b.ConnectedNorth = true;
            }
            else if (delta == Vector2Int.right)
            {
                a.ConnectedEast = true;
                b.ConnectedWest = true;
            }
            else if (delta == Vector2Int.left)
            {
                a.ConnectedWest = true;
                b.ConnectedEast = true;
            }
        }
    }
}
