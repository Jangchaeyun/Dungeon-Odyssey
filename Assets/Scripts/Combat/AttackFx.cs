using System.Collections;
using DungeonOdyssey.View3D;
using UnityEngine;

namespace DungeonOdyssey.Combat
{
    /// <summary>스킬 전용 VFX 스타일 — 모션마다 형태가 다름.</summary>
    public enum SkillFxStyle
    {
        DashSlash,
        Beam,
        Stab,
        Blink,
        Chaos,
        Spin,
        Crescent,
        Bolt,
        Frost,
        Holy,
        Void,
        Slam,
        Quake,
        Blood,
        Cone
    }

    public static class AttackFx
    {
        public static void PlaySlash3D(Vector3 origin, Vector3 dir, int comboIndex = 0)
        {
            PlaySlash3D(origin, dir, comboIndex, null);
        }

        /// <summary>칼끝 Transform을 따라가며 휘두름 궤적 FX를 그린다.</summary>
        public static void PlaySlash3D(Vector3 origin, Vector3 dir, int comboIndex, Transform followTip)
        {
            if (dir.sqrMagnitude < 0.01f)
            {
                dir = Vector3.forward;
            }

            dir.y = 0f;
            dir.Normalize();

            var host = new GameObject("SlashFxHost");
            host.transform.position = origin + dir * 0.08f + Vector3.up * 0.02f;
            host.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            var runner = host.AddComponent<FxRunner>();
            runner.StartCoroutine(SlashArcRoutine(host.transform, comboIndex, followTip, dir));
            Object.Destroy(host, 0.4f);
        }

        /// <summary>스킬 모션별 고유 연출 (색·형태 모두 다름).</summary>
        public static void PlaySkill(SkillFxStyle style, Color color, Vector3 origin, Vector3 dir,
            float scale = 1f)
        {
            if (dir.sqrMagnitude < 0.01f)
            {
                dir = Vector3.forward;
            }

            dir.y = 0f;
            dir = dir.normalized;
            scale = Mathf.Clamp(scale, 0.5f, 2.5f);
            // 알파가 너무 낮으면 안 보이므로 최소 확보
            if (color.a < 0.55f)
            {
                color.a = 0.85f;
            }

            switch (style)
            {
                case SkillFxStyle.Beam:
                    PlaySkillBeam(origin, dir, color, scale);
                    break;
                case SkillFxStyle.Stab:
                    PlaySkillStab(origin, dir, color, scale);
                    break;
                case SkillFxStyle.Blink:
                    PlaySkillBlink(origin, dir, color, scale);
                    break;
                case SkillFxStyle.Chaos:
                    PlaySkillChaos(origin, dir, color, scale);
                    break;
                case SkillFxStyle.Spin:
                    PlaySkillSpin(origin, color, scale);
                    break;
                case SkillFxStyle.Crescent:
                    PlaySkillCrescent(origin, dir, color, scale);
                    break;
                case SkillFxStyle.Bolt:
                    PlaySkillBolt(origin, dir, color, scale);
                    break;
                case SkillFxStyle.Frost:
                    PlaySkillFrost(origin, color, scale);
                    break;
                case SkillFxStyle.Holy:
                    PlaySkillHoly(origin, color, scale);
                    break;
                case SkillFxStyle.Void:
                    PlaySkillVoid(origin, color, scale);
                    break;
                case SkillFxStyle.Slam:
                    PlaySkillSlam(origin, color, scale, blood: false);
                    break;
                case SkillFxStyle.Blood:
                    PlaySkillSlam(origin, color, scale, blood: true);
                    break;
                case SkillFxStyle.Quake:
                    PlaySkillQuake(origin, color, scale);
                    break;
                case SkillFxStyle.Cone:
                    PlaySkillCone(origin, dir, color, scale);
                    break;
                default:
                    PlaySkillSlash(origin, dir, color, scale);
                    break;
            }
        }

        public static void PlayColoredImpact(Vector3 origin, Color color, bool big = false)
        {
            var count = big ? 10 : 6;
            for (var i = 0; i < count; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = "SkillSpark";
                var offset = Random.insideUnitSphere;
                offset.y = Mathf.Abs(offset.y) * 0.55f;
                go.transform.position = origin + offset * (big ? 0.35f : 0.18f);
                go.transform.localScale = Vector3.one * Random.Range(0.08f, big ? 0.16f : 0.1f);
                var mr = go.GetComponent<MeshRenderer>();
                mr.sharedMaterial = MatLib.GetFx(color, 0.75f);
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                Object.Destroy(go.GetComponent<Collider>());
                var chunk = go.AddComponent<FxChunk>();
                chunk.Velocity = offset.normalized * Random.Range(2.2f, 4.2f) + Vector3.up * 1.6f;
                chunk.Life = Random.Range(0.16f, 0.28f);
            }

            SpawnRing(origin, color, big ? 1.8f : 1.1f, big ? 0.2f : 0.14f);
        }

        private static void PlaySkillSlash(Vector3 origin, Vector3 dir, Color color, float scale)
        {
            var host = new GameObject("SkillSlash");
            // 칼끝이 바깥쪽이 되도록 중심을 살짝 뒤로
            host.transform.position = origin - dir * (0.45f * scale);
            host.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            var runner = host.AddComponent<FxRunner>();
            runner.StartCoroutine(ColoredSlashRoutine(host.transform, color, scale, -50f, 50f, 8f, 7));
            Object.Destroy(host, 0.35f);
        }

        private static void PlaySkillBeam(Vector3 origin, Vector3 dir, Color color, float scale)
        {
            var len = 2.8f * scale;
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "SkillBeam";
            go.transform.position = origin + dir * (len * 0.45f);
            go.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            go.transform.localScale = new Vector3(0.22f * scale, 0.22f * scale, len);
            var mr = go.GetComponent<MeshRenderer>();
            mr.sharedMaterial = MatLib.GetFx(color, 0.85f);
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Object.Destroy(go.GetComponent<Collider>());
            var fade = go.AddComponent<FxScaleFade>();
            fade.Life = 0.22f;
            Object.Destroy(go, 0.28f);

            for (var i = 0; i < 5; i++)
            {
                var tip = origin + dir * (len * (0.2f + i * 0.18f));
                PlayColoredImpact(tip, color, false);
            }
        }

        private static void PlaySkillStab(Vector3 origin, Vector3 dir, Color color, float scale)
        {
            for (var i = 0; i < 3; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "SkillStab";
                go.transform.position = origin + dir * (0.35f + i * 0.28f) + Vector3.up * 0.05f;
                go.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
                go.transform.localScale = new Vector3(0.06f, 0.06f, 0.45f * scale);
                go.GetComponent<MeshRenderer>().sharedMaterial = MatLib.GetFx(color, 0.8f);
                Object.Destroy(go.GetComponent<Collider>());
                var fade = go.AddComponent<FxScaleFade>();
                fade.Life = 0.1f;
                Object.Destroy(go, 0.12f);
            }

            PlayColoredImpact(origin + dir * 0.9f, color, false);
        }

        private static void PlaySkillBlink(Vector3 origin, Vector3 dir, Color color, float scale)
        {
            // 전방 잔상 + 후방 잔상
            SpawnPoof(origin, color, 0.45f * scale);
            SpawnPoof(origin + dir * 1.2f, color, 0.35f * scale);
            var host = new GameObject("SkillBlink");
            host.transform.position = origin;
            host.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            var runner = host.AddComponent<FxRunner>();
            runner.StartCoroutine(ColoredSlashRoutine(host.transform, color, scale * 0.9f, -35f, 35f, -8f, 5));
            Object.Destroy(host, 0.26f);
        }

        private static void PlaySkillChaos(Vector3 origin, Vector3 dir, Color color, float scale)
        {
            var alt = new Color(1f - color.r * 0.3f, color.b, color.g, color.a);
            for (var i = 0; i < 3; i++)
            {
                var yaw = i == 0 ? -50f : i == 1 ? 50f : 0f;
                var d = Quaternion.Euler(0f, yaw, 0f) * dir;
                var host = new GameObject("SkillChaos");
                host.transform.position = origin + d * 0.2f;
                host.transform.rotation = Quaternion.LookRotation(d, Vector3.up);
                var runner = host.AddComponent<FxRunner>();
                var c = i == 1 ? alt : color;
                runner.StartCoroutine(ColoredSlashRoutine(host.transform, c, scale, -40f, 40f, 12f, 4));
                Object.Destroy(host, 0.26f);
            }

            PlayColoredImpact(origin, color, true);
        }

        private static void PlaySkillSpin(Vector3 origin, Color color, float scale)
        {
            SpawnRing(origin, color, 1.6f * scale, 0.18f);
            var host = new GameObject("SkillSpin");
            host.transform.position = origin;
            var runner = host.AddComponent<FxRunner>();
            runner.StartCoroutine(OrbitCubesRoutine(host.transform, color, scale));
            Object.Destroy(host, 0.28f);
        }

        private static void PlaySkillCrescent(Vector3 origin, Vector3 dir, Color color, float scale)
        {
            var host = new GameObject("SkillCrescent");
            host.transform.position = origin;
            host.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
            var runner = host.AddComponent<FxRunner>();
            runner.StartCoroutine(ColoredSlashRoutine(host.transform, color, scale * 1.15f, -70f, 70f, 5f, 8));
            Object.Destroy(host, 0.3f);
            SpawnRing(origin + dir * 0.4f, color, 1.1f * scale, 0.12f);
        }

        private static void PlaySkillBolt(Vector3 origin, Vector3 dir, Color color, float scale)
        {
            PlaySkillSpin(origin, color, scale * 0.7f);
            var len = 3f * scale;
            var prev = origin;
            for (var i = 1; i <= 6; i++)
            {
                var t = i / 6f;
                var next = origin + dir * (len * t) +
                           Vector3.Cross(dir, Vector3.up) * (Mathf.Sin(i * 2.1f) * 0.18f * scale);
                next.y = origin.y + Random.Range(-0.05f, 0.15f);
                var mid = (prev + next) * 0.5f;
                var seg = next - prev;
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "SkillBolt";
                go.transform.position = mid;
                if (seg.sqrMagnitude > 0.001f)
                {
                    go.transform.rotation = Quaternion.LookRotation(seg.normalized, Vector3.up);
                }

                go.transform.localScale = new Vector3(0.07f * scale, 0.07f * scale, seg.magnitude);
                go.GetComponent<MeshRenderer>().sharedMaterial = MatLib.GetFx(color, 0.9f);
                Object.Destroy(go.GetComponent<Collider>());
                var fade = go.AddComponent<FxScaleFade>();
                fade.Life = 0.12f;
                Object.Destroy(go, 0.14f);
                prev = next;
            }

            PlayColoredImpact(origin + dir * len * 0.85f, color, true);
        }

        private static void PlaySkillFrost(Vector3 origin, Color color, float scale)
        {
            SpawnRing(origin, color, 1.8f * scale, 0.2f);
            for (var i = 0; i < 8; i++)
            {
                var ang = i * 45f * Mathf.Deg2Rad;
                var p = origin + new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * (0.7f * scale);
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "SkillIce";
                go.transform.position = p + Vector3.up * 0.35f;
                go.transform.localScale = new Vector3(0.1f, 0.55f * scale, 0.1f);
                go.transform.rotation = Quaternion.Euler(0f, i * 45f, 12f);
                go.GetComponent<MeshRenderer>().sharedMaterial = MatLib.GetFx(color, 0.7f);
                Object.Destroy(go.GetComponent<Collider>());
                var fade = go.AddComponent<FxScaleFade>();
                fade.Life = 0.22f;
                Object.Destroy(go, 0.24f);
            }
        }

        private static void PlaySkillHoly(Vector3 origin, Color color, float scale)
        {
            SpawnRing(origin, color, 1.5f * scale, 0.2f);
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "SkillHoly";
            pillar.transform.position = origin + Vector3.up * (0.9f * scale);
            pillar.transform.localScale = new Vector3(0.35f * scale, 1.1f * scale, 0.35f * scale);
            pillar.GetComponent<MeshRenderer>().sharedMaterial = MatLib.GetFx(color, 0.5f);
            Object.Destroy(pillar.GetComponent<Collider>());
            var fade = pillar.AddComponent<FxScaleFade>();
            fade.Life = 0.28f;
            Object.Destroy(pillar, 0.3f);
            PlayColoredImpact(origin + Vector3.up * 1.2f, color, true);
        }

        private static void PlaySkillVoid(Vector3 origin, Color color, float scale)
        {
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "SkillVoid";
            ball.transform.position = origin + Vector3.up * 0.6f;
            ball.transform.localScale = Vector3.one * (0.4f * scale);
            ball.GetComponent<MeshRenderer>().sharedMaterial = MatLib.GetFx(color, 0.4f);
            Object.Destroy(ball.GetComponent<Collider>());
            var pulse = ball.AddComponent<FxRingPulse>();
            pulse.MaxScale = 2.2f * scale;
            pulse.Life = 0.22f;
            pulse.UseSphere = true;
            SpawnRing(origin, color, 1.7f * scale, 0.18f);
            PlayColoredImpact(origin, color, true);
        }

        private static void PlaySkillSlam(Vector3 origin, Color color, float scale, bool blood)
        {
            SpawnRing(origin, color, 1.5f * scale, 0.16f);
            var count = blood ? 10 : 6;
            for (var i = 0; i < count; i++)
            {
                var go = GameObject.CreatePrimitive(blood ? PrimitiveType.Sphere : PrimitiveType.Cube);
                go.name = blood ? "SkillBlood" : "SkillDebris";
                var flat = Random.insideUnitSphere;
                flat.y = Mathf.Abs(flat.y) * 0.2f;
                go.transform.position = origin + flat * (0.35f * scale);
                go.transform.localScale = Vector3.one * Random.Range(0.05f, 0.12f) * scale;
                go.GetComponent<MeshRenderer>().sharedMaterial = MatLib.GetFx(color, 0.7f);
                Object.Destroy(go.GetComponent<Collider>());
                var chunk = go.AddComponent<FxChunk>();
                chunk.Velocity = flat.normalized * Random.Range(2f, 4f) + Vector3.up * (blood ? 2.2f : 1.2f);
                chunk.Life = blood ? 0.2f : 0.14f;
            }
        }

        private static void PlaySkillQuake(Vector3 origin, Color color, float scale)
        {
            for (var i = 0; i < 3; i++)
            {
                SpawnRing(origin, color, (0.9f + i * 0.55f) * scale, 0.1f + i * 0.03f);
            }

            PlaySkillSlam(origin, color, scale, blood: false);
        }

        private static void PlaySkillCone(Vector3 origin, Vector3 dir, Color color, float scale)
        {
            for (var i = -3; i <= 3; i++)
            {
                var d = Quaternion.Euler(0f, i * 14f, 0f) * dir;
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "SkillCone";
                go.transform.position = origin + d * (0.9f * scale);
                go.transform.rotation = Quaternion.LookRotation(d, Vector3.up);
                go.transform.localScale = new Vector3(0.1f, 0.08f, 1.2f * scale);
                go.GetComponent<MeshRenderer>().sharedMaterial = MatLib.GetFx(color, 0.8f);
                Object.Destroy(go.GetComponent<Collider>());
                var fade = go.AddComponent<FxScaleFade>();
                fade.Life = 0.14f;
                Object.Destroy(go, 0.16f);
            }

            PlayColoredImpact(origin + dir * scale, color, true);
        }

        private static void SpawnRing(Vector3 origin, Color color, float maxScale, float life)
        {
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "SkillRing";
            ring.transform.position = origin + Vector3.up * 0.05f;
            ring.transform.localScale = new Vector3(0.25f, 0.02f, 0.25f);
            var mr = ring.GetComponent<MeshRenderer>();
            mr.sharedMaterial = MatLib.GetFx(color, 0.2f);
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Object.Destroy(ring.GetComponent<Collider>());
            var pulse = ring.AddComponent<FxRingPulse>();
            pulse.MaxScale = maxScale;
            pulse.Life = life;
        }

        private static void SpawnPoof(Vector3 origin, Color color, float size)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "SkillPoof";
            go.transform.position = origin + Vector3.up * 0.5f;
            go.transform.localScale = Vector3.one * size;
            go.GetComponent<MeshRenderer>().sharedMaterial = MatLib.GetFx(color, 0.35f);
            Object.Destroy(go.GetComponent<Collider>());
            var pulse = go.AddComponent<FxRingPulse>();
            pulse.MaxScale = size * 2.2f;
            pulse.Life = 0.16f;
            pulse.UseSphere = true;
        }

        private static IEnumerator ColoredSlashRoutine(Transform host, Color color, float scale,
            float startA, float endA, float tilt, int segments)
        {
            var blades = new Transform[segments];
            var mats = new Material[segments];
            for (var i = 0; i < segments; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "SkillSlashSeg";
                go.transform.SetParent(host, false);
                Object.Destroy(go.GetComponent<Collider>());
                mats[i] = MatLib.GetFx(color, 0.85f);
                go.GetComponent<MeshRenderer>().sharedMaterial = mats[i];
                blades[i] = go.transform;
                blades[i].localScale = Vector3.zero;
            }

            var t = 0f;
            const float dur = 0.13f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                var p = Mathf.Clamp01(t / dur);
                var ease = 1f - (1f - p) * (1f - p);
                for (var i = 0; i < segments; i++)
                {
                    var lag = i / (float)Mathf.Max(1, segments - 1);
                    var segP = Mathf.Clamp01(ease - lag * 0.15f);
                    var angle = Mathf.Lerp(startA, endA, segP);
                    var rad = angle * Mathf.Deg2Rad;
                    var radius = (0.45f + lag * 0.3f) * scale;
                    blades[i].localPosition = new Vector3(
                        Mathf.Sin(rad) * radius,
                        0.05f + Mathf.Sin(segP * Mathf.PI) * 0.08f,
                        Mathf.Cos(rad) * radius * 0.2f + 0.1f);
                    var tangent = new Vector3(Mathf.Cos(rad), 0.1f, -Mathf.Sin(rad) * 0.2f);
                    if (tangent.sqrMagnitude > 0.001f)
                    {
                        blades[i].localRotation = Quaternion.LookRotation(tangent.normalized, Vector3.up)
                                                  * Quaternion.Euler(tilt, 0f, 0f);
                    }

                    var fade = Mathf.Max(0.4f, (1f - lag * 0.35f) * (1f - ease * 0.3f));
                    blades[i].localScale = new Vector3(
                        0.045f * fade * scale, 0.02f * fade, 0.42f * fade * scale);
                    if (mats[i] != null)
                    {
                        var c = color;
                        c.a = color.a * fade;
                        mats[i].color = c;
                        if (mats[i].HasProperty("_BaseColor"))
                        {
                            mats[i].SetColor("_BaseColor", c);
                        }
                    }
                }

                yield return null;
            }

            for (var i = 0; i < blades.Length; i++)
            {
                if (blades[i] != null)
                {
                    Object.Destroy(blades[i].gameObject);
                }
            }
        }

        private static IEnumerator OrbitCubesRoutine(Transform host, Color color, float scale)
        {
            const int n = 6;
            var cubes = new Transform[n];
            for (var i = 0; i < n; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "SkillOrbit";
                go.transform.SetParent(host, false);
                Object.Destroy(go.GetComponent<Collider>());
                go.GetComponent<MeshRenderer>().sharedMaterial = MatLib.GetFx(color, 0.8f);
                cubes[i] = go.transform;
            }

            var t = 0f;
            const float dur = 0.22f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                var p = t / dur;
                for (var i = 0; i < n; i++)
                {
                    var ang = (i / (float)n + p) * Mathf.PI * 2f;
                    var r = (0.7f + p * 0.5f) * scale;
                    cubes[i].localPosition = new Vector3(Mathf.Cos(ang) * r, 0.2f, Mathf.Sin(ang) * r);
                    cubes[i].localScale = Vector3.one * (0.12f * (1f - p * 0.5f) * scale);
                }

                yield return null;
            }

            for (var i = 0; i < n; i++)
            {
                if (cubes[i] != null)
                {
                    Object.Destroy(cubes[i].gameObject);
                }
            }
        }

        public static void PlayImpact3D(Vector3 origin, bool critical = false, bool slime = false)
        {
            var count = critical ? 6 : 3;
            var color = slime
                ? new Color(0.55f, 0.35f, 0.7f, 0.65f)
                : critical
                    ? new Color(1f, 0.88f, 0.4f, 0.8f)
                    : new Color(1f, 0.72f, 0.48f, 0.7f);

            for (var i = 0; i < count; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = "ImpactSpark3D";
                var offset = Random.insideUnitSphere;
                offset.y = Mathf.Abs(offset.y) * 0.5f;
                go.transform.position = origin + offset * (critical ? 0.2f : 0.12f);
                go.transform.localScale = Vector3.one * Random.Range(0.03f, critical ? 0.08f : 0.05f);
                go.GetComponent<MeshRenderer>().sharedMaterial = MatLib.GetFx(color, 0.7f);
                Object.Destroy(go.GetComponent<Collider>());
                var chunk = go.AddComponent<FxChunk>();
                chunk.Velocity = offset.normalized * Random.Range(1.6f, 3.2f) + Vector3.up * 1.1f;
                chunk.Life = Random.Range(0.1f, 0.16f);
            }

            if (critical)
            {
                var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ring.name = "ShockRing";
                ring.transform.position = origin;
                ring.transform.localScale = new Vector3(0.16f, 0.006f, 0.16f);
                ring.GetComponent<MeshRenderer>().sharedMaterial =
                    MatLib.GetFx(new Color(1f, 0.9f, 0.45f, 0.4f), 0.14f);
                Object.Destroy(ring.GetComponent<Collider>());
                var pulse = ring.AddComponent<FxRingPulse>();
                pulse.MaxScale = 1.35f;
                pulse.Life = 0.14f;
            }
            else
            {
                // 일반 타격도 짧은 링으로 무게감
                var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ring.name = "HitRing";
                ring.transform.position = origin;
                ring.transform.localScale = new Vector3(0.1f, 0.005f, 0.1f);
                ring.GetComponent<MeshRenderer>().sharedMaterial =
                    MatLib.GetFx(new Color(1f, 0.78f, 0.5f, 0.28f), 0.1f);
                Object.Destroy(ring.GetComponent<Collider>());
                var pulse = ring.AddComponent<FxRingPulse>();
                pulse.MaxScale = 0.7f;
                pulse.Life = 0.08f;
            }
        }

        public static void PlayBiteTelegraph(Vector3 origin)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "BiteWarn";
            go.transform.position = origin + Vector3.up * 0.9f;
            go.transform.localScale = Vector3.one * 0.32f;
            go.GetComponent<MeshRenderer>().sharedMaterial =
                MatLib.GetFx(new Color(1f, 0.3f, 0.22f, 0.5f), 0.2f);
            Object.Destroy(go.GetComponent<Collider>());
            var pulse = go.AddComponent<FxRingPulse>();
            pulse.MaxScale = 1.1f;
            pulse.Life = 0.24f;
            pulse.UseSphere = true;
        }

        public static void PlaySlash(Vector3 origin, Vector2 dir)
        {
            PlaySlash3D(origin, new Vector3(dir.x, 0f, dir.y));
        }

        private static IEnumerator SlashArcRoutine(Transform host, int combo, Transform followTip,
            Vector3 faceDir)
        {
            float startA, endA, tilt;
            var alpha = combo == 2 ? 0.72f : combo == 1 ? 0.55f : 0.5f;
            var color = combo == 2
                ? new Color(1f, 0.88f, 0.55f, alpha)
                : new Color(0.92f, 0.95f, 1f, alpha);
            var weight = combo == 2 ? 1.2f : 1f;

            switch (combo)
            {
                case 1:
                    startA = 62f;
                    endA = -62f;
                    tilt = -14f;
                    break;
                case 2:
                    startA = -8f;
                    endA = 8f;
                    tilt = 68f;
                    break;
                default:
                    startA = -55f;
                    endA = 55f;
                    tilt = 8f;
                    break;
            }

            var segments = combo == 2 ? 8 : 7;
            var blades = new Transform[segments];
            var mats = new Material[segments];

            for (var i = 0; i < segments; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "SlashCore";
                go.transform.SetParent(host, false);
                Object.Destroy(go.GetComponent<Collider>());
                mats[i] = MatLib.GetFx(color, 0.8f);
                go.GetComponent<MeshRenderer>().sharedMaterial = mats[i];
                blades[i] = go.transform;
                blades[i].localScale = Vector3.zero;
            }

            // 칼끝이 호의 바깥쪽이 되도록, 중심은 칼끝보다 몸 쪽으로 둠
            var tipArcRadius = 0.58f * weight;

            var t = 0f;
            var dur = combo == 2 ? 0.15f : 0.13f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                var p = Mathf.Clamp01(t / dur);
                var ease = 1f - (1f - p) * (1f - p);

                if (followTip != null)
                {
                    var tip = followTip.position;
                    var look = faceDir;
                    look.y = 0f;
                    if (look.sqrMagnitude < 0.01f)
                    {
                        look = host.forward;
                    }

                    look.Normalize();
                    host.position = tip - look * (tipArcRadius * 0.9f);
                    host.rotation = Quaternion.LookRotation(look, Vector3.up);
                }

                for (var i = 0; i < segments; i++)
                {
                    var lag = i / (float)Mathf.Max(1, segments - 1);
                    var segP = Mathf.Clamp01(ease - lag * 0.16f);
                    var angle = Mathf.Lerp(startA, endA, segP);
                    var rad = angle * Mathf.Deg2Rad;
                    var radius = followTip != null
                        ? tipArcRadius * (0.82f + lag * 0.2f)
                        : (0.42f + lag * 0.28f) * weight;
                    var height = combo == 2
                        ? Mathf.Lerp(0.42f, -0.12f, segP)
                        : 0.02f + Mathf.Sin(segP * Mathf.PI) * 0.08f;

                    blades[i].localPosition = new Vector3(
                        Mathf.Sin(rad) * radius,
                        height,
                        Mathf.Cos(rad) * radius * 0.22f + 0.1f);

                    var tangent = new Vector3(
                        Mathf.Cos(rad),
                        combo == 2 ? -0.85f : 0.1f,
                        -Mathf.Sin(rad) * 0.22f);
                    if (tangent.sqrMagnitude > 0.001f)
                    {
                        blades[i].localRotation = Quaternion.LookRotation(tangent.normalized, Vector3.up)
                                                  * Quaternion.Euler(tilt, 0f, 0f);
                    }

                    var fade = Mathf.Max(0.35f, (1f - lag * 0.35f) * (1f - ease * 0.35f));
                    var thick = i < 2 ? 1.35f : 1f;
                    var len = 0.38f + (1f - lag) * 0.2f;
                    blades[i].localScale = new Vector3(
                        0.04f * fade * weight * thick,
                        0.018f * fade * thick,
                        len * fade * weight);

                    if (mats[i] != null)
                    {
                        var c = color;
                        c.a = color.a * fade;
                        mats[i].color = c;
                        if (mats[i].HasProperty("_BaseColor"))
                        {
                            mats[i].SetColor("_BaseColor", c);
                        }
                    }
                }

                yield return null;
            }

            for (var i = 0; i < blades.Length; i++)
            {
                if (blades[i] != null)
                {
                    Object.Destroy(blades[i].gameObject);
                }
            }
        }

        private class FxRunner : MonoBehaviour
        {
        }

        private class FxChunk : MonoBehaviour
        {
            public Vector3 Velocity;
            public float Life = 0.18f;
            private Material _mat;
            private Color _base;
            private float _life0;

            private void Start()
            {
                _life0 = Life;
                var r = GetComponent<MeshRenderer>();
                if (r != null)
                {
                    _mat = r.material;
                    _base = _mat.HasProperty("_BaseColor") ? _mat.GetColor("_BaseColor") : _mat.color;
                }
            }

            private void Update()
            {
                Life -= Time.unscaledDeltaTime;
                transform.position += Velocity * Time.unscaledDeltaTime;
                Velocity += Vector3.down * 12f * Time.unscaledDeltaTime;
                transform.localScale *= 0.94f;

                if (_mat != null)
                {
                    var c = _base;
                    c.a = _base.a * Mathf.Clamp01(Life / Mathf.Max(0.01f, _life0));
                    _mat.color = c;
                    if (_mat.HasProperty("_BaseColor"))
                    {
                        _mat.SetColor("_BaseColor", c);
                    }
                }

                if (Life <= 0f)
                {
                    Destroy(gameObject);
                }
            }
        }

        private class FxRingPulse : MonoBehaviour
        {
            public float MaxScale = 1.2f;
            public float Life = 0.16f;
            public bool UseSphere;

            private float _t;
            private Vector3 _start;
            private Material _mat;
            private Color _base;

            private void Start()
            {
                _start = transform.localScale;
                var r = GetComponent<MeshRenderer>();
                if (r != null)
                {
                    _mat = r.material;
                    _base = _mat.HasProperty("_BaseColor") ? _mat.GetColor("_BaseColor") : _mat.color;
                }
            }

            private void Update()
            {
                _t += Time.unscaledDeltaTime;
                var p = Mathf.Clamp01(_t / Life);
                var s = Mathf.Lerp(_start.x, MaxScale, p);
                transform.localScale = UseSphere
                    ? Vector3.one * s
                    : new Vector3(s, _start.y, s);

                if (_mat != null)
                {
                    var c = _base;
                    c.a = _base.a * (1f - p);
                    _mat.color = c;
                    if (_mat.HasProperty("_BaseColor"))
                    {
                        _mat.SetColor("_BaseColor", c);
                    }
                }

                if (p >= 1f)
                {
                    Destroy(gameObject);
                }
            }
        }

        private class FxScaleFade : MonoBehaviour
        {
            public float Life = 0.15f;
            private float _t;
            private Vector3 _scale0;
            private Material _mat;
            private Color _base;

            private void Start()
            {
                _scale0 = transform.localScale;
                var r = GetComponent<MeshRenderer>();
                if (r != null)
                {
                    _mat = r.material;
                    _base = _mat.HasProperty("_BaseColor") ? _mat.GetColor("_BaseColor") : _mat.color;
                }
            }

            private void Update()
            {
                _t += Time.unscaledDeltaTime;
                var p = Mathf.Clamp01(_t / Life);
                transform.localScale = _scale0 * (1f - p * 0.35f);
                if (_mat != null)
                {
                    var c = _base;
                    c.a = _base.a * (1f - p);
                    _mat.color = c;
                    if (_mat.HasProperty("_BaseColor"))
                    {
                        _mat.SetColor("_BaseColor", c);
                    }
                }

                if (p >= 1f)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
