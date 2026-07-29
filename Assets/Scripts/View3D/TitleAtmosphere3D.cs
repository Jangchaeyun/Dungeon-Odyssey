using UnityEngine;

namespace DungeonOdyssey.View3D
{
    /// <summary>
    /// 타이틀용 풀블리드 배경 — 폐허 관문·횃불·안개·느린 카메라 드리프트.
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

            // 바닥·안개층
            Prim.Cube("Ground", _rig, new Vector3(0f, -0.5f, 6f), new Vector3(48f, 1f, 40f),
                new Color(0.1f, 0.09f, 0.1f), 0.12f);
            Prim.Cube("AshPath", _rig, new Vector3(0f, 0.02f, 4f), new Vector3(4.2f, 0.04f, 18f),
                new Color(0.22f, 0.18f, 0.16f), 0.15f);

            // 관문 실루엣 (화면을 가로지르는 지배적 시각)
            BuildGate(-1);
            BuildGate(1);

            // 먼 배경 벽
            Prim.Cube("FarWall", _rig, new Vector3(0f, 4f, 22f), new Vector3(36f, 12f, 1.2f),
                new Color(0.12f, 0.11f, 0.14f), 0.1f);
            Prim.Cube("ArchL", _rig, new Vector3(-3.2f, 5.2f, 18f), new Vector3(2.2f, 8f, 1.4f),
                MatLib.StoneDark, 0.18f);
            Prim.Cube("ArchR", _rig, new Vector3(3.2f, 5.2f, 18f), new Vector3(2.2f, 8f, 1.4f),
                MatLib.StoneDark, 0.18f);
            Prim.Cube("ArchTop", _rig, new Vector3(0f, 9.2f, 18f), new Vector3(8.6f, 1.6f, 1.6f),
                MatLib.Stone, 0.2f);

            // 룬 빛
            var rune = Prim.Cube("Rune", _rig, new Vector3(0f, 6.4f, 17.2f), new Vector3(1.4f, 1.4f, 0.2f),
                MatLib.AccentCopper, 0.4f);
            rune.GetComponent<MeshRenderer>().sharedMaterial =
                MatLib.GetEmissive(MatLib.AccentCopper, new Color(0.9f, 0.45f, 0.25f), 1.4f, 0.35f);

            BuildPillar(new Vector3(-7.5f, 0f, 8f), 1.05f);
            BuildPillar(new Vector3(7.5f, 0f, 8f), 1.05f);
            BuildPillar(new Vector3(-5.2f, 0f, 14f), 0.85f);
            BuildPillar(new Vector3(5.2f, 0f, 14f), 0.85f);

            // 잔해
            Prim.Cube("RubbleA", _rig, new Vector3(-2.4f, 0.25f, 3.2f), new Vector3(1.4f, 0.5f, 0.9f),
                MatLib.StoneDark, 0.15f);
            Prim.Cube("RubbleB", _rig, new Vector3(3.1f, 0.18f, 5.5f), new Vector3(1.1f, 0.35f, 1.3f),
                MatLib.Stone, 0.15f);
            Prim.Cube("Fallen", _rig, new Vector3(1.8f, 0.55f, 10f), new Vector3(3.2f, 0.35f, 0.7f),
                MatLib.StoneDark, 0.2f).transform.localRotation = Quaternion.Euler(0f, 18f, -12f);

            _torchL = MakeTorch(new Vector3(-4.6f, 3.2f, 12f), new Color(1f, 0.55f, 0.28f));
            _torchR = MakeTorch(new Vector3(4.6f, 3.2f, 12f), new Color(1f, 0.48f, 0.22f));

            // 키 라이트 — 석양빛
            var keyGo = new GameObject("KeyLight");
            keyGo.transform.SetParent(_rig, false);
            keyGo.transform.position = new Vector3(-6f, 10f, 2f);
            _key = keyGo.AddComponent<Light>();
            _key.type = LightType.Directional;
            _key.transform.rotation = Quaternion.Euler(28f, 40f, 0f);
            _key.color = new Color(0.85f, 0.78f, 0.72f);
            _key.intensity = 0.9f;
            _key.shadows = LightShadows.Soft;

            var fillGo = new GameObject("FillLight");
            fillGo.transform.SetParent(_rig, false);
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.transform.rotation = Quaternion.Euler(50f, -120f, 0f);
            fill.color = new Color(0.28f, 0.42f, 0.55f);
            fill.intensity = 0.32f;

            SpawnEmbers(30);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.038f;
            RenderSettings.fogColor = new Color(0.08f, 0.09f, 0.12f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.12f, 0.14f, 0.18f);

            SetupCamera();
        }

        private void BuildGate(int side)
        {
            var x = side * 5.8f;
            Prim.Cube($"GatePillar_{(side < 0 ? "L" : "R")}", _rig, new Vector3(x, 3.6f, 16f),
                new Vector3(1.6f, 7.2f, 1.8f), MatLib.DungeonWall, 0.18f);
            Prim.Cube($"GateTrim_{(side < 0 ? "L" : "R")}", _rig, new Vector3(x + side * 0.2f, 6.8f, 15.2f),
                new Vector3(0.35f, 2.4f, 0.35f), MatLib.NpcTrim, 0.45f);
        }

        private void BuildPillar(Vector3 pos, float scale)
        {
            Prim.Cyl("Pillar", _rig, pos + Vector3.up * (2.4f * scale),
                new Vector3(0.7f * scale, 2.4f * scale, 0.7f * scale), MatLib.StoneDark, 0.2f);
            Prim.Cube("Cap", _rig, pos + Vector3.up * (4.85f * scale),
                new Vector3(1.15f * scale, 0.28f * scale, 1.15f * scale), MatLib.NpcTrim, 0.4f);
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
            light.range = 9f;
            light.intensity = 2.2f;
            return light;
        }

        private void SpawnEmbers(int count)
        {
            _embers = new Transform[count];
            for (var i = 0; i < count; i++)
            {
                var c = Color.Lerp(new Color(1f, 0.55f, 0.25f), new Color(0.9f, 0.35f, 0.15f), Random.value);
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
            cam.backgroundColor = new Color(0.06f, 0.055f, 0.06f);
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

            // 횃불 깜빡임
            if (_torchL != null)
            {
                _torchL.intensity = 2f + Mathf.PerlinNoise(_t * 3.2f, 0.1f) * 0.9f;
            }

            if (_torchR != null)
            {
                _torchR.intensity = 2f + Mathf.PerlinNoise(0.3f, _t * 2.8f) * 0.9f;
            }

            if (_key != null)
            {
                _key.intensity = 0.78f + Mathf.Sin(_t * 0.35f) * 0.06f;
            }

            var cam = Camera.main;
            if (cam != null)
            {
                // 느린 숨결 드리프트
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
