using UnityEngine;

namespace DungeonOdyssey.Dungeon
{
    public class DungeonRoomData
    {
        public int Id;
        public Vector2Int GridPosition;
        public RoomType Type;
        public bool ConnectedNorth;
        public bool ConnectedSouth;
        public bool ConnectedEast;
        public bool ConnectedWest;
        public Vector3 WorldCenter;
        public bool Cleared;
        public int MonsterCount;
        public string ThemeName;
    }
}
