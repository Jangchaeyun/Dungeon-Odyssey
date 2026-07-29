using DungeonOdyssey.Player;
using UnityEngine;

namespace DungeonOdyssey.View3D
{
    /// <summary>
    /// 손 본에 붙는 검·방패. 무기 타입별 비주얼 교체 지원.
    /// </summary>
    public static class EquipmentBuilder
    {
        public static Transform BuildSword(Transform hand) =>
            BuildWeapon(hand, WeaponId.IronSword, 0);

        public static Transform BuildWeapon(Transform hand, WeaponId id, int upgradeLevel)
        {
            // 손바닥 중앙에 그립 · 칼날은 손 앞(+Z)으로 뻗음 (본 Y- = 칼끝)
            var sword = Prim.Bone("Sword", hand, new Vector3(0.01f, -0.05f, 0.04f));
            sword.localRotation = Quaternion.Euler(-78f, 90f, 8f);

            var (bladeCol, edgeCol, tipCol, guardCol, gripCol) = Palette(id, upgradeLevel);
            var def = WeaponCatalog.Get(id);
            var tier = WeaponTier(id);
            var lengthBoost = 1f + upgradeLevel * 0.035f + tier * 0.05f;
            var isScythe = id is WeaponId.VoidReaper or WeaponId.MoonScythe;
            var isHeavy = id is WeaponId.StoneMaul or WeaponId.TitanHammer or WeaponId.ObsidianGreatsword
                or WeaponId.CrimsonAxe or WeaponId.GlacierAxe;
            var bladeH = (isScythe ? 1.15f : isHeavy ? 1.08f : 1.0f) * lengthBoost;
            var bladeCenterY = -(0.55f * lengthBoost);
            var tipY = -(1.12f * lengthBoost);
            var guardY = -0.14f;
            var pommelY = 0.13f;
            var bladeThick = def.Element == WeaponElement.Storm || isHeavy ? 0.065f : 0.05f;
            var guardW = 0.34f + tier * 0.035f;

            Prim.Cyl("Grip", sword, new Vector3(0f, 0f, 0f), new Vector3(0.048f, 0.11f, 0.048f), gripCol, 0.25f);
            Prim.Sphere("Pommel", sword, new Vector3(0f, pommelY, 0f), 0.085f, MatLib.GoldTrim, 0.55f);
            Prim.Cube("Guard", sword, new Vector3(0f, guardY, 0f),
                new Vector3(guardW, 0.05f, 0.08f), guardCol, 0.7f);

            Prim.Cube("Blade", sword, new Vector3(0f, bladeCenterY, 0f), new Vector3(bladeThick, bladeH, 0.028f),
                bladeCol, 0.92f);
            Prim.Cube("Edge", sword, new Vector3(0f, bladeCenterY, 0.012f),
                new Vector3(0.016f, bladeH * 0.92f, 0.014f), edgeCol, 0.95f);
            Prim.Cube("Tip", sword, new Vector3(0f, tipY, 0f), new Vector3(0.038f, 0.11f, 0.022f),
                tipCol, 0.9f);

            switch (def.Element)
            {
                case WeaponElement.Flame:
                    Prim.Cube("Ember", sword, new Vector3(0f, bladeCenterY, -0.018f),
                        new Vector3(0.028f, bladeH * 0.82f, 0.018f),
                        new Color(1f, 0.35f, 0.1f), 0.4f);
                    break;
                case WeaponElement.Frost:
                    Prim.Cube("Frost", sword, new Vector3(0f, bladeCenterY, -0.016f),
                        new Vector3(0.022f, bladeH * 0.75f, 0.016f),
                        new Color(0.55f, 0.85f, 1f), 0.55f);
                    break;
                case WeaponElement.Storm:
                    Prim.Cube("Spark", sword, new Vector3(0f, bladeCenterY * 0.7f, 0.02f),
                        new Vector3(0.02f, 0.35f, 0.02f),
                        new Color(0.7f, 0.85f, 1f), 0.7f);
                    break;
                case WeaponElement.Void:
                    Prim.Cube("VoidVein", sword, new Vector3(0f, bladeCenterY, -0.02f),
                        new Vector3(0.03f, bladeH * 0.9f, 0.02f),
                        new Color(0.35f, 0.12f, 0.45f), 0.45f);
                    break;
                case WeaponElement.Holy:
                    Prim.Cube("Halo", sword, new Vector3(0f, bladeCenterY * 0.5f, 0.02f),
                        new Vector3(0.024f, 0.28f, 0.016f),
                        new Color(1f, 0.95f, 0.7f), 0.55f);
                    break;
                case WeaponElement.Nature:
                    Prim.Cube("Leaf", sword, new Vector3(0f, bladeCenterY, -0.016f),
                        new Vector3(0.022f, bladeH * 0.7f, 0.014f),
                        new Color(0.35f, 0.75f, 0.4f), 0.45f);
                    break;
                case WeaponElement.Blood:
                    Prim.Cube("Blood", sword, new Vector3(0f, bladeCenterY, -0.018f),
                        new Vector3(0.026f, bladeH * 0.8f, 0.016f),
                        new Color(0.65f, 0.1f, 0.18f), 0.4f);
                    break;
                case WeaponElement.Earth:
                    Prim.Cube("Stone", sword, new Vector3(0f, bladeCenterY * 0.85f, 0.018f),
                        new Vector3(0.04f, 0.22f, 0.02f),
                        new Color(0.55f, 0.48f, 0.35f), 0.5f);
                    break;
            }

            if (upgradeLevel >= 3)
            {
                Prim.Cube("Rune", sword, new Vector3(0f, bladeCenterY * 0.65f, 0.018f),
                    new Vector3(0.028f, 0.18f, 0.018f), MatLib.AccentTeal, 0.6f);
            }

            if (upgradeLevel >= 6)
            {
                Prim.Cube("RuneHi", sword, new Vector3(0f, bladeCenterY * 0.35f, 0.018f),
                    new Vector3(0.022f, 0.12f, 0.016f), MatLib.GoldTrim, 0.7f);
            }

            var trailTip = Prim.Bone("TrailTip", sword, new Vector3(0f, tipY - 0.04f, 0f));
            var trailRoot = Prim.Bone("TrailRoot", sword, new Vector3(0f, guardY - 0.08f, 0f));
            trailTip.gameObject.AddComponent<WeaponTrail>().Configure(trailTip, trailRoot);

            return sword;
        }

        private static int WeaponTier(WeaponId id)
        {
            var lv = WeaponCatalog.Get(id).UnlockLevel;
            return Mathf.Clamp((lv - 1) / 3, 0, 5);
        }

        private static (Color blade, Color edge, Color tip, Color guard, Color grip) Palette(WeaponId id,
            int upgradeLevel)
        {
            var shine = Mathf.Clamp01(upgradeLevel * 0.06f);
            var el = WeaponCatalog.Get(id).Element;
            return el switch
            {
                WeaponElement.Steel => (
                    Color.Lerp(new Color(0.65f, 0.72f, 0.82f), Color.white, shine),
                    new Color(0.9f, 0.95f, 1f),
                    new Color(0.75f, 0.8f, 0.9f),
                    MatLib.Metal,
                    new Color(0.28f, 0.18f, 0.12f)),
                WeaponElement.Frost => (
                    Color.Lerp(new Color(0.55f, 0.72f, 0.88f), new Color(0.8f, 0.92f, 1f), shine),
                    new Color(0.7f, 0.95f, 1f),
                    new Color(0.85f, 0.95f, 1f),
                    new Color(0.4f, 0.55f, 0.7f),
                    new Color(0.2f, 0.28f, 0.35f)),
                WeaponElement.Flame => (
                    Color.Lerp(new Color(0.75f, 0.25f, 0.15f), new Color(1f, 0.45f, 0.2f), shine),
                    new Color(1f, 0.75f, 0.3f),
                    new Color(1f, 0.4f, 0.15f),
                    new Color(0.35f, 0.15f, 0.12f),
                    new Color(0.2f, 0.1f, 0.08f)),
                WeaponElement.Storm => (
                    Color.Lerp(new Color(0.45f, 0.55f, 0.75f), new Color(0.7f, 0.8f, 1f), shine),
                    new Color(0.85f, 0.9f, 1f),
                    new Color(0.95f, 0.95f, 1f),
                    new Color(0.3f, 0.35f, 0.5f),
                    new Color(0.15f, 0.15f, 0.22f)),
                WeaponElement.Void => (
                    Color.Lerp(new Color(0.28f, 0.12f, 0.35f), new Color(0.5f, 0.25f, 0.6f), shine),
                    new Color(0.7f, 0.4f, 0.85f),
                    new Color(0.55f, 0.3f, 0.7f),
                    new Color(0.2f, 0.1f, 0.25f),
                    new Color(0.12f, 0.08f, 0.14f)),
                WeaponElement.Holy => (
                    Color.Lerp(new Color(0.9f, 0.85f, 0.65f), new Color(1f, 0.96f, 0.8f), shine),
                    new Color(1f, 0.98f, 0.85f),
                    new Color(0.95f, 0.9f, 0.7f),
                    new Color(0.55f, 0.5f, 0.35f),
                    new Color(0.35f, 0.28f, 0.18f)),
                WeaponElement.Nature => (
                    Color.Lerp(new Color(0.35f, 0.55f, 0.32f), new Color(0.5f, 0.75f, 0.45f), shine),
                    new Color(0.7f, 0.95f, 0.55f),
                    new Color(0.45f, 0.7f, 0.4f),
                    new Color(0.3f, 0.4f, 0.25f),
                    new Color(0.22f, 0.16f, 0.1f)),
                WeaponElement.Blood => (
                    Color.Lerp(new Color(0.55f, 0.12f, 0.18f), new Color(0.85f, 0.2f, 0.28f), shine),
                    new Color(1f, 0.4f, 0.45f),
                    new Color(0.7f, 0.15f, 0.2f),
                    new Color(0.3f, 0.1f, 0.12f),
                    new Color(0.18f, 0.08f, 0.08f)),
                WeaponElement.Earth => (
                    Color.Lerp(new Color(0.5f, 0.42f, 0.32f), new Color(0.7f, 0.6f, 0.45f), shine),
                    new Color(0.85f, 0.75f, 0.55f),
                    new Color(0.6f, 0.52f, 0.4f),
                    new Color(0.35f, 0.3f, 0.25f),
                    new Color(0.25f, 0.18f, 0.12f)),
                _ => (
                    Color.Lerp(new Color(0.78f, 0.82f, 0.9f), Color.white, shine),
                    new Color(0.95f, 0.97f, 1f),
                    new Color(0.88f, 0.9f, 0.96f),
                    MatLib.MetalDark,
                    new Color(0.35f, 0.22f, 0.12f))
            };
        }

        public static Transform BuildShield(Transform handL)
        {
            // 원기둥 기본면이 Y — 세워 전방을 보게 하고, 왼팔 바깥쪽에 붙임
            var shield = Prim.Bone("Shield", handL, new Vector3(-0.1f, 0.02f, 0.06f));
            shield.localRotation = Quaternion.Euler(88f, 8f, -12f);

            Prim.Cyl("ShieldDisc", shield, new Vector3(0f, 0f, 0f), new Vector3(0.48f, 0.035f, 0.48f),
                new Color(0.4f, 0.32f, 0.22f), 0.3f);
            Prim.Cyl("ShieldBoss", shield, new Vector3(0f, 0.045f, 0f), new Vector3(0.15f, 0.045f, 0.15f),
                MatLib.Metal, 0.65f);
            Prim.Cyl("ShieldRim", shield, new Vector3(0f, 0f, 0f), new Vector3(0.52f, 0.022f, 0.52f),
                MatLib.GoldTrim, 0.5f);
            return shield;
        }

        public static void EnsurePlayerGear(GameObject playerRoot, Transform armR, Transform armL,
            out Transform sword, out Transform shield)
        {
            sword = FindDeep(playerRoot.transform, "Sword");
            shield = FindDeep(playerRoot.transform, "Shield");

            if (sword == null)
            {
                var handR = FindDeep(armR != null ? armR : playerRoot.transform, "HandR")
                            ?? FindDeep(playerRoot.transform, "HandR");
                if (handR == null)
                {
                    handR = Prim.Bone("HandR", armR != null ? armR : playerRoot.transform,
                        new Vector3(0.35f, 1.0f, 0.2f));
                }

                sword = BuildSword(handR);
            }

            if (shield == null)
            {
                var handL = FindDeep(armL != null ? armL : playerRoot.transform, "HandL")
                            ?? FindDeep(playerRoot.transform, "HandL");
                if (handL != null)
                {
                    shield = BuildShield(handL);
                }
            }
        }

        public static Transform ApplyWeaponVisual(GameObject playerRoot, WeaponId id, int upgradeLevel)
        {
            if (playerRoot == null)
            {
                return null;
            }

            var old = FindDeep(playerRoot.transform, "Sword");
            Transform handR = null;
            if (old != null)
            {
                handR = old.parent;
                Object.Destroy(old.gameObject);
            }

            handR ??= FindDeep(playerRoot.transform, "HandR");
            if (handR == null)
            {
                return null;
            }

            var sword = BuildWeapon(handR, id, upgradeLevel);
            var anim = playerRoot.GetComponent<HumanoidAnimator>();
            if (anim != null)
            {
                anim.RefreshSword(sword);
            }

            return sword;
        }

        private static Transform FindDeep(Transform root, string name)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == name)
            {
                return root;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var found = FindDeep(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
