using DungeonOdyssey.Combat;
using DungeonOdyssey.Dungeon;
using DungeonOdyssey.Enemy;
using DungeonOdyssey.Player;
using UnityEngine;

namespace DungeonOdyssey.View3D
{
    public static class ActorFactory3D
    {
        public static GameObject CreatePlayer(Vector3 position)
        {
            var root = new GameObject("Player");
            root.tag = "Player";
            root.transform.position = position;

            var aiVisual = ModelCatalog.TrySpawnLocal(ModelCatalog.Player, root.transform, Vector3.zero);
            if (aiVisual != null)
            {
                aiVisual.name = "Visual";
                FinishPlayer(root, bindProceduralAnim: false);
                return root;
            }

            // 날씬한 암살자 기사 — 낮은 채도, 강철·와인 망토
            var visual = Prim.Bone("Visual", root.transform, Vector3.zero);
            var hip = Prim.Bone("Hip", visual, new Vector3(0f, 0.98f, 0f));

            var legL = Prim.Bone("LegL", hip, new Vector3(-0.12f, -0.02f, 0f));
            Prim.Cap("ThighL", legL, new Vector3(0f, -0.24f, 0f), new Vector3(0.16f, 0.3f, 0.16f), MatLib.Pants, 0.2f);
            Prim.Cap("CalfL", legL, new Vector3(0f, -0.58f, 0.01f), new Vector3(0.14f, 0.28f, 0.14f), MatLib.MetalDark, 0.45f);
            Prim.Cube("BootL", legL, new Vector3(0f, -0.88f, 0.05f), new Vector3(0.18f, 0.12f, 0.3f), MatLib.Boot, 0.15f);

            var legR = Prim.Bone("LegR", hip, new Vector3(0.12f, -0.02f, 0f));
            Prim.Cap("ThighR", legR, new Vector3(0f, -0.24f, 0f), new Vector3(0.16f, 0.3f, 0.16f), MatLib.Pants, 0.2f);
            Prim.Cap("CalfR", legR, new Vector3(0f, -0.58f, 0.01f), new Vector3(0.14f, 0.28f, 0.14f), MatLib.MetalDark, 0.45f);
            Prim.Cube("BootR", legR, new Vector3(0f, -0.88f, 0.05f), new Vector3(0.18f, 0.12f, 0.3f), MatLib.Boot, 0.15f);

            var chest = Prim.Bone("Chest", hip, new Vector3(0f, 0.3f, 0f));
            Prim.Cube("Pelvis", hip, new Vector3(0f, 0.02f, 0f), new Vector3(0.34f, 0.14f, 0.22f), MatLib.MetalDark, 0.5f);
            Prim.Cap("Torso", chest, new Vector3(0f, 0.2f, 0f), new Vector3(0.38f, 0.36f, 0.26f), MatLib.Cloth, 0.2f);
            Prim.Cube("Breastplate", chest, new Vector3(0f, 0.26f, 0.1f), new Vector3(0.36f, 0.32f, 0.1f), MatLib.Metal, 0.65f);
            Prim.Cube("PauldronL", chest, new Vector3(-0.28f, 0.38f, 0f), new Vector3(0.16f, 0.12f, 0.2f), MatLib.MetalDark, 0.55f);
            Prim.Cube("PauldronR", chest, new Vector3(0.28f, 0.38f, 0f), new Vector3(0.16f, 0.12f, 0.2f), MatLib.MetalDark, 0.55f);
            Prim.Cube("Belt", hip, new Vector3(0f, 0.1f, 0f), new Vector3(0.4f, 0.08f, 0.26f), MatLib.Belt, 0.25f);
            Prim.Cube("Buckle", hip, new Vector3(0f, 0.1f, 0.14f), new Vector3(0.1f, 0.08f, 0.04f), MatLib.NpcTrim, 0.55f);

            Prim.Cube("Cape", chest, new Vector3(0f, 0.05f, -0.18f), new Vector3(0.42f, 0.85f, 0.04f), MatLib.Cape, 0.12f);
            Prim.Cube("CapeTrim", chest, new Vector3(0f, 0.42f, -0.2f), new Vector3(0.44f, 0.06f, 0.03f), MatLib.NpcTrim, 0.4f);

            // 어깨 → 팔뚝 → 손 체인 (무기가 손에 붙고 팔이 접힘)
            var armL = Prim.Bone("ArmL", chest, new Vector3(-0.3f, 0.34f, 0f));
            Prim.Sphere("ShoulderL", armL, Vector3.zero, 0.14f, MatLib.MetalDark, 0.5f);
            Prim.Cap("UpperArmMeshL", armL, new Vector3(0f, -0.18f, 0f), new Vector3(0.13f, 0.2f, 0.13f),
                MatLib.Cloth, 0.2f);
            var foreL = Prim.Bone("ForeArmL", armL, new Vector3(0f, -0.38f, 0.02f));
            Prim.Cap("ForeArmMeshL", foreL, new Vector3(0f, -0.16f, 0f), new Vector3(0.11f, 0.18f, 0.11f),
                MatLib.MetalDark, 0.5f);
            var handL = Prim.Bone("HandL", foreL, new Vector3(0f, -0.34f, 0.02f));
            Prim.Sphere("HandMeshL", handL, Vector3.zero, 0.12f, MatLib.Skin, 0.25f);

            var armR = Prim.Bone("ArmR", chest, new Vector3(0.3f, 0.34f, 0f));
            Prim.Sphere("ShoulderR", armR, Vector3.zero, 0.14f, MatLib.MetalDark, 0.5f);
            Prim.Cap("UpperArmMeshR", armR, new Vector3(0f, -0.18f, 0f), new Vector3(0.13f, 0.2f, 0.13f),
                MatLib.Cloth, 0.2f);
            var foreR = Prim.Bone("ForeArmR", armR, new Vector3(0f, -0.38f, 0.02f));
            Prim.Cap("ForeArmMeshR", foreR, new Vector3(0f, -0.16f, 0f), new Vector3(0.11f, 0.18f, 0.11f),
                MatLib.MetalDark, 0.5f);
            var handR = Prim.Bone("HandR", foreR, new Vector3(0f, -0.34f, 0.02f));
            Prim.Sphere("HandMeshR", handR, Vector3.zero, 0.12f, MatLib.Skin, 0.25f);

            var sword = EquipmentBuilder.BuildSword(handR);
            var shield = EquipmentBuilder.BuildShield(handL);

            var neck = Prim.Bone("Neck", chest, new Vector3(0f, 0.52f, 0f));
            Prim.Cyl("NeckMesh", neck, Vector3.zero, new Vector3(0.09f, 0.07f, 0.09f), MatLib.Skin, 0.25f);
            var head = Prim.Bone("Head", neck, new Vector3(0f, 0.16f, 0f));
            Prim.Sphere("Skull", head, Vector3.zero, 0.3f, MatLib.Skin, 0.3f);
            Prim.Sphere("Hair", head, new Vector3(0f, 0.06f, -0.04f), 0.32f, MatLib.Hair, 0.1f);
            Prim.Cube("Brow", head, new Vector3(0f, 0.05f, 0.13f), new Vector3(0.18f, 0.025f, 0.04f), MatLib.Hair, 0.1f);
            Prim.Sphere("EyeL", head, new Vector3(-0.06f, 0.02f, 0.13f), 0.045f, MatLib.EyeSteel, 0.5f);
            Prim.Sphere("EyeR", head, new Vector3(0.06f, 0.02f, 0.13f), 0.045f, MatLib.EyeSteel, 0.5f);
            Prim.Sphere("PupilL", head, new Vector3(-0.06f, 0.02f, 0.15f), 0.022f, MatLib.Black, 0.2f);
            Prim.Sphere("PupilR", head, new Vector3(0.06f, 0.02f, 0.15f), 0.022f, MatLib.Black, 0.2f);
            Prim.Sphere("Nose", head, new Vector3(0f, -0.02f, 0.15f), 0.045f, new Color(0.7f, 0.55f, 0.48f), 0.25f);
            Prim.Cube("HelmBand", head, new Vector3(0f, 0.08f, 0.02f), new Vector3(0.28f, 0.06f, 0.28f), MatLib.MetalDark, 0.55f);

            Prim.Cyl("Shadow", visual, new Vector3(0f, 0.015f, 0f), new Vector3(0.7f, 0.008f, 0.7f),
                new Color(0f, 0f, 0f, 0.45f), 0.05f);

            FinishPlayer(root, bindProceduralAnim: true, hip, chest, head, legL, legR, armL, armR, sword,
                foreL, foreR, handL, handR, shield);
            return root;
        }

        private static void FinishPlayer(GameObject root, bool bindProceduralAnim,
            Transform hip = null, Transform chest = null, Transform head = null,
            Transform legL = null, Transform legR = null, Transform armL = null, Transform armR = null,
            Transform sword = null, Transform foreL = null, Transform foreR = null,
            Transform handL = null, Transform handR = null, Transform shield = null)
        {
            CharacterPhysics.Apply3D(root, radius: 0.34f, height: 1.9f, center: new Vector3(0f, 0.95f, 0f),
                mass: 1.2f);

            root.AddComponent<Health>();
            var reactor = root.AddComponent<CombatReactor>();
            reactor.Configure3D(player: true, knockback: 3f);
            root.AddComponent<PlayerStats>();

            // HumanoidAnimator를 PlayerCombat보다 먼저 붙여 Awake에서 참조되게 함
            var anim = root.AddComponent<HumanoidAnimator>();
            if (bindProceduralAnim && hip != null)
            {
                anim.Hip = hip;
                anim.Chest = chest;
                anim.Head = head;
                anim.LegL = legL;
                anim.LegR = legR;
                anim.ArmL = armL;
                anim.ArmR = armR;
                anim.ForeArmL = foreL;
                anim.ForeArmR = foreR;
                anim.HandL = handL;
                anim.HandR = handR;
                anim.Sword = sword;
                anim.Shield = shield;
                anim.Bind();
            }
            else
            {
                EquipmentBuilder.EnsurePlayerGear(root, armR, armL, out sword, out shield);
                anim.Sword = sword;
                anim.Shield = shield;
                anim.HandR = sword != null ? sword.parent : null;
                anim.ArmR = anim.HandR != null ? anim.HandR.parent : null;
                if (anim.ArmR != null && anim.ArmR.parent != null && anim.ArmR.name.Contains("Fore"))
                {
                    anim.ForeArmR = anim.ArmR;
                    anim.ArmR = anim.ForeArmR.parent;
                }

                anim.Bind();
                root.AddComponent<ModelBob>();
            }

            root.AddComponent<PlayerCombat>();
            root.AddComponent<PlayerController>();
            root.AddComponent<PlayerSkill>();
        }

        public static GameObject CreateSlime(Vector3 position, Transform parent)
        {
            // Void Maw — 보라빛 이끼/공허형 괴수 (귀여운 슬라임 X)
            var root = new GameObject("VoidMaw");
            if (parent != null)
            {
                root.transform.SetParent(parent, false);
            }

            root.transform.position = position;
            var enemy = LayerMask.NameToLayer("Enemy");
            if (enemy >= 0)
            {
                root.layer = enemy;
            }

            var ai = ModelCatalog.TrySpawnLocal(ModelCatalog.MonsterSlime, root.transform, Vector3.zero);
            if (ai != null)
            {
                ai.name = "Visual";
                FinishMonster(root, procedural: false);
                return root;
            }

            var body = Prim.Sphere("Body", root.transform, new Vector3(0f, 0.62f, 0f), 1.2f, MatLib.Slime, 0.55f);
            body.transform.localScale = new Vector3(1.15f, 0.9f, 1.25f);
            Prim.Sphere("Core", body.transform, new Vector3(0f, 0f, 0.05f), 0.6f, MatLib.SlimeDark, 0.75f);
            Prim.Sphere("Vein", body.transform, new Vector3(0.15f, 0.1f, 0.2f), 0.45f, MatLib.SlimeGlow, 0.4f);

            // 각진 등갑
            for (var i = 0; i < 5; i++)
            {
                var spike = Prim.Cube($"Plate{i}", body.transform,
                    new Vector3((i - 2) * 0.16f, 0.35f, -0.2f - i * 0.02f),
                    new Vector3(0.14f, 0.08f, 0.35f), MatLib.MetalDark, 0.4f);
                spike.transform.localRotation = Quaternion.Euler(-40f, 0f, 0f);
            }

            Prim.Cube("Brow", body.transform, new Vector3(0f, 0.28f, 0.42f), new Vector3(0.65f, 0.1f, 0.18f),
                MatLib.SlimeDark, 0.3f);

            var eyeL = Prim.Sphere("EyeL", body.transform, new Vector3(-0.26f, 0.16f, 0.5f), 0.2f, MatLib.EyeRed, 0.7f);
            var eyeR = Prim.Sphere("EyeR", body.transform, new Vector3(0.26f, 0.16f, 0.5f), 0.2f, MatLib.EyeRed, 0.7f);
            Prim.Sphere("PupilL", eyeL.transform, new Vector3(0f, 0f, 0.35f), 0.4f, MatLib.Black, 0.2f);
            Prim.Sphere("PupilR", eyeR.transform, new Vector3(0f, 0f, 0.35f), 0.4f, MatLib.Black, 0.2f);

            var jaw = Prim.Bone("Jaw", body.transform, new Vector3(0f, -0.08f, 0.38f));
            Prim.Cube("JawMesh", jaw, new Vector3(0f, -0.1f, 0.1f), new Vector3(0.65f, 0.16f, 0.4f),
                MatLib.SlimeDark, 0.35f);
            for (var i = -2; i <= 2; i++)
            {
                Prim.Cube($"FangU{i}", body.transform, new Vector3(i * 0.1f, 0.0f, 0.55f),
                    new Vector3(0.05f, 0.16f, 0.05f), MatLib.Bone, 0.25f);
                Prim.Cube($"FangL{i}", jaw, new Vector3(i * 0.1f, 0.0f, 0.22f),
                    new Vector3(0.05f, 0.12f, 0.05f), MatLib.Bone, 0.25f);
            }

            var tentL = Prim.Bone("TentacleL", root.transform, new Vector3(-0.58f, 0.5f, 0.05f));
            Prim.Cap("TentL", tentL, Vector3.zero, new Vector3(0.18f, 0.4f, 0.18f), MatLib.Slime, 0.5f);
            Prim.Cube("ClawL", tentL, new Vector3(-0.05f, -0.42f, 0.2f), new Vector3(0.1f, 0.08f, 0.28f),
                MatLib.Bone, 0.3f);

            var tentR = Prim.Bone("TentacleR", root.transform, new Vector3(0.58f, 0.5f, 0.05f));
            Prim.Cap("TentR", tentR, Vector3.zero, new Vector3(0.18f, 0.4f, 0.18f), MatLib.Slime, 0.5f);
            Prim.Cube("ClawR", tentR, new Vector3(0.05f, -0.42f, 0.2f), new Vector3(0.1f, 0.08f, 0.28f),
                MatLib.Bone, 0.3f);

            Prim.Cyl("Shadow", root.transform, new Vector3(0f, 0.015f, 0f), new Vector3(1.15f, 0.008f, 1.15f),
                new Color(0f, 0f, 0f, 0.5f), 0.05f);

            // 희미한 보라 오라
            var aura = Prim.Sphere("Aura", root.transform, new Vector3(0f, 0.7f, 0f), 1.6f,
                new Color(0.35f, 0.1f, 0.25f, 0.15f), 0.1f);
            Object.Destroy(aura.GetComponent<Collider>());

            FinishMonster(root, procedural: true, body.transform, jaw, tentL, tentR, eyeL.transform, eyeR.transform);
            return root;
        }

        private static void FinishMonster(GameObject root, bool procedural,
            Transform body = null, Transform jaw = null, Transform tentL = null, Transform tentR = null,
            Transform eyeL = null, Transform eyeR = null)
        {
            CharacterPhysics.Apply3D(root, radius: 0.6f, height: 1.25f, center: new Vector3(0f, 0.6f, 0f),
                mass: 1.15f);
            var enemy = LayerMask.NameToLayer("Enemy");
            if (enemy >= 0)
            {
                root.layer = enemy;
            }

            var health = root.AddComponent<Health>();
            health.Initialize(36, 36);
            var reactor = root.AddComponent<CombatReactor>();
            reactor.Configure3D(player: false, knockback: 4.5f);
            root.AddComponent<MonsterBrain>();

            if (procedural && body != null)
            {
                var anim = root.AddComponent<MonsterAnimator>();
                anim.Body = body;
                anim.Jaw = jaw;
                anim.TentacleL = tentL;
                anim.TentacleR = tentR;
                anim.EyeL = eyeL;
                anim.EyeR = eyeR;
                anim.Bind();
            }
            else
            {
                root.AddComponent<ModelBob>();
            }

            WorldHpBar.Attach(root, yOffset: 1.75f, width: 1.1f);
            // "적" 월드 글자 제거 — HP 바로 구분 (글자 겹침 방지)
        }

        public static GameObject CreateNpc(Vector3 position, Transform parent = null)
        {
            var root = new GameObject("VillageGuide");
            if (parent != null)
            {
                root.transform.SetParent(parent, false);
            }

            root.transform.position = position;

            // Mira — 가녀린 안내자, 남빛 로브·은발·지팡이
            var robe = MatLib.NpcRobe;
            var trim = MatLib.NpcTrim;
            var hair = new Color(0.72f, 0.7f, 0.78f);
            var skin = new Color(0.82f, 0.68f, 0.58f);

            var visual = Prim.Bone("Visual", root.transform, Vector3.zero);
            var hip = Prim.Bone("Hip", visual, new Vector3(0f, 0.96f, 0f));
            Prim.Cube("Pelvis", hip, new Vector3(0f, 0.02f, 0f), new Vector3(0.3f, 0.12f, 0.2f), robe, 0.2f);

            var legL = Prim.Bone("LegL", hip, new Vector3(-0.1f, -0.02f, 0f));
            Prim.Cap("ThighL", legL, new Vector3(0f, -0.22f, 0f), new Vector3(0.14f, 0.26f, 0.14f), robe, 0.18f);
            Prim.Cap("CalfL", legL, new Vector3(0f, -0.52f, 0.01f), new Vector3(0.12f, 0.24f, 0.12f), robe, 0.18f);
            Prim.Cube("BootL", legL, new Vector3(0f, -0.82f, 0.04f), new Vector3(0.15f, 0.1f, 0.26f), MatLib.Boot, 0.15f);

            var legR = Prim.Bone("LegR", hip, new Vector3(0.1f, -0.02f, 0f));
            Prim.Cap("ThighR", legR, new Vector3(0f, -0.22f, 0f), new Vector3(0.14f, 0.26f, 0.14f), robe, 0.18f);
            Prim.Cap("CalfR", legR, new Vector3(0f, -0.52f, 0.01f), new Vector3(0.12f, 0.24f, 0.12f), robe, 0.18f);
            Prim.Cube("BootR", legR, new Vector3(0f, -0.82f, 0.04f), new Vector3(0.15f, 0.1f, 0.26f), MatLib.Boot, 0.15f);

            // 스커트 (다리 위 얇은 천 — 몸통 캡슐 대신)
            Prim.Cube("Skirt", hip, new Vector3(0f, -0.15f, 0f), new Vector3(0.42f, 0.55f, 0.28f), robe, 0.15f);
            Prim.Cube("SkirtTrim", hip, new Vector3(0f, -0.4f, 0.02f), new Vector3(0.44f, 0.06f, 0.3f), trim, 0.4f);

            var chest = Prim.Bone("Chest", hip, new Vector3(0f, 0.28f, 0f));
            Prim.Cap("Torso", chest, new Vector3(0f, 0.18f, 0f), new Vector3(0.32f, 0.32f, 0.22f), robe, 0.2f);
            Prim.Cube("Bodice", chest, new Vector3(0f, 0.22f, 0.08f), new Vector3(0.3f, 0.28f, 0.1f),
                new Color(0.18f, 0.16f, 0.26f), 0.25f);
            Prim.Cube("Sash", chest, new Vector3(0f, 0.02f, 0.12f), new Vector3(0.34f, 0.07f, 0.08f), trim, 0.45f);
            Prim.Cube("Collar", chest, new Vector3(0f, 0.38f, 0.06f), new Vector3(0.26f, 0.07f, 0.18f), trim, 0.4f);
            Prim.Cube("Capelet", chest, new Vector3(0f, 0.2f, -0.14f), new Vector3(0.4f, 0.35f, 0.05f), robe, 0.15f);

            var armL = Prim.Bone("ArmL", chest, new Vector3(-0.26f, 0.3f, 0f));
            Prim.Sphere("ShoulderL", armL, Vector3.zero, 0.11f, robe, 0.2f);
            Prim.Cap("UpperArmL", armL, new Vector3(0f, -0.16f, 0f), new Vector3(0.11f, 0.18f, 0.11f), robe, 0.18f);
            var foreL = Prim.Bone("ForeArmL", armL, new Vector3(0f, -0.34f, 0.02f));
            Prim.Cap("ForeArmMeshL", foreL, new Vector3(0f, -0.14f, 0f), new Vector3(0.1f, 0.16f, 0.1f), robe, 0.18f);
            var handL = Prim.Bone("HandL", foreL, new Vector3(0f, -0.3f, 0.02f));
            Prim.Sphere("HandMeshL", handL, Vector3.zero, 0.1f, skin, 0.25f);

            var armR = Prim.Bone("ArmR", chest, new Vector3(0.26f, 0.3f, 0f));
            Prim.Sphere("ShoulderR", armR, Vector3.zero, 0.11f, robe, 0.2f);
            Prim.Cap("UpperArmR", armR, new Vector3(0f, -0.16f, 0f), new Vector3(0.11f, 0.18f, 0.11f), robe, 0.18f);
            var foreR = Prim.Bone("ForeArmR", armR, new Vector3(0f, -0.34f, 0.02f));
            Prim.Cap("ForeArmMeshR", foreR, new Vector3(0f, -0.14f, 0f), new Vector3(0.1f, 0.16f, 0.1f), robe, 0.18f);
            var handR = Prim.Bone("HandR", foreR, new Vector3(0f, -0.3f, 0.02f));
            Prim.Sphere("HandMeshR", handR, Vector3.zero, 0.1f, skin, 0.25f);

            // 지팡이 — 손 위(+Y)로 오르브가 올라감
            var staff = Prim.Bone("Staff", handR, new Vector3(0.02f, -0.04f, 0.05f));
            staff.localRotation = Quaternion.Euler(8f, 0f, -12f);
            Prim.Cyl("StaffPole", staff, new Vector3(0f, 0.45f, 0f), new Vector3(0.04f, 0.7f, 0.04f),
                MatLib.Wood, 0.2f);
            var orb = Prim.Sphere("Orb", staff, new Vector3(0f, 1.15f, 0f), 0.16f, MatLib.AccentTeal, 0.75f);
            var orbLight = orb.AddComponent<Light>();
            orbLight.type = LightType.Point;
            orbLight.color = new Color(0.45f, 0.75f, 0.9f);
            orbLight.range = 3.2f;
            orbLight.intensity = 0.85f;

            var neck = Prim.Bone("Neck", chest, new Vector3(0f, 0.48f, 0f));
            Prim.Cyl("NeckMesh", neck, Vector3.zero, new Vector3(0.08f, 0.06f, 0.08f), skin, 0.25f);
            var head = Prim.Bone("Head", neck, new Vector3(0f, 0.15f, 0f));
            Prim.Sphere("Skull", head, Vector3.zero, 0.26f, skin, 0.3f);
            Prim.Sphere("HairMain", head, new Vector3(0f, 0.08f, -0.02f), 0.28f, hair, 0.12f);
            Prim.Sphere("HairSideL", head, new Vector3(-0.1f, 0.02f, 0f), 0.14f, hair, 0.12f);
            Prim.Sphere("HairSideR", head, new Vector3(0.1f, 0.02f, 0f), 0.14f, hair, 0.12f);
            Prim.Cube("Brow", head, new Vector3(0f, 0.05f, 0.11f), new Vector3(0.16f, 0.02f, 0.04f), hair, 0.1f);
            Prim.Sphere("EyeL", head, new Vector3(-0.055f, 0.02f, 0.115f), 0.04f, MatLib.EyeSteel, 0.5f);
            Prim.Sphere("EyeR", head, new Vector3(0.055f, 0.02f, 0.115f), 0.04f, MatLib.EyeSteel, 0.5f);
            Prim.Sphere("PupilL", head, new Vector3(-0.055f, 0.02f, 0.135f), 0.02f, MatLib.Black, 0.2f);
            Prim.Sphere("PupilR", head, new Vector3(0.055f, 0.02f, 0.135f), 0.02f, MatLib.Black, 0.2f);
            Prim.Sphere("Nose", head, new Vector3(0f, -0.015f, 0.13f), 0.035f, new Color(0.75f, 0.6f, 0.52f), 0.25f);
            Prim.Cube("Mouth", head, new Vector3(0f, -0.06f, 0.12f), new Vector3(0.08f, 0.02f, 0.03f),
                new Color(0.55f, 0.35f, 0.35f), 0.2f);

            Prim.Cyl("Shadow", visual, new Vector3(0f, 0.015f, 0f), new Vector3(0.65f, 0.008f, 0.65f),
                new Color(0f, 0f, 0f, 0.4f), 0.05f);

            FinishNpc(root, hip, chest, head, legL, legR, armL, armR, foreL, foreR, handL, handR);
            return root;
        }

        /// <summary>대장장이 Borin — 건장한 체형·앞치마·수염·망치.</summary>
        public static GameObject CreateBlacksmith(Vector3 position, Transform parent = null)
        {
            var root = new GameObject("Blacksmith");
            if (parent != null)
            {
                root.transform.SetParent(parent, false);
            }

            root.transform.position = position;

            var leather = new Color(0.32f, 0.22f, 0.16f);
            var apron = new Color(0.5f, 0.4f, 0.28f);
            var beard = new Color(0.42f, 0.28f, 0.18f);
            var skin = new Color(0.78f, 0.58f, 0.48f);

            var visual = Prim.Bone("Visual", root.transform, Vector3.zero);
            var hip = Prim.Bone("Hip", visual, new Vector3(0f, 0.95f, 0f));
            Prim.Cube("Pelvis", hip, new Vector3(0f, 0.02f, 0f), new Vector3(0.4f, 0.14f, 0.26f), leather, 0.25f);

            var legL = Prim.Bone("LegL", hip, new Vector3(-0.13f, -0.02f, 0f));
            Prim.Cap("ThighL", legL, new Vector3(0f, -0.24f, 0f), new Vector3(0.18f, 0.28f, 0.18f), leather, 0.2f);
            Prim.Cap("CalfL", legL, new Vector3(0f, -0.56f, 0.01f), new Vector3(0.16f, 0.26f, 0.16f), leather, 0.2f);
            Prim.Cube("BootL", legL, new Vector3(0f, -0.86f, 0.05f), new Vector3(0.2f, 0.12f, 0.32f), MatLib.Boot, 0.15f);

            var legR = Prim.Bone("LegR", hip, new Vector3(0.13f, -0.02f, 0f));
            Prim.Cap("ThighR", legR, new Vector3(0f, -0.24f, 0f), new Vector3(0.18f, 0.28f, 0.18f), leather, 0.2f);
            Prim.Cap("CalfR", legR, new Vector3(0f, -0.56f, 0.01f), new Vector3(0.16f, 0.26f, 0.16f), leather, 0.2f);
            Prim.Cube("BootR", legR, new Vector3(0f, -0.86f, 0.05f), new Vector3(0.2f, 0.12f, 0.32f), MatLib.Boot, 0.15f);

            var chest = Prim.Bone("Chest", hip, new Vector3(0f, 0.3f, 0f));
            Prim.Cap("Torso", chest, new Vector3(0f, 0.2f, 0f), new Vector3(0.48f, 0.38f, 0.32f), leather, 0.22f);
            Prim.Cube("Shirt", chest, new Vector3(0f, 0.22f, 0.1f), new Vector3(0.42f, 0.3f, 0.12f),
                new Color(0.28f, 0.2f, 0.16f), 0.2f);
            Prim.Cube("Apron", chest, new Vector3(0f, -0.08f, 0.16f), new Vector3(0.38f, 0.7f, 0.06f), apron, 0.25f);
            Prim.Cube("ApronStrapL", chest, new Vector3(-0.14f, 0.28f, 0.12f), new Vector3(0.06f, 0.35f, 0.04f),
                apron, 0.25f);
            Prim.Cube("ApronStrapR", chest, new Vector3(0.14f, 0.28f, 0.12f), new Vector3(0.06f, 0.35f, 0.04f),
                apron, 0.25f);
            Prim.Cube("Belt", hip, new Vector3(0f, 0.08f, 0.12f), new Vector3(0.44f, 0.08f, 0.1f), MatLib.Belt, 0.3f);
            Prim.Cube("PauldronL", chest, new Vector3(-0.32f, 0.34f, 0f), new Vector3(0.18f, 0.14f, 0.22f),
                MatLib.MetalDark, 0.5f);
            Prim.Cube("PauldronR", chest, new Vector3(0.32f, 0.34f, 0f), new Vector3(0.18f, 0.14f, 0.22f),
                MatLib.MetalDark, 0.5f);

            var armL = Prim.Bone("ArmL", chest, new Vector3(-0.34f, 0.3f, 0f));
            Prim.Sphere("ShoulderL", armL, Vector3.zero, 0.14f, skin, 0.25f);
            Prim.Cap("UpperArmL", armL, new Vector3(0f, -0.18f, 0f), new Vector3(0.15f, 0.2f, 0.15f), skin, 0.25f);
            var foreL = Prim.Bone("ForeArmL", armL, new Vector3(0f, -0.38f, 0.02f));
            Prim.Cap("ForeArmMeshL", foreL, new Vector3(0f, -0.16f, 0f), new Vector3(0.13f, 0.18f, 0.13f), skin, 0.25f);
            var handL = Prim.Bone("HandL", foreL, new Vector3(0f, -0.32f, 0.02f));
            Prim.Sphere("HandMeshL", handL, Vector3.zero, 0.12f, skin, 0.25f);

            var armR = Prim.Bone("ArmR", chest, new Vector3(0.34f, 0.3f, 0f));
            Prim.Sphere("ShoulderR", armR, Vector3.zero, 0.14f, skin, 0.25f);
            Prim.Cap("UpperArmR", armR, new Vector3(0f, -0.18f, 0f), new Vector3(0.15f, 0.2f, 0.15f), skin, 0.25f);
            var foreR = Prim.Bone("ForeArmR", armR, new Vector3(0f, -0.38f, 0.02f));
            Prim.Cap("ForeArmMeshR", foreR, new Vector3(0f, -0.16f, 0f), new Vector3(0.13f, 0.18f, 0.13f), skin, 0.25f);
            var handR = Prim.Bone("HandR", foreR, new Vector3(0f, -0.32f, 0.02f));
            Prim.Sphere("HandMeshR", handR, Vector3.zero, 0.12f, skin, 0.25f);

            var hammer = Prim.Bone("Hammer", handR, new Vector3(0.03f, -0.02f, 0.06f));
            hammer.localRotation = Quaternion.Euler(10f, 0f, -20f);
            Prim.Cyl("Handle", hammer, new Vector3(0f, -0.2f, 0f), new Vector3(0.045f, 0.32f, 0.045f),
                MatLib.Wood, 0.2f);
            Prim.Cube("Head", hammer, new Vector3(0f, -0.55f, 0f), new Vector3(0.28f, 0.14f, 0.16f),
                MatLib.Metal, 0.65f);

            var neck = Prim.Bone("Neck", chest, new Vector3(0f, 0.5f, 0f));
            Prim.Cyl("NeckMesh", neck, Vector3.zero, new Vector3(0.1f, 0.07f, 0.1f), skin, 0.25f);
            var head = Prim.Bone("Head", neck, new Vector3(0f, 0.16f, 0f));
            Prim.Sphere("Skull", head, Vector3.zero, 0.28f, skin, 0.3f);
            Prim.Sphere("Hair", head, new Vector3(0f, 0.08f, -0.04f), 0.26f, beard, 0.12f);
            Prim.Sphere("BeardMain", head, new Vector3(0f, -0.1f, 0.08f), 0.2f, beard, 0.15f);
            Prim.Sphere("BeardTip", head, new Vector3(0f, -0.2f, 0.1f), 0.12f, beard, 0.15f);
            Prim.Cube("Brow", head, new Vector3(0f, 0.06f, 0.12f), new Vector3(0.18f, 0.035f, 0.05f), beard, 0.1f);
            Prim.Sphere("EyeL", head, new Vector3(-0.06f, 0.02f, 0.12f), 0.04f, MatLib.EyeSteel, 0.45f);
            Prim.Sphere("EyeR", head, new Vector3(0.06f, 0.02f, 0.12f), 0.04f, MatLib.EyeSteel, 0.45f);
            Prim.Sphere("PupilL", head, new Vector3(-0.06f, 0.02f, 0.14f), 0.02f, MatLib.Black, 0.2f);
            Prim.Sphere("PupilR", head, new Vector3(0.06f, 0.02f, 0.14f), 0.02f, MatLib.Black, 0.2f);
            Prim.Sphere("Nose", head, new Vector3(0f, -0.02f, 0.14f), 0.045f, new Color(0.7f, 0.5f, 0.42f), 0.25f);

            Prim.Cyl("Shadow", visual, new Vector3(0f, 0.015f, 0f), new Vector3(0.8f, 0.008f, 0.8f),
                new Color(0f, 0f, 0f, 0.4f), 0.05f);

            FinishNpc(root, hip, chest, head, legL, legR, armL, armR, foreL, foreR, handL, handR,
                radius: 0.42f, height: 1.9f, mass: 2.2f);
            return root;
        }

        private static void FinishNpc(GameObject root, Transform hip, Transform chest, Transform head,
            Transform legL, Transform legR, Transform armL, Transform armR,
            Transform foreL, Transform foreR, Transform handL, Transform handR,
            float radius = 0.36f, float height = 1.85f, float mass = 1.8f)
        {
            CharacterPhysics.Apply3D(root, radius, height, new Vector3(0f, height * 0.5f, 0f), mass);
            var rb = root.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }

            var anim = root.AddComponent<HumanoidAnimator>();
            anim.Hip = hip;
            anim.Chest = chest;
            anim.Head = head;
            anim.LegL = legL;
            anim.LegR = legR;
            anim.ArmL = armL;
            anim.ArmR = armR;
            anim.ForeArmL = foreL;
            anim.ForeArmR = foreR;
            anim.HandL = handL;
            anim.HandR = handR;
            anim.Bind();
        }
    }
}
