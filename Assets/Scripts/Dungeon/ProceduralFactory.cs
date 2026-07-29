using DungeonOdyssey.View3D;
using UnityEngine;

namespace DungeonOdyssey.Dungeon
{
    /// <summary>
    /// 액터/프롭 생성 — 3D 팩토리로 위임.
    /// </summary>
    public static class ProceduralFactory
    {
        public static GameObject CreatePlayer(Vector3 position) => ActorFactory3D.CreatePlayer(position);

        public static GameObject CreateSlime(Vector3 position, Transform parent) =>
            ActorFactory3D.CreateSlime(position, parent);

        public static GameObject CreateNpc(Vector3 position, Transform parent = null) =>
            ActorFactory3D.CreateNpc(position, parent);

        public static GameObject CreateDungeonDoor(Vector3 position, Transform parent = null) =>
            EnvironmentFactory3D.CreateDungeonGate(position, parent);

        public static GameObject CreatePortal(Vector3 position, Transform parent) =>
            EnvironmentFactory3D.CreateExitPortal(position, parent);
    }
}
