using UnityEngine;

namespace DungeonOdyssey.View3D
{
    public static class Prim
    {
        public static GameObject Make(PrimitiveType type, string name, Transform parent, Vector3 localPos,
            Vector3 scale, Color color, float smooth = 0.35f)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.transform.localRotation = Quaternion.identity;

            var col = go.GetComponent<Collider>();
            if (col != null)
            {
                Object.Destroy(col);
            }

            go.GetComponent<MeshRenderer>().sharedMaterial = MatLib.Get(color, smooth);
            return go;
        }

        public static GameObject Cap(string name, Transform parent, Vector3 pos, Vector3 scale, Color color,
            float smooth = 0.35f) =>
            Make(PrimitiveType.Capsule, name, parent, pos, scale, color, smooth);

        public static GameObject Sphere(string name, Transform parent, Vector3 pos, float diameter, Color color,
            float smooth = 0.35f) =>
            Make(PrimitiveType.Sphere, name, parent, pos, Vector3.one * diameter, color, smooth);

        public static GameObject Cube(string name, Transform parent, Vector3 pos, Vector3 scale, Color color,
            float smooth = 0.35f) =>
            Make(PrimitiveType.Cube, name, parent, pos, scale, color, smooth);

        public static GameObject Cyl(string name, Transform parent, Vector3 pos, Vector3 scale, Color color,
            float smooth = 0.35f) =>
            Make(PrimitiveType.Cylinder, name, parent, pos, scale, color, smooth);

        public static Transform Bone(string name, Transform parent, Vector3 localPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.identity;
            return go.transform;
        }
    }
}
