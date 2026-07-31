using UnityEngine;

namespace DungeonOdyssey.View3D
{
    /// <summary>
    /// 타이틀용 풀블리드 배경 — 밝은 판타지 성문·잔디·횃불·느린 카메라 드리프트.
    /// </summary>
    public class TitleAtmosphere3D : MonoBehaviour
    {
        private Transform _rig;
        private Light _torchL;
        private Light _torchR;
        private Light _key;
        private float _t;
        private Vector3 _camBase;
        private Transform[] _embers;

        public static TitleAtmosphere3D Spawn()
        {
            var host = new GameObject("TitleAtmosphere");
            var atm = host.AddComponent<TitleAtmosphere3D>();
            atm.Build();
            return atm;
        }

        private void Build()
        {
            _rig = transform;

            // 잔디 · 돌길
            Prim.Cube("Ground", _rig, new Vector3(0f, -0.5f, 6f), new Vector3(48f, 1f, 40f),
                MatLib.Grass, 0.12f);
            Prim.Cube("Path", _rig, new Vector3(0f, 0.02f, 4f), new Vector3(5.2f, 0.06f, 20f),
                MatLib.Path, 0.15f);
            Prim.Cube("PathEdgeL", _rig, new Vector3(-2.8f, 0.04f, 4f), new Vector3(0.35f, 0.05f, 20f),
                MatLib.Stone, 0.18f);
            Prim.Cube("PathEdgeR", _rig, new Vector3(2.8f, 0.04f, 4f), new Vector3(0.35f, 0.05f, 20f),
                MatLib.Stone, 0.18f);

            // 중앙 성문 (쌍탑 + 빨간 깃발)
            BuildCastleGate();

            // 양옆 집·나무 실루엣
            BuildHouseSilhouette(new Vector3(-9.5f, 0f, 10f), MatLib.Roof, 1f);
            BuildHouseSilhouette(new Vector3(9.5f, 0f, 10f), MatLib.RoofRed, 0.9f);
            BuildHouseSilhouette(new Vector3(-11f, 0f, 5f), MatLib.RoofRed, 0.75f);
            BuildHouseSilhouette(new Vector3(11f, 0f, 5.5f), MatLib.Roof, 0.8f);

            BuildPine(new Vector3(-7f, 0f, 3f), 1.1f);
            BuildPine(new Vector3(7.2f, 0f, 3.2f), 1f);
            BuildPine(new Vector3(-12f, 0f, 12f), 1.2f);
            BuildPine(new Vector3(12f, 0f, 11.5f), 1.15f);

            Prim.Cube("FenceL", _rig, new Vector3(-5.5f, 0.45f, 1f), new Vector3(0.12f, 0.9f, 4f),
                MatLib.Wood, 0.2f);
            Prim.Cube("FenceR", _rig, new Vector3(5.5f, 0.45f, 1f), new Vector3(0.12f, 0.9f, 4f),
                MatLib.Wood, 0.2f);

            _torchL = MakeTorch(new Vector3(-4.2f, 2.8f, 12f), new Color(1f, 0.62f, 0.3f));
            _torchR = MakeTorch(new Vector3(4.2f, 2.8f, 12f), new Color(1f, 0.58f, 0.28f));

            var keyGo = new GameObject("KeyLight");
            keyGo.transform.SetParent(_rig, false);
            keyGo.transform.position = new Vector3(-5f, 11f, 1f);
            _key = keyGo.AddComponent<Light>();
            _key.type = LightType.Directional;
            _key.transform.rotation = Quaternion.Euler(32f, 38f, 0f);
            _key.color = new Color(1f, 0.9f, 0.75f);
            _key.intensity = 1.55f;
            _key.shadows = LightShadows.Soft;

            var fillGo = new GameObject("FillLight");
            fillGo.transform.SetParent(_rig, false);
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.transform.rotation = Quaternion.Euler(48f, -125f, 0f);
            fill.color = new Color(0.48f, 0.6f, 0.8f);
            fill.intensity = 0.48f;

            SpawnEmbers(28);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.018f;
            RenderSettings.fogColor = MatLib.Fog;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.4f, 0.42f, 0.48f);

            SetupCamera();
        }

        private void BuildCastleGate()
        {
            Prim.Cube("TowerL", _rig, new Vector3(-4.2f, 3.6f, 16f), new Vector3(2.4f, 7.2f, 2.4f),
                MatLib.Stone, 0.18f);
            Prim.Cube("TowerR", _rig, new Vector3(4.2f, 3.6f, 16f), new Vector3(2.4f, 7.2f, 2.4f),
                MatLib.Stone, 0.18f);
            Prim.Cube("CapL", _rig, new Vector3(-4.2f, 7.4f, 16f), new Vector3(2.8f, 0.4f, 2.8f),
                MatLib.StoneDark, 0.2f);
            Prim.Cube("CapR", _rig, new Vector3(4.2f, 7.4f, 16f), new Vector3(2.8f, 0.4f, 2.8f),
                MatLib.StoneDark, 0.2f);
            Prim.Cube("WinL", _rig, new Vector3(-4.2f, 5.2f, 17.15f), new Vector3(0.7f, 0.85f, 0.12f),
                new Color(1f, 0.82f, 0.4f), 0.4f);
            Prim.Cube("WinR", _rig, new Vector3(4.2f, 5.2f, 17.15f), new Vector3(0.7f, 0.85f, 0.12f),
                new Color(1f, 0.82f, 0.4f), 0.4f);

            Prim.Cube("ArchL", _rig, new Vector3(-2f, 3.2f, 16f), new Vector3(1.4f, 6.4f, 1.6f),
                MatLib.StoneDark, 0.18f);
            Prim.Cube("ArchR", _rig, new Vector3(2f, 3.2f, 16f), new Vector3(1.4f, 6.4f, 1.6f),
                MatLib.StoneDark, 0.18f);
            Prim.Cube("ArchTop", _rig, new Vector3(0f, 6.6f, 16f), new Vector3(5.6f, 1.2f, 1.8f),
                MatLib.Stone, 0.2f);
            Prim.Cube("GateDoor", _rig, new Vector3(0f, 2.4f, 15.6f), new Vector3(3.2f, 4.6f, 0.35f),
                MatLib.Wood, 0.22f);
            Prim.Cube("GateBand", _rig, new Vector3(0f, 2.4f, 15.8f), new Vector3(3.3f, 0.18f, 0.12f),
                MatLib.MetalDark, 0.45f);

            Prim.Cube("BannerL", _rig, new Vector3(-4.2f, 4.2f, 17.3f), new Vector3(1.1f, 2.4f, 0.08f),
                MatLib.AccentWine, 0.2f);
            Prim.Cube("BannerR", _rig, new Vector3(4.2f, 4.2f, 17.3f), new Vector3(1.1f, 2.4f, 0.08f),
                MatLib.AccentWine, 0.2f);

            Prim.Cube("FarHill", _rig, new Vector3(0f, 2.5f, 24f), new Vector3(40f, 8f, 2f),
                Color.Lerp(MatLib.GrassDark, MatLib.Fog, 0.35f), 0.1f);
        }

        private void BuildHouseSilhouette(Vector3 pos, Color roof, float scale)
        {
            var house = new GameObject("HouseSil");
            house.transform.SetParent(_rig, false);
            house.transform.position = pos;
            house.transform.localScale = Vector3.one * scale;
            Prim.Cube("Body", house.transform, new Vector3(0f, 1.4f, 0f), new Vector3(3.2f, 2.8f, 2.6f),
                Color.Lerp(MatLib.Stone, new Color(0.72f, 0.64f, 0.52f), 0.4f), 0.18f);
            Prim.Cube("Roof", house.transform, new Vector3(0f, 3.1f, 0f), new Vector3(3.8f, 0.55f, 3.1f),
                roof, 0.15f);
            Prim.Cube("Win", house.transform, new Vector3(0f, 1.6f, 1.35f), new Vector3(0.7f, 0.7f, 0.08f),
                new Color(1f, 0.8f, 0.4f), 0.4f);
        }

        private void BuildPine(Vector3 pos, float scale)
        {
            var tree = new GameObject("Pine");
            tree.transform.SetParent(_rig, false);
            tree.transform.position = pos;
            tree.transform.localScale = Vector3.one * scale;
            Prim.Cyl("Trunk", tree.transform, new Vector3(0f, 0.7f, 0f), new Vector3(0.35f, 0.7f, 0.35f),
                MatLib.Wood, 0.15f);
            Prim.Cube("T1", tree.transform, new Vector3(0f, 1.6f, 0f), new Vector3(1.8f, 0.8f, 1.8f),
                MatLib.Leaf, 0.12f);
            Prim.Cube("T2", tree.transform, new Vector3(0f, 2.3f, 0f), new Vector3(1.3f, 0.7f, 1.3f),
                MatLib.Leaf, 0.12f);
            Prim.Cube("T3", tree.transform, new Vector3(0f, 2.9f, 0f), new Vector3(0.85f, 0.6f, 0.85f),
                MatLib.Leaf, 0.12f);
        }

        private Light MakeTorch(Vector3 pos, Color flame)
        {
            Prim.Cyl("TorchStick", _rig, pos, new Vector3(0.12f, 0.55f, 0.12f), MatLib.Wood, 0.2f);
            var fire = Prim.Sphere("Flame", _rig, pos + Vector3.up * 0.7f, 0.55f, flame, 0.2f);
            fire.GetComponent<MeshRenderer>().sharedMaterial =
                MatLib.GetEmissive(flame, flame, 2.4f, 0.2f);
            var light = fire.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = flame;
            light.range = 10f;
            light.intensity = 2.4f;
            return light;
        }

        private void SpawnEmbers(int count)
        {
            _embers = new Transform[count];
            for (var i = 0; i < count; i++)
            {
                var c = Color.Lerp(new Color(1f, 0.7f, 0.35f), new Color(1f, 0.45f, 0.2f), Random.value);
                var e = Prim.Sphere($"Ember_{i}", _rig,
                    new Vector3(Random.Range(-8f, 8f), Random.Range(0.5f, 6f), Random.Range(2f, 16f)),
                    Random.Range(0.04f, 0.1f), c, 0.1f);
                e.GetComponent<MeshRenderer>().sharedMaterial =
                    MatLib.GetEmissive(c, c, 1.6f, 0.1f);
                _embers[i] = e.transform;
            }
        }

        private void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("TitleCamera");
                cam = go.AddComponent<Camera>();
                go.tag = "MainCamera";
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.55f, 0.62f, 0.72f);
            cam.orthographic = false;
            cam.fieldOfView = 36f;
            cam.nearClipPlane = 0.2f;
            cam.farClipPlane = 80f;
            _camBase = new Vector3(0f, 3.5f, -2.5f);
            cam.transform.position = _camBase;
            cam.transform.rotation = Quaternion.Euler(7f, 0f, 0f);
            cam.transform.LookAt(new Vector3(0f, 4f, 16f));
        }

        private void Update()
        {
            _t += Time.unscaledDeltaTime;

            if (_torchL != null)
            {
                _torchL.intensity = 2.2f + Mathf.PerlinNoise(_t * 3.2f, 0.1f) * 0.9f;
            }

            if (_torchR != null)
            {
                _torchR.intensity = 2.2f + Mathf.PerlinNoise(0.3f, _t * 2.8f) * 0.9f;
            }

            if (_key != null)
            {
                _key.intensity = 1.45f + Mathf.Sin(_t * 0.35f) * 0.08f;
            }

            var cam = Camera.main;
            if (cam != null)
            {
                var drift = new Vector3(
                    Mathf.Sin(_t * 0.18f) * 0.35f,
                    Mathf.Sin(_t * 0.14f) * 0.12f,
                    Mathf.Cos(_t * 0.11f) * 0.2f);
                cam.transform.position = _camBase + drift;
                var look = new Vector3(Mathf.Sin(_t * 0.12f) * 0.4f, 3.8f + Mathf.Sin(_t * 0.09f) * 0.15f, 16f);
                cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation,
                    Quaternion.LookRotation(look - cam.transform.position), Time.unscaledDeltaTime * 1.2f);
            }

            if (_embers == null)
            {
                return;
            }

            for (var i = 0; i < _embers.Length; i++)
            {
                var e = _embers[i];
                if (e == null)
                {
                    continue;
                }

                var p = e.localPosition;
                p.y += (0.25f + (i % 5) * 0.04f) * Time.unscaledDeltaTime;
                p.x += Mathf.Sin(_t * 0.7f + i) * 0.15f * Time.unscaledDeltaTime;
                if (p.y > 8f)
                {
                    p.y = 0.3f;
                    p.x = Random.Range(-8f, 8f);
                    p.z = Random.Range(2f, 16f);
                }

                e.localPosition = p;
            }
        }
    }
}
