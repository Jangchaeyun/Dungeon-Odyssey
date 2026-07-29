using DungeonOdyssey.Dungeon;
using UnityEngine;

namespace DungeonOdyssey.View3D
{
    public static class EnvironmentFactory3D
    {
        public static void BuildTown(Transform root)
        {
            BuildTownGround(root);

            // —— 서쪽 거주 구역 ——
            BuildHouse(root, new Vector3(-18f, 0f, 6.5f), new Vector3(5.4f, 3.4f, 4.4f), MatLib.Roof, lit: true);
            BuildHouse(root, new Vector3(-19.5f, 0f, -5.5f), new Vector3(4.8f, 3f, 3.8f),
                new Color(0.22f, 0.14f, 0.12f), lit: true);
            BuildHouse(root, new Vector3(-13.5f, 0f, 9.5f), new Vector3(4.2f, 2.9f, 3.5f), MatLib.StoneDark);
            BuildHouse(root, new Vector3(-14.2f, 0f, -9.2f), new Vector3(4f, 2.7f, 3.4f), MatLib.Roof, lit: true);

            // —— 북쪽 · 남쪽 주택 ——
            BuildHouse(root, new Vector3(-4f, 0f, 13.5f), new Vector3(5f, 3.1f, 4f),
                new Color(0.2f, 0.18f, 0.16f), lit: true);
            BuildHouse(root, new Vector3(4.5f, 0f, 12.8f), new Vector3(4.6f, 3f, 3.6f), MatLib.StoneDark);
            BuildHouse(root, new Vector3(-5.5f, 0f, -12.5f), new Vector3(4.4f, 2.8f, 3.5f), MatLib.Roof);
            BuildHouse(root, new Vector3(5f, 0f, -13f), new Vector3(4.2f, 2.9f, 3.4f),
                new Color(0.24f, 0.16f, 0.14f), lit: true);

            // —— 동쪽(던전 길목) ——
            BuildHouse(root, new Vector3(12.5f, 0f, 8.5f), new Vector3(4.5f, 3f, 3.6f), MatLib.StoneDark, lit: true);
            BuildHouse(root, new Vector3(13f, 0f, -8.8f), new Vector3(4.2f, 2.8f, 3.4f), MatLib.Roof);

            // 시장 가판 — 길에서 비켜 배치
            BuildStall(root, new Vector3(-9.5f, 0f, -3.2f), MatLib.AccentWine, 0f);
            BuildStall(root, new Vector3(-7.2f, 0f, 4.2f), MatLib.AccentTeal, 180f);
            BuildStall(root, new Vector3(6.8f, 0f, 4.5f), MatLib.AccentCopper, 180f);
            BuildStall(root, new Vector3(8.2f, 0f, -3.8f), new Color(0.4f, 0.28f, 0.22f), 0f);

            BuildWell(root, new Vector3(-1.5f, 0f, -2.4f));

            // 울타리 — 넓은 경계
            BuildFence(root, new Vector3(-24f, 0f, 0f), 14, true);
            BuildFence(root, new Vector3(24f, 0f, 0f), 14, true);
            BuildFence(root, new Vector3(0f, 0f, 16f), 16, false);
            BuildFence(root, new Vector3(0f, 0f, -16f), 16, false);

            // 수목 · 관목
            BuildTree(root, new Vector3(-22f, 0f, -4f), 1.05f);
            BuildTree(root, new Vector3(-20f, 0f, 10f), 0.9f);
            BuildTree(root, new Vector3(-10f, 0f, 14f), 1.1f);
            BuildTree(root, new Vector3(2f, 0f, 15f), 0.95f);
            BuildTree(root, new Vector3(11f, 0f, 13f), 0.85f);
            BuildTree(root, new Vector3(20f, 0f, 6f), 1f);
            BuildTree(root, new Vector3(21f, 0f, -7f), 0.9f);
            BuildTree(root, new Vector3(10f, 0f, -14f), 1.05f);
            BuildTree(root, new Vector3(-8f, 0f, -14.5f), 0.95f);
            BuildTree(root, new Vector3(-21f, 0f, -11f), 1.15f);
            BuildBush(root, new Vector3(-11f, 0f, -5.2f));
            BuildBush(root, new Vector3(-3.5f, 0f, 6.2f));
            BuildBush(root, new Vector3(4.5f, 0f, -5.8f));
            BuildBush(root, new Vector3(9.5f, 0f, 6f));
            BuildBush(root, new Vector3(15.5f, 0f, -4f));
            BuildBush(root, new Vector3(-16.5f, 0f, 3.2f));

            // 화단 · 돌 · 수레 (동선 밖)
            BuildFlowerBed(root, new Vector3(-4.8f, 0f, 6.8f), 2f);
            BuildFlowerBed(root, new Vector3(2.2f, 0f, -6.2f), 1.7f);
            BuildFlowerBed(root, new Vector3(-12.8f, 0f, -5f), 1.5f);
            BuildStonePile(root, new Vector3(15.5f, 0f, -6.2f));
            BuildCart(root, new Vector3(-12.2f, 0f, -5.5f), 15f);
            BuildClothesLine(root, new Vector3(-16.8f, 0f, 8.2f), 10f);

            // 가로등 — 큰길 가장자리
            BuildLamp(root, new Vector3(-14f, 0f, 2.4f));
            BuildLamp(root, new Vector3(-6.5f, 0f, 2.6f));
            BuildLamp(root, new Vector3(1.5f, 0f, 2.6f));
            BuildLamp(root, new Vector3(9f, 0f, 2.5f));
            BuildLamp(root, new Vector3(16.5f, 0f, 2.4f));
            BuildLamp(root, new Vector3(-2f, 0f, 8.2f));
            BuildLamp(root, new Vector3(-2f, 0f, -7.5f));
            BuildLamp(root, new Vector3(19.5f, 0f, 2.4f), 1.05f);

            // 벤치 — 광장 타일 위가 아니라 길가·녹지
            BuildTownBench(root, new Vector3(-5.8f, 0f, -5.2f), 0f);
            BuildTownBench(root, new Vector3(4.5f, 0f, 9.2f), 180f);
            BuildTownBench(root, new Vector3(11.5f, 0f, -5.5f), 15f);
            BuildTownCrate(root, new Vector3(-8.8f, 0f, -4.5f));
            BuildTownCrate(root, new Vector3(-8.1f, 0f, -4.8f));
            BuildTownCrate(root, new Vector3(7.5f, 0f, -5f));
            BuildTownBarrel(root, new Vector3(-10.2f, 0f, 4.5f));
            BuildTownBarrel(root, new Vector3(5.8f, 0f, 5.2f));
            BuildTownBarrel(root, new Vector3(14.5f, 0f, 3.5f));

            // 광장 깃발 — Mira와 멀리 (위에서 보면 빨간 판처럼 보이던 위치 피함)
            BuildTownBanner(root, new Vector3(-8.5f, 0f, 8.5f), MatLib.AccentTeal);
            BuildTownBanner(root, new Vector3(8.5f, 0f, 8.5f), MatLib.AccentCopper);

            var signRoot = new GameObject("Sign");
            signRoot.transform.SetParent(root, false);
            signRoot.transform.position = new Vector3(17.5f, 0f, -2.2f);
            Prim.Cube("SignPole", signRoot.transform, new Vector3(0f, 1.15f, 0f), new Vector3(0.12f, 2.3f, 0.12f),
                MatLib.MetalDark, 0.4f);
            Prim.Cube("SignBoard", signRoot.transform, new Vector3(0.85f, 1.7f, 0f), new Vector3(1.5f, 0.7f, 0.08f),
                MatLib.StoneDark, 0.25f);
            SolidBox(signRoot.transform, "SignBlock", new Vector3(0.4f, 1.2f, 0f), new Vector3(1.6f, 2.4f, 0.35f));
            WorldLabel.Attach(signRoot.transform, "던전 입구 →", new Vector3(0.85f, 2.35f, 0f),
                new Color(0.55f, 0.9f, 0.85f), 0.04f, LabelShowMode.Proximity, 6f);

            // 황혼 조명 (과하지 않게)
            EnsureDirectionalLight(new Color(0.92f, 0.78f, 0.62f), new Vector3(35f, -35f, 10f));
            EnsureFillLight(new Color(0.3f, 0.4f, 0.55f), new Vector3(-25f, 45f, -20f), 0.42f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = new Color(0.32f, 0.32f, 0.36f);
            RenderSettings.fogDensity = 0.008f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.27f, 0.27f, 0.31f);
        }

        private static void BuildTownBench(Transform root, Vector3 pos, float yaw)
        {
            // 탑다운에서도 좌석으로 읽히게 — 짧고 두툼한 실루엣
            var bench = new GameObject("Bench");
            bench.transform.SetParent(root, false);
            bench.transform.position = pos;
            bench.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            Prim.Cube("LegL", bench.transform, new Vector3(-0.7f, 0.22f, 0f), new Vector3(0.16f, 0.44f, 0.5f),
                MatLib.Wood, 0.15f);
            Prim.Cube("LegR", bench.transform, new Vector3(0.7f, 0.22f, 0f), new Vector3(0.16f, 0.44f, 0.5f),
                MatLib.Wood, 0.15f);
            Prim.Cube("Seat", bench.transform, new Vector3(0f, 0.48f, 0.05f), new Vector3(1.7f, 0.14f, 0.55f),
                new Color(0.4f, 0.3f, 0.2f), 0.2f);
            Prim.Cube("Back", bench.transform, new Vector3(0f, 0.85f, -0.22f), new Vector3(1.7f, 0.55f, 0.12f),
                MatLib.Wood, 0.18f);
            Prim.Cube("ArmL", bench.transform, new Vector3(-0.85f, 0.62f, 0f), new Vector3(0.12f, 0.12f, 0.55f),
                new Color(0.34f, 0.24f, 0.16f), 0.2f);
            Prim.Cube("ArmR", bench.transform, new Vector3(0.85f, 0.62f, 0f), new Vector3(0.12f, 0.12f, 0.55f),
                new Color(0.34f, 0.24f, 0.16f), 0.2f);
            SolidBox(bench.transform, "BenchBlock", new Vector3(0f, 0.55f, 0f), new Vector3(1.85f, 1.1f, 0.7f));
        }

        private static void BuildTownCrate(Transform root, Vector3 pos, float size = 0.75f)
        {
            Prim.Cube("Crate", root, pos + Vector3.up * (size * 0.5f),
                new Vector3(size, size, size), MatLib.Wood, 0.18f);
            Prim.Cube("CrateBand", root, pos + Vector3.up * (size * 0.5f),
                new Vector3(size + 0.04f, size * 0.12f, size + 0.04f), MatLib.MetalDark, 0.4f);
            SolidBox(root, "CrateBlock", pos + Vector3.up * (size * 0.5f), new Vector3(size, size, size));
        }

        private static void BuildTownBarrel(Transform root, Vector3 pos)
        {
            Prim.Cyl("Barrel", root, pos + Vector3.up * 0.55f, new Vector3(0.55f, 0.55f, 0.55f),
                new Color(0.32f, 0.22f, 0.14f), 0.2f);
            Prim.Cyl("BarrelRim", root, pos + Vector3.up * 1.05f, new Vector3(0.58f, 0.04f, 0.58f),
                MatLib.MetalDark, 0.35f);
            Prim.Cyl("BarrelBand", root, pos + Vector3.up * 0.55f, new Vector3(0.6f, 0.05f, 0.6f),
                MatLib.MetalDark, 0.35f);
            SolidBox(root, "BarrelBlock", pos + Vector3.up * 0.55f, new Vector3(0.7f, 1.1f, 0.7f));
        }

        private static void BuildTownBanner(Transform root, Vector3 pos, Color cloth)
        {
            var banner = new GameObject("Banner");
            banner.transform.SetParent(root, false);
            banner.transform.position = pos;
            Prim.Cyl("BannerPole", banner.transform, new Vector3(0f, 1.6f, 0f), new Vector3(0.12f, 1.6f, 0.12f),
                MatLib.MetalDark, 0.4f);
            Prim.Sphere("BannerFinial", banner.transform, new Vector3(0f, 3.25f, 0f), 0.2f,
                MatLib.GoldTrim, 0.5f);
            // 세로로 늘어뜨린 깃발 (위에서 봐도 천으로 읽히게)
            Prim.Cube("BannerCloth", banner.transform, new Vector3(0.55f, 2.35f, 0f), new Vector3(1.0f, 1.5f, 0.05f),
                cloth, 0.2f);
            Prim.Cube("BannerFold", banner.transform, new Vector3(0.55f, 1.5f, 0f), new Vector3(0.7f, 0.25f, 0.04f),
                Color.Lerp(cloth, MatLib.Black, 0.25f), 0.15f);
            SolidBox(banner.transform, "BannerPoleBlock", new Vector3(0f, 1.6f, 0f), new Vector3(0.3f, 3.2f, 0.3f));
        }

        /// <summary>Mira 옆 나무 의뢰판 — 상호작용 시각 앵커.</summary>
        public static void BuildQuestNoticeBoard(Transform parent, Vector3 pos)
        {
            var board = new GameObject("QuestNoticeBoard");
            board.transform.SetParent(parent, false);
            board.transform.position = pos;

            Prim.Cyl("PostL", board.transform, new Vector3(-0.55f, 0.85f, 0f), new Vector3(0.1f, 0.85f, 0.1f),
                MatLib.Wood, 0.15f);
            Prim.Cyl("PostR", board.transform, new Vector3(0.55f, 0.85f, 0f), new Vector3(0.1f, 0.85f, 0.1f),
                MatLib.Wood, 0.15f);
            Prim.Cube("Frame", board.transform, new Vector3(0f, 1.45f, 0f), new Vector3(1.5f, 1.2f, 0.12f),
                MatLib.Wood, 0.18f);
            Prim.Cube("Panel", board.transform, new Vector3(0f, 1.45f, 0.04f), new Vector3(1.3f, 1.0f, 0.06f),
                new Color(0.22f, 0.28f, 0.3f), 0.2f);
            // 게시물 조각
            Prim.Cube("NoteA", board.transform, new Vector3(-0.28f, 1.65f, 0.08f), new Vector3(0.45f, 0.35f, 0.02f),
                new Color(0.85f, 0.8f, 0.65f), 0.15f);
            Prim.Cube("NoteB", board.transform, new Vector3(0.3f, 1.55f, 0.08f), new Vector3(0.4f, 0.4f, 0.02f),
                new Color(0.8f, 0.75f, 0.7f), 0.15f);
            Prim.Cube("NoteC", board.transform, new Vector3(-0.15f, 1.2f, 0.08f), new Vector3(0.55f, 0.28f, 0.02f),
                new Color(0.75f, 0.78f, 0.7f), 0.15f);
            Prim.Cube("Header", board.transform, new Vector3(0f, 2.15f, 0.02f), new Vector3(1.2f, 0.22f, 0.08f),
                MatLib.AccentTeal, 0.35f);
            SolidBox(board.transform, "BoardBlock", new Vector3(0f, 1.1f, 0f), new Vector3(1.55f, 2.2f, 0.45f));
            WorldLabel.Attach(board.transform, "의뢰판", new Vector3(0f, 2.55f, 0f),
                new Color(0.55f, 0.9f, 0.85f), 0.035f, LabelShowMode.Proximity, 5f);
        }

        private static void BuildFlowerBed(Transform root, Vector3 pos, float width)
        {
            Prim.Cube("BedSoil", root, pos + Vector3.up * 0.08f, new Vector3(width, 0.16f, width * 0.55f),
                new Color(0.22f, 0.16f, 0.1f), 0.1f);
            Prim.Cube("BedRim", root, pos + Vector3.up * 0.12f, new Vector3(width + 0.15f, 0.08f, width * 0.55f + 0.15f),
                MatLib.StoneDark, 0.2f);
            var blooms = new[]
            {
                new Color(0.72f, 0.35f, 0.32f), new Color(0.85f, 0.7f, 0.35f),
                new Color(0.55f, 0.45f, 0.7f), new Color(0.9f, 0.55f, 0.45f)
            };
            for (var i = 0; i < 5; i++)
            {
                var ox = (i - 2) * (width * 0.18f);
                var oz = ((i & 1) == 0 ? 0.12f : -0.1f) * width * 0.2f;
                Prim.Sphere($"Bloom{i}", root, pos + new Vector3(ox, 0.28f, oz), 0.22f + (i % 3) * 0.04f,
                    blooms[i % blooms.Length], 0.25f);
            }

            SolidBox(root, "BedBlock", pos + Vector3.up * 0.25f,
                new Vector3(width + 0.1f, 0.5f, width * 0.55f + 0.1f));
        }

        private static void BuildStonePile(Transform root, Vector3 pos)
        {
            Prim.Cube("RockA", root, pos + new Vector3(0f, 0.22f, 0f), new Vector3(0.7f, 0.4f, 0.55f),
                MatLib.Stone, 0.15f);
            Prim.Cube("RockB", root, pos + new Vector3(0.35f, 0.18f, 0.15f), new Vector3(0.45f, 0.32f, 0.4f),
                MatLib.StoneDark, 0.12f);
            Prim.Cube("RockC", root, pos + new Vector3(-0.25f, 0.15f, -0.2f), new Vector3(0.4f, 0.28f, 0.35f),
                new Color(0.35f, 0.34f, 0.36f), 0.12f);
            SolidBox(root, "RockBlock", pos + Vector3.up * 0.3f, new Vector3(1.1f, 0.6f, 0.9f));
        }

        private static void BuildCart(Transform root, Vector3 pos, float yaw)
        {
            var cart = new GameObject("Cart");
            cart.transform.SetParent(root, false);
            cart.transform.position = pos;
            cart.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            Prim.Cube("CartBed", cart.transform, new Vector3(0f, 0.55f, 0f), new Vector3(2.2f, 0.18f, 1.2f),
                MatLib.Wood, 0.18f);
            Prim.Cube("CartSideL", cart.transform, new Vector3(0f, 0.75f, 0.55f), new Vector3(2f, 0.45f, 0.08f),
                MatLib.Wood, 0.15f);
            Prim.Cube("CartSideR", cart.transform, new Vector3(0f, 0.75f, -0.55f), new Vector3(2f, 0.45f, 0.08f),
                MatLib.Wood, 0.15f);
            var wL = Prim.Cyl("WheelL", cart.transform, new Vector3(-0.7f, 0.35f, 0.72f),
                new Vector3(0.7f, 0.08f, 0.7f), MatLib.Wood, 0.2f);
            wL.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            var wR = Prim.Cyl("WheelR", cart.transform, new Vector3(-0.7f, 0.35f, -0.72f),
                new Vector3(0.7f, 0.08f, 0.7f), MatLib.Wood, 0.2f);
            wR.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            Prim.Cube("CartLoad", cart.transform, new Vector3(0.15f, 0.85f, 0f), new Vector3(1.2f, 0.45f, 0.8f),
                new Color(0.28f, 0.2f, 0.14f), 0.15f);
            SolidBox(cart.transform, "CartBlock", new Vector3(0f, 0.7f, 0f), new Vector3(2.3f, 1.4f, 1.4f));
        }

        private static void BuildClothesLine(Transform root, Vector3 pos, float yaw)
        {
            var line = new GameObject("ClothesLine");
            line.transform.SetParent(root, false);
            line.transform.position = pos;
            line.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            Prim.Cyl("PoleL", line.transform, new Vector3(-1.4f, 1.1f, 0f), new Vector3(0.08f, 1.1f, 0.08f),
                MatLib.Wood, 0.15f);
            Prim.Cyl("PoleR", line.transform, new Vector3(1.4f, 1.1f, 0f), new Vector3(0.08f, 1.1f, 0.08f),
                MatLib.Wood, 0.15f);
            Prim.Cube("Rope", line.transform, new Vector3(0f, 2f, 0f), new Vector3(2.7f, 0.03f, 0.03f),
                MatLib.MetalDark, 0.2f);
            Prim.Cube("ClothA", line.transform, new Vector3(-0.5f, 1.55f, 0f), new Vector3(0.55f, 0.7f, 0.04f),
                new Color(0.75f, 0.72f, 0.65f), 0.15f);
            Prim.Cube("ClothB", line.transform, new Vector3(0.55f, 1.5f, 0f), new Vector3(0.5f, 0.6f, 0.04f),
                new Color(0.45f, 0.55f, 0.6f), 0.15f);
            SolidBox(line.transform, "PoleBlockL", new Vector3(-1.4f, 1.1f, 0f), new Vector3(0.25f, 2.2f, 0.25f));
            SolidBox(line.transform, "PoleBlockR", new Vector3(1.4f, 1.1f, 0f), new Vector3(0.25f, 2.2f, 0.25f));
        }

        private readonly struct DungeonTheme
        {
            public readonly Color Floor;
            public readonly Color Wall;
            public readonly Color Accent;
            public readonly Color Trim;
            public readonly Color Torch;
            public readonly Color Fog;
            public readonly float FogDensity;

            public DungeonTheme(Color floor, Color wall, Color accent, Color trim, Color torch, Color fog,
                float fogDensity)
            {
                Floor = floor;
                Wall = wall;
                Accent = accent;
                Trim = trim;
                Torch = torch;
                Fog = fog;
                FogDensity = fogDensity;
            }
        }

        private static DungeonTheme ResolveDungeonTheme(DungeonRoomData room)
        {
            var name = room.ThemeName ?? "";
            if (name.Contains("흑요"))
            {
                return new DungeonTheme(
                    new Color(0.07f, 0.08f, 0.12f), new Color(0.12f, 0.13f, 0.2f),
                    MatLib.AccentTeal, new Color(0.35f, 0.4f, 0.5f),
                    new Color(0.45f, 0.75f, 1f), new Color(0.1f, 0.12f, 0.18f), 0.018f);
            }

            if (name.Contains("뼈") || name.Contains("납골"))
            {
                return new DungeonTheme(
                    new Color(0.12f, 0.11f, 0.1f), new Color(0.2f, 0.18f, 0.16f),
                    MatLib.Bone, new Color(0.45f, 0.4f, 0.32f),
                    new Color(1f, 0.7f, 0.45f), new Color(0.16f, 0.14f, 0.12f), 0.015f);
            }

            if (name.Contains("공허") || name.Contains("심연"))
            {
                return new DungeonTheme(
                    new Color(0.06f, 0.05f, 0.08f), new Color(0.1f, 0.09f, 0.14f),
                    MatLib.AccentWine, new Color(0.35f, 0.18f, 0.28f),
                    new Color(0.85f, 0.3f, 0.45f), new Color(0.08f, 0.07f, 0.11f), 0.022f);
            }

            // 잿빛 회랑 기본 + RoomType 보정
            var floor = room.Type switch
            {
                RoomType.Start => new Color(0.11f, 0.12f, 0.13f),
                RoomType.Exit => new Color(0.09f, 0.11f, 0.14f),
                RoomType.Treasure => new Color(0.13f, 0.11f, 0.09f),
                RoomType.Boss => new Color(0.14f, 0.08f, 0.1f),
                RoomType.Event => new Color(0.1f, 0.12f, 0.14f),
                _ => MatLib.DungeonFloor
            };
            return new DungeonTheme(
                floor, MatLib.DungeonWall, MatLib.AccentCopper, MatLib.NpcTrim,
                new Color(1f, 0.55f, 0.3f), MatLib.Fog, 0.016f);
        }

        public static Transform BuildDungeonRoom(Transform parent, DungeonRoomData room, bool doorsLocked)
        {
            var root = new GameObject($"Room3D_{room.Id}").transform;
            root.SetParent(parent, false);
            var theme = ResolveDungeonTheme(room);

            // 바닥 — 높이 겹침(Z-fighting) 없이 층층이
            // Base top=0, Inset top=0.04, Rim top=0.06, Tile top=0.08
            GroundSlab(root, "Floor", Vector3.zero, new Vector2(16.4f, 12.4f), theme.Floor, 0f, 1f, true);
            GroundSlab(root, "FloorInset", Vector3.zero, new Vector2(14.6f, 10.6f),
                theme.Floor * 1.12f, 0.04f, 0.08f);
            // 몰딩은 전체 판이 아니라 테두리만 (전체 판이면 석판과 Z-fight)
            GroundSlab(root, "FloorRimN", new Vector3(0f, 0f, 5.45f), new Vector2(15.0f, 0.35f),
                theme.Wall * 0.85f, 0.06f, 0.05f);
            GroundSlab(root, "FloorRimS", new Vector3(0f, 0f, -5.45f), new Vector2(15.0f, 0.35f),
                theme.Wall * 0.85f, 0.06f, 0.05f);
            GroundSlab(root, "FloorRimE", new Vector3(7.45f, 0f, 0f), new Vector2(0.35f, 10.55f),
                theme.Wall * 0.85f, 0.06f, 0.05f);
            GroundSlab(root, "FloorRimW", new Vector3(-7.45f, 0f, 0f), new Vector2(0.35f, 10.55f),
                theme.Wall * 0.85f, 0.06f, 0.05f);

            for (var x = -3; x <= 3; x++)
            {
                for (var z = -2; z <= 2; z++)
                {
                    if ((x + z) % 2 != 0)
                    {
                        continue;
                    }

                    GroundSlab(root, $"Tile_{x}_{z}",
                        new Vector3(x * 1.85f, 0f, z * 1.85f),
                        new Vector2(1.5f, 1.5f),
                        Color.Lerp(theme.Wall, MatLib.StoneDark, 0.55f), 0.08f, 0.035f);
                }
            }

            // 벽 — 문 방향은 개구부 분할
            BuildSegmentedWall(root, "WallN", true, 5.55f, room.ConnectedNorth, theme.Wall);
            BuildSegmentedWall(root, "WallS", true, -5.55f, room.ConnectedSouth, theme.Wall);
            BuildSegmentedWall(root, "WallE", false, 7.85f, room.ConnectedEast, theme.Wall);
            BuildSegmentedWall(root, "WallW", false, -7.85f, room.ConnectedWest, theme.Wall);

            // 중간 띠 — 천장 교차보/슬래브는 탑뷰를 가리므로 쓰지 않음
            Prim.Cube("WainscotN", root, new Vector3(0f, 1.15f, 5.2f), new Vector3(15.5f, 0.12f, 0.12f),
                theme.Trim, 0.4f);
            Prim.Cube("WainscotS", root, new Vector3(0f, 1.15f, -5.2f), new Vector3(15.5f, 0.12f, 0.12f),
                theme.Trim, 0.4f);
            // 벽 가장자리 보만 (방 안쪽 십자보 없음)
            Prim.Cube("BeamN", root, new Vector3(0f, 3.55f, 5.15f), new Vector3(15.2f, 0.18f, 0.28f),
                MatLib.MetalDark, 0.45f);
            Prim.Cube("BeamE", root, new Vector3(7.4f, 3.55f, 0f), new Vector3(0.28f, 0.18f, 10.5f),
                MatLib.MetalDark, 0.45f);
            Prim.Cube("BeamW", root, new Vector3(-7.4f, 3.55f, 0f), new Vector3(0.28f, 0.18f, 10.5f),
                MatLib.MetalDark, 0.45f);

            Pillar(root, new Vector3(-4.6f, 0f, 3.1f), theme);
            Pillar(root, new Vector3(4.6f, 0f, 3.1f), theme);
            Pillar(root, new Vector3(-4.6f, 0f, -3.1f), theme);
            Pillar(root, new Vector3(4.6f, 0f, -3.1f), theme);

            Torch(root, new Vector3(-5.6f, 2.55f, 3.5f), theme.Torch);
            Torch(root, new Vector3(5.6f, 2.55f, 3.5f), theme.Torch);
            Torch(root, new Vector3(-5.6f, 2.55f, -3.5f), theme.Torch);
            Torch(root, new Vector3(5.6f, 2.55f, -3.5f), theme.Torch);

            // 코너 브라켓
            PlaceBracket(root, new Vector3(-7.2f, 2.85f, 4.9f));
            PlaceBracket(root, new Vector3(7.2f, 2.85f, 4.9f));
            PlaceBracket(root, new Vector3(-7.2f, 2.85f, -4.9f));
            PlaceBracket(root, new Vector3(7.2f, 2.85f, -4.9f));

            // 배너 (북쪽 먼 벽 — 카메라 쪽에는 달지 않음)
            Banner(root, new Vector3(-2.2f, 2.2f, 5.15f), theme.Accent);
            Banner(root, new Vector3(2.2f, 2.2f, 5.15f), theme.Accent);

            DecorateByType(root, room, theme);

            var locked = doorsLocked && !room.Cleared;
            if (room.ConnectedNorth)
            {
                CreateDoor(root, DoorDir.North, new Vector3(0f, 0f, 4.55f), locked, theme);
            }

            if (room.ConnectedSouth)
            {
                CreateDoor(root, DoorDir.South, new Vector3(0f, 0f, -4.55f), locked, theme);
            }

            if (room.ConnectedEast)
            {
                CreateDoor(root, DoorDir.East, new Vector3(6.55f, 0f, 0f), locked, theme);
            }

            if (room.ConnectedWest)
            {
                CreateDoor(root, DoorDir.West, new Vector3(-6.55f, 0f, 0f), locked, theme);
            }

            ApplyDungeonAtmosphere(theme);
            DungeonOdyssey.Art.AmbientDust.Create(root, theme.Fog * 1.4f + new Color(0f, 0f, 0f, 0.35f), 7);
            return root;
        }

        private static void DecorateByType(Transform root, DungeonRoomData room, DungeonTheme theme)
        {
            switch (room.Type)
            {
                case RoomType.Start:
                    // 러그 top≈0.10, 룬 top≈0.14 — 겹치면 흰 깜빡임(Z-fight)
                    GroundSlab(root, "StartRug", new Vector3(0f, 0f, -1f), new Vector2(3.6f, 2.4f),
                        MatLib.AccentWine, 0.10f, 0.04f);
                    GroundSlab(root, "WelcomeRune", new Vector3(0f, 0f, 0.5f), new Vector2(1.6f, 1.6f),
                        theme.Accent, 0.14f, 0.03f);
                    Breakable(root, new Vector3(-3.2f, 0.4f, -2.4f), MatLib.Wood);
                    Breakable(root, new Vector3(3.4f, 0.35f, -2.2f), MatLib.MetalDark);
                    break;
                case RoomType.Exit:
                    Altar(root, new Vector3(0f, 0f, 1.2f), theme);
                    GroundSlab(root, "ExitRug", new Vector3(0f, 0f, 0f), new Vector2(4.2f, 3.2f),
                        MatLib.AccentTeal, 0.10f, 0.04f);
                    Prim.Cyl("PortalMark", root, new Vector3(0f, 0.14f, -1.5f), new Vector3(2.4f, 0.03f, 2.4f),
                        theme.Accent, 0.35f);
                    break;
                case RoomType.Treasure:
                    Chest(root, new Vector3(0f, 0f, 1f));
                    GroundSlab(root, "TreasureRug", new Vector3(0f, 0f, -1f), new Vector2(3.6f, 2.4f),
                        MatLib.GoldTrim, 0.10f, 0.04f);
                    Prim.Cube("GoldPileA", root, new Vector3(-1.8f, 0.2f, 1.6f), new Vector3(0.7f, 0.35f, 0.55f),
                        MatLib.AccentCopper, 0.55f);
                    Prim.Cube("GoldPileB", root, new Vector3(1.9f, 0.18f, 1.4f), new Vector3(0.55f, 0.3f, 0.5f),
                        new Color(0.7f, 0.55f, 0.25f), 0.55f);
                    break;
                case RoomType.Boss:
                    GroundSlab(root, "BossRug", new Vector3(0f, 0f, 0f), new Vector2(5.5f, 4.2f),
                        MatLib.AccentWine, 0.10f, 0.04f);
                    GroundSlab(root, "BossSigil", new Vector3(0f, 0f, 0.8f), new Vector2(2.4f, 2.4f),
                        new Color(0.55f, 0.12f, 0.18f), 0.14f, 0.03f);
                    Bones(root, new Vector3(-3.2f, 0.15f, -2.2f));
                    Bones(root, new Vector3(3.1f, 0.15f, -2f));
                    Bones(root, new Vector3(-2.5f, 0.15f, 2.4f));
                    Bones(root, new Vector3(2.6f, 0.15f, 2.2f));
                    break;
                case RoomType.Event:
                    BuildEventShrine(root, theme);
                    break;
                default:
                    Breakable(root, new Vector3(3.2f, 0.4f, -2f), MatLib.Wood);
                    Breakable(root, new Vector3(-3.5f, 0.4f, 2f), MatLib.MetalDark);
                    Bones(root, new Vector3(-2.5f, 0.15f, -1.5f));
                    Bones(root, new Vector3(2.2f, 0.15f, 1.8f));
                    Bones(root, new Vector3(0.5f, 0.12f, -2.6f));
                    if ((room.ThemeName ?? "").Contains("뼈") || (room.ThemeName ?? "").Contains("납골"))
                    {
                        Bones(root, new Vector3(-1.2f, 0.15f, 2.2f));
                        Bones(root, new Vector3(3f, 0.15f, 0.2f));
                    }

                    break;
            }
        }

        private static void ApplyDungeonAtmosphere(DungeonTheme theme)
        {
            EnsureDirectionalLight(new Color(0.48f, 0.52f, 0.62f), new Vector3(38f, -42f, 8f));
            EnsureFillLight(theme.Torch * 0.5f + new Color(0.18f, 0.22f, 0.32f), new Vector3(-30f, 48f, -14f),
                0.55f);
            // 약한 선형 안개 — 깊이감만 주고 화면을 먹이지 않음
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = theme.Fog;
            RenderSettings.fogStartDistance = 12f;
            RenderSettings.fogEndDistance = 38f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.18f, 0.2f, 0.26f);
        }

        private static void BuildSegmentedWall(Transform root, string name, bool alongX, float edge,
            bool hasDoor, Color wallColor)
        {
            const float open = 2.9f;
            const float openH = 2.7f;
            const float thick = 0.55f;
            // 탑뷰 카메라가 벽 너머를 보도록 높이를 제한
            const float wallH = 3.2f;
            const float wallY = 1.6f;

            if (!hasDoor)
            {
                if (alongX)
                {
                    Wall(root, name, new Vector3(0f, wallY, edge), new Vector3(16.4f, wallH, thick), wallColor);
                }
                else
                {
                    Wall(root, name, new Vector3(edge, wallY, 0f), new Vector3(thick, wallH, 12.4f), wallColor);
                }

                return;
            }

            // 좌/우(또는 전/후) 벽 + 문 위 린텔 벽
            if (alongX)
            {
                var sideW = (16.4f - open) * 0.5f;
                var sideC = (16.4f + open) * 0.25f;
                Wall(root, name + "L", new Vector3(-sideC, wallY, edge), new Vector3(sideW, wallH, thick),
                    wallColor);
                Wall(root, name + "R", new Vector3(sideC, wallY, edge), new Vector3(sideW, wallH, thick),
                    wallColor);
                var topH = wallH - openH;
                var topY = openH + topH * 0.5f;
                Wall(root, name + "Top", new Vector3(0f, topY, edge), new Vector3(open + 0.15f, topH, thick),
                    wallColor);
            }
            else
            {
                var sideW = (12.4f - open) * 0.5f;
                var sideC = (12.4f + open) * 0.25f;
                Wall(root, name + "L", new Vector3(edge, wallY, -sideC), new Vector3(thick, wallH, sideW),
                    wallColor);
                Wall(root, name + "R", new Vector3(edge, wallY, sideC), new Vector3(thick, wallH, sideW),
                    wallColor);
                var topH = wallH - openH;
                var topY = openH + topH * 0.5f;
                Wall(root, name + "Top", new Vector3(edge, topY, 0f), new Vector3(thick, topH, open + 0.15f),
                    wallColor);
            }
        }

        private static void CreateDoor(Transform root, DoorDir dir, Vector3 pos, bool locked, DungeonTheme theme)
        {
            var dirKo = RoomDoor.DirKorean(dir);
            var doorLabel = locked ? $"{dirKo} 문 · 봉인" : $"{dirKo} 문 [E]";
            var doorColor = locked ? new Color(1f, 0.4f, 0.35f) : new Color(0.5f, 1f, 0.85f);
            var yaw = dir switch
            {
                DoorDir.North => 0f,
                DoorDir.South => 180f,
                DoorDir.East => 90f,
                _ => -90f
            };

            var door = new GameObject($"Door_{dir}");
            door.transform.SetParent(root, false);
            door.transform.position = pos;
            door.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            // 로컬: +Z가 방 바깥(문 앞면). NS/EW 모두 동일 로컬 조립 후 yaw 회전.
            // 프레임
            Prim.Cube("FrameL", door.transform, new Vector3(-1.15f, 1.35f, 0f), new Vector3(0.32f, 2.7f, 0.55f),
                theme.Wall, 0.25f);
            Prim.Cube("FrameR", door.transform, new Vector3(1.15f, 1.35f, 0f), new Vector3(0.32f, 2.7f, 0.55f),
                theme.Wall, 0.25f);
            Prim.Cube("Lintel", door.transform, new Vector3(0f, 2.8f, 0f), new Vector3(2.7f, 0.3f, 0.62f),
                theme.Trim, 0.45f);
            Prim.Cube("Keystone", door.transform, new Vector3(0f, 3.0f, 0.05f), new Vector3(0.45f, 0.28f, 0.5f),
                theme.Accent, 0.5f);
            Prim.Cube("Threshold", door.transform, new Vector3(0f, 0.08f, 0f), new Vector3(2.5f, 0.16f, 0.7f),
                MatLib.StoneDark, 0.2f);

            // 문짝 (고체 콜라이더 — 개구부로 맵 밖 유출 방지)
            var leaf = Prim.Cube("Leaf", door.transform, new Vector3(0f, 1.45f, 0f), new Vector3(2f, 2.7f, 0.22f),
                locked ? MatLib.AccentWine : new Color(0.2f, 0.34f, 0.36f), 0.3f);
            var leafCol = leaf.AddComponent<BoxCollider>();
            leafCol.size = Vector3.one;
            Prim.Cube("BandA", door.transform, new Vector3(0f, 0.55f, 0.12f), new Vector3(2.05f, 0.12f, 0.08f),
                MatLib.MetalDark, 0.6f);
            Prim.Cube("BandB", door.transform, new Vector3(0f, 1.45f, 0.12f), new Vector3(2.05f, 0.12f, 0.08f),
                MatLib.MetalDark, 0.6f);
            Prim.Cube("BandC", door.transform, new Vector3(0f, 2.35f, 0.12f), new Vector3(2.05f, 0.12f, 0.08f),
                MatLib.MetalDark, 0.6f);
            Prim.Cube("PanelL", door.transform, new Vector3(-0.45f, 1.45f, 0.14f), new Vector3(0.7f, 1.5f, 0.04f),
                theme.Wall * 1.1f, 0.25f);
            Prim.Cube("PanelR", door.transform, new Vector3(0.45f, 1.45f, 0.14f), new Vector3(0.7f, 1.5f, 0.04f),
                theme.Wall * 1.1f, 0.25f);
            Prim.Cube("Handle", door.transform, new Vector3(0.7f, 1.35f, 0.22f), new Vector3(0.12f, 0.35f, 0.18f),
                MatLib.AccentCopper, 0.65f);

            // 봉인 룬
            var seal = Prim.Sphere("Seal", door.transform, new Vector3(0f, 1.55f, 0.28f), 0.28f,
                locked ? MatLib.AccentWine : MatLib.AccentTeal, 0.55f);
            seal.GetComponent<MeshRenderer>().sharedMaterial = locked
                ? MatLib.GetEmissive(MatLib.AccentWine, new Color(1f, 0.25f, 0.2f), 1.8f)
                : MatLib.GetEmissive(MatLib.AccentTeal, new Color(0.4f, 1f, 0.85f), 1.4f);
            var sealLight = seal.AddComponent<Light>();
            sealLight.type = LightType.Point;
            sealLight.range = 3.2f;
            sealLight.intensity = locked ? 1.8f : 1.1f;
            sealLight.color = locked ? new Color(1f, 0.3f, 0.25f) : new Color(0.45f, 1f, 0.85f);

            // 문 옆 작은 횃불
            Torch(door.transform, new Vector3(-1.55f, 2.2f, 0.15f), theme.Torch);
            Torch(door.transform, new Vector3(1.55f, 2.2f, 0.15f), theme.Torch);

            // 트리거 (루트)
            var box = door.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.center = new Vector3(0f, 1.4f, 0f);
            box.size = new Vector3(2.6f, 2.8f, 1.4f);

            var roomDoor = door.AddComponent<RoomDoor>();
            var tint = new[]
            {
                leaf.GetComponent<MeshRenderer>(),
                door.transform.Find("BandA")?.GetComponent<MeshRenderer>(),
                door.transform.Find("BandB")?.GetComponent<MeshRenderer>(),
                door.transform.Find("BandC")?.GetComponent<MeshRenderer>(),
                door.transform.Find("Handle")?.GetComponent<MeshRenderer>(),
                seal.GetComponent<MeshRenderer>()
            };
            roomDoor.BindVisuals(tint, sealLight, seal.transform);
            roomDoor.Setup(dir, locked);

            var label = WorldLabel.Attach(door.transform, doorLabel,
                new Vector3(0f, 3.7f, 0f), doorColor, 0.042f, LabelShowMode.Manual);
            roomDoor.BindLabel(label);
        }

        private static void BuildEventShrine(Transform root, DungeonTheme theme)
        {
            GroundSlab(root, "EventRug", Vector3.zero, new Vector2(4f, 3.2f), MatLib.AccentTeal, 0.10f, 0.04f);
            var shrine = new GameObject("EventShrine");
            shrine.transform.SetParent(root, false);
            shrine.transform.position = new Vector3(0f, 0f, 0.8f);
            Prim.Cyl("Base", shrine.transform, new Vector3(0f, 0.25f, 0f), new Vector3(1.4f, 0.25f, 1.4f),
                MatLib.StoneDark, 0.2f);
            Prim.Cube("Pillar", shrine.transform, new Vector3(0f, 1.1f, 0f), new Vector3(0.55f, 1.6f, 0.55f),
                theme.Wall, 0.22f);
            var gem = Prim.Sphere("Gem", shrine.transform, new Vector3(0f, 2.05f, 0f), 0.45f, theme.Accent, 0.55f);
            gem.GetComponent<MeshRenderer>().sharedMaterial =
                MatLib.GetEmissive(theme.Accent, theme.Torch, 1.6f, 0.35f);
            var light = gem.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = theme.Torch;
            light.range = 5f;
            light.intensity = 1.4f;

            var col = shrine.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 1.6f;
            col.center = new Vector3(0f, 1f, 0f);
            shrine.AddComponent<DungeonEventInteractable>();
            WorldLabel.Attach(shrine.transform, "성소 [E]", new Vector3(0f, 2.8f, 0f),
                new Color(0.7f, 0.9f, 1f), 0.04f, LabelShowMode.Proximity, 3.2f);
        }

        private static void PlaceBracket(Transform parent, Vector3 pos)
        {
            Prim.Cube("Bracket", parent, pos, new Vector3(0.45f, 0.18f, 0.45f), MatLib.MetalDark, 0.5f);
            Prim.Cube("BracketArm", parent, pos + new Vector3(0f, -0.25f, 0f), new Vector3(0.12f, 0.45f, 0.12f),
                MatLib.MetalDark, 0.5f);
        }

        private static void Banner(Transform parent, Vector3 pos, Color color)
        {
            Prim.Cube("BannerPole", parent, pos + new Vector3(0f, 0.4f, 0f), new Vector3(0.08f, 0.9f, 0.08f),
                MatLib.MetalDark, 0.4f);
            Prim.Cube("BannerCloth", parent, pos + new Vector3(0f, -0.15f, 0.08f), new Vector3(0.7f, 1.1f, 0.05f),
                color, 0.2f);
        }

        /// <summary>
        /// 마을 바닥 — 두꺼운 큐브 중첩(지파이팅)을 피하고 얇은 슬랩으로 층위 구성.
        /// </summary>
        private static void BuildTownGround(Transform root)
        {
            // 넓은 기반 지형
            GroundSlab(root, "Earth", Vector3.zero, new Vector2(88f, 64f), MatLib.GrassDark, topY: 0f, thick: 0.8f,
                withCollider: true);

            GroundSlab(root, "GrassW", new Vector3(-18f, 0f, -4f), new Vector2(28f, 22f), MatLib.Grass, 0.02f);
            GroundSlab(root, "GrassE", new Vector3(16f, 0f, 4f), new Vector2(24f, 20f), MatLib.Grass, 0.02f);
            GroundSlab(root, "GrassN", new Vector3(0f, 0f, 12f), new Vector2(36f, 14f),
                new Color(0.18f, 0.24f, 0.17f), 0.02f);
            GroundSlab(root, "GrassS", new Vector3(0f, 0f, -12f), new Vector2(34f, 12f),
                new Color(0.17f, 0.23f, 0.16f), 0.02f);

            // 잔디 얼룩 · 흙 패치
            GroundSlab(root, "PatchA", new Vector3(-15f, 0f, 5f), new Vector2(6f, 4f),
                new Color(0.2f, 0.28f, 0.18f), 0.025f);
            GroundSlab(root, "DirtYardW", new Vector3(-17f, 0f, 0.5f), new Vector2(8f, 6f),
                new Color(0.32f, 0.28f, 0.22f), 0.03f);

            // 동서 큰길
            GroundSlab(root, "Road", new Vector3(2f, 0f, 0.4f), new Vector2(52f, 4.2f), MatLib.Path, 0.04f);
            GroundSlab(root, "RoadEdgeN", new Vector3(2f, 0f, 2.65f), new Vector2(52f, 0.32f),
                MatLib.StoneDark, 0.06f);
            GroundSlab(root, "RoadEdgeS", new Vector3(2f, 0f, -1.85f), new Vector2(52f, 0.32f),
                MatLib.StoneDark, 0.06f);

            // 남북 교차로
            GroundSlab(root, "RoadCross", new Vector3(-1.5f, 0f, 0.2f), new Vector2(4f, 26f), MatLib.Path, 0.045f);

            GroundSlab(root, "RoadGate", new Vector3(18f, 0f, 0.4f), new Vector2(16f, 3.2f),
                new Color(0.3f, 0.3f, 0.32f), 0.05f);

            // 집 앞 돌패드
            GroundSlab(root, "DoorPad1", new Vector3(-18f, 0f, 4.2f), new Vector2(2.2f, 1.4f),
                MatLib.Stone, 0.06f);
            GroundSlab(root, "DoorPad2", new Vector3(-4f, 0f, 11.2f), new Vector2(2f, 1.2f),
                MatLib.Stone, 0.06f);

            // 중앙 광장
            const float plazaX = -1.5f;
            const float plazaZ = 0.8f;
            GroundSlab(root, "PlazaBase", new Vector3(plazaX, 0f, plazaZ), new Vector2(16f, 14f),
                MatLib.StoneDark, 0.04f);

            const float tile = 1.55f;
            for (var ix = -4; ix <= 4; ix++)
            {
                for (var iz = -3; iz <= 3; iz++)
                {
                    var darker = ((ix + iz) & 1) == 0;
                    var worn = (ix * ix + iz * iz) < 4;
                    var c = darker
                        ? (worn ? new Color(0.3f, 0.3f, 0.32f) : new Color(0.34f, 0.34f, 0.36f))
                        : (worn ? new Color(0.38f, 0.37f, 0.36f) : new Color(0.42f, 0.41f, 0.4f));
                    GroundSlab(root, $"Tile_{ix}_{iz}",
                        new Vector3(plazaX + ix * tile, 0f, plazaZ + iz * tile),
                        new Vector2(tile - 0.08f, tile - 0.08f), c, 0.09f, 0.05f);
                }
            }

            GroundSlab(root, "PlazaRimN", new Vector3(plazaX, 0f, plazaZ + 7.1f), new Vector2(16.4f, 0.35f),
                MatLib.GoldTrim, 0.12f);
            GroundSlab(root, "PlazaRimS", new Vector3(plazaX, 0f, plazaZ - 7.1f), new Vector2(16.4f, 0.35f),
                MatLib.GoldTrim, 0.12f);
            GroundSlab(root, "PlazaRimE", new Vector3(plazaX + 8.2f, 0f, plazaZ), new Vector2(0.35f, 14.2f),
                MatLib.GoldTrim, 0.12f);
            GroundSlab(root, "PlazaRimW", new Vector3(plazaX - 8.2f, 0f, plazaZ), new Vector2(0.35f, 14.2f),
                MatLib.GoldTrim, 0.12f);

            Prim.Cyl("PlazaDisc", root, new Vector3(plazaX, 0.13f, plazaZ), new Vector3(4.6f, 0.02f, 4.6f),
                new Color(0.32f, 0.32f, 0.34f), 0.2f);
            Prim.Cyl("PlazaCenter", root, new Vector3(plazaX, 0.145f, plazaZ), new Vector3(2f, 0.018f, 2f),
                new Color(0.38f, 0.36f, 0.32f), 0.25f);
            for (var i = 0; i < 10; i++)
            {
                var a = i * 36f * Mathf.Deg2Rad;
                Prim.Cube($"PlazaAccent{i}", root,
                    new Vector3(plazaX + Mathf.Cos(a) * 2.4f, 0.155f, plazaZ + Mathf.Sin(a) * 2.4f),
                    new Vector3(0.38f, 0.03f, 0.12f), MatLib.GoldTrim, 0.35f);
            }

            // 서쪽 시장 석판
            GroundSlab(root, "MarketPad", new Vector3(-10f, 0f, 0.5f), new Vector2(10f, 8f),
                new Color(0.28f, 0.27f, 0.26f), 0.05f);
        }

        private static void GroundSlab(Transform parent, string name, Vector3 center, Vector2 sizeXZ, Color color,
            float topY, float thick = 0.06f, bool withCollider = false)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(center.x, topY - thick * 0.5f, center.z);
            go.transform.localScale = new Vector3(sizeXZ.x, thick, sizeXZ.y);
            go.GetComponent<MeshRenderer>().sharedMaterial = MatLib.Get(color, 0.12f);
            if (!withCollider)
            {
                Object.Destroy(go.GetComponent<Collider>());
            }
        }

        /// <summary>보이지 않는 장애물 박스 — Prim 소품(콜라이더 제거됨)용.</summary>
        private static void SolidBox(Transform parent, string name, Vector3 localPos, Vector3 size) =>
            AddBoxBlock(parent, name, localPos, size);

        private static void Floor(Transform parent, string name, Vector3 pos, Vector3 scale, Color color, float y)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(pos.x, y, pos.z);
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = MatLib.Get(color);
        }

        private static void Wall(Transform parent, string name, Vector3 pos, Vector3 scale, Color? color = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = MatLib.Get(color ?? MatLib.DungeonWall, 0.22f);
        }

        private static void Pillar(Transform parent, Vector3 pos, DungeonTheme theme)
        {
            if (ModelCatalog.TrySpawn(ModelCatalog.DungeonPillar, pos, parent) != null)
            {
                return;
            }

            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Pillar";
            go.transform.SetParent(parent, false);
            go.transform.position = pos + Vector3.up * 1.5f;
            go.transform.localScale = new Vector3(0.55f, 1.5f, 0.55f);
            go.GetComponent<MeshRenderer>().sharedMaterial = MatLib.Get(theme.Wall * 1.1f, 0.25f);
            Prim.Cube("Cap", go.transform, new Vector3(0f, 1.02f, 0f), new Vector3(1.55f, 0.16f, 1.55f),
                theme.Trim, 0.45f);
            Prim.Cube("Base", go.transform, new Vector3(0f, -1.02f, 0f), new Vector3(1.65f, 0.14f, 1.65f),
                MatLib.Stone, 0.2f);
            Prim.Cube("Ring", go.transform, new Vector3(0f, 0.15f, 0f), new Vector3(1.25f, 0.08f, 1.25f),
                theme.Accent, 0.5f);
        }

        private static void Torch(Transform parent, Vector3 pos, Color flameColor)
        {
            if (ModelCatalog.TrySpawn(ModelCatalog.DungeonTorch, pos, parent) != null)
            {
                return;
            }

            var bracket = Prim.Cube("TorchBracket", parent, pos, new Vector3(0.16f, 0.1f, 0.28f), MatLib.MetalDark,
                0.55f);
            var stick = Prim.Cyl("Torch", bracket.transform, new Vector3(0f, 0.38f, 0.12f),
                new Vector3(0.09f, 0.38f, 0.09f), MatLib.Wood, 0.15f);
            var flame = Prim.Sphere("Flame", stick.transform, new Vector3(0f, 1.1f, 0f), 0.38f,
                flameColor, 0.2f);
            flame.GetComponent<MeshRenderer>().sharedMaterial =
                MatLib.GetEmissive(flameColor, flameColor, 2.2f, 0.2f);
            Prim.Sphere("Ember", stick.transform, new Vector3(0.05f, 0.95f, 0.02f), 0.18f,
                flameColor * 1.2f, 0.15f);

            var light = flame.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = flameColor;
            light.range = 6.2f;
            light.intensity = 1.85f;
        }

        private static void Rug(Transform parent, Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Rug";
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = MatLib.Get(color);
            Object.Destroy(go.GetComponent<Collider>());
        }

        private static void Breakable(Transform parent, Vector3 pos, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Crate";
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            go.GetComponent<MeshRenderer>().sharedMaterial = MatLib.Get(color);
        }

        private static void Bones(Transform parent, Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Bones";
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.3f, 0.15f, 0.3f);
            go.transform.rotation = Quaternion.Euler(0f, 35f, 90f);
            go.GetComponent<MeshRenderer>().sharedMaterial = MatLib.Get(new Color(0.9f, 0.88f, 0.8f));
            Object.Destroy(go.GetComponent<Collider>());
        }

        private static void Chest(Transform parent, Vector3 pos)
        {
            GameObject go = ModelCatalog.TrySpawn(ModelCatalog.DungeonChest, pos, parent);
            if (go == null)
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Chest";
                go.transform.SetParent(parent, false);
                go.transform.position = pos + Vector3.up * 0.45f;
                go.transform.localScale = new Vector3(1.2f, 0.8f, 0.8f);
                go.GetComponent<MeshRenderer>().sharedMaterial = MatLib.Get(MatLib.AccentCopper);
            }

            go.name = "Chest";
            var col = go.GetComponent<Collider>();
            if (col == null)
            {
                col = go.AddComponent<BoxCollider>();
            }

            col.isTrigger = true;
            if (go.GetComponent<TreasureChest>() == null)
            {
                var chest = go.AddComponent<TreasureChest>();
                chest.Configure(gold: 35 + Random.Range(0, 20), potionChancePercent: 55);
            }

            WorldLabel.Attach(go.transform, "보물상자 [E]", new Vector3(0f, 1.55f, 0f),
                new Color(1f, 0.85f, 0.3f), 0.04f, LabelShowMode.Proximity, 2.8f, priority: 1);
        }

        private static void Altar(Transform parent, Vector3 pos, DungeonTheme theme)
        {
            if (ModelCatalog.TrySpawn(ModelCatalog.DungeonAltar, pos, parent) != null)
            {
                return;
            }

            var baseGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseGo.name = "Altar";
            baseGo.transform.SetParent(parent, false);
            baseGo.transform.position = pos + Vector3.up * 0.45f;
            baseGo.transform.localScale = new Vector3(2.2f, 0.9f, 1.35f);
            baseGo.GetComponent<MeshRenderer>().sharedMaterial = MatLib.Get(theme.Wall, 0.25f);

            Prim.Cube("AltarTop", baseGo.transform, new Vector3(0f, 0.65f, 0f), new Vector3(1.15f, 0.2f, 1.15f),
                theme.Trim, 0.45f);
            Prim.Cube("AltarStep", baseGo.transform, new Vector3(0f, -0.35f, 0.55f), new Vector3(1.4f, 0.25f, 0.5f),
                MatLib.StoneDark, 0.2f);

            var gem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gem.name = "AltarGem";
            gem.transform.SetParent(baseGo.transform, false);
            gem.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            gem.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            gem.GetComponent<MeshRenderer>().sharedMaterial =
                MatLib.GetEmissive(theme.Accent, theme.Torch, 1.6f, 0.55f);
            Object.Destroy(gem.GetComponent<Collider>());

            var gemLight = gem.AddComponent<Light>();
            gemLight.type = LightType.Point;
            gemLight.color = theme.Torch;
            gemLight.range = 5f;
            gemLight.intensity = 1.4f;
        }

        private static void BuildHouse(Transform parent, Vector3 pos, Vector3 scale, Color roofColor,
            bool lit = false)
        {
            var ai = ModelCatalog.TrySpawn(ModelCatalog.TownHouse, pos, parent);
            if (ai != null)
            {
                return;
            }

            var house = new GameObject("House");
            house.transform.SetParent(parent, false);
            house.transform.position = pos;

            var wallTone = Color.Lerp(MatLib.Stone, new Color(0.48f, 0.44f, 0.4f), 0.35f);
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(house.transform, false);
            body.transform.localPosition = Vector3.up * (scale.y * 0.5f);
            body.transform.localScale = scale;
            body.GetComponent<MeshRenderer>().sharedMaterial = MatLib.Get(wallTone, 0.18f);

            var hx = scale.x * 0.5f;
            var hz = scale.z * 0.5f;
            var hy = scale.y;

            Prim.Cube("Foundation", house.transform, new Vector3(0f, 0.08f, 0f),
                new Vector3(scale.x + 0.3f, 0.16f, scale.z + 0.3f), MatLib.StoneDark, 0.15f);

            // 안정적인 슬랩 지붕 (+ 살짝 큰 처마)
            Prim.Cube("Roof", house.transform, new Vector3(0f, hy + 0.2f, 0f),
                new Vector3(scale.x + 0.7f, 0.45f, scale.z + 0.7f), roofColor, 0.12f);
            Prim.Cube("RoofPeak", house.transform, new Vector3(0f, hy + 0.48f, 0f),
                new Vector3(scale.x * 0.55f, 0.22f, scale.z * 0.55f),
                Color.Lerp(roofColor, MatLib.MetalDark, 0.25f), 0.15f);

            Prim.Cube("Chimney", house.transform, new Vector3(hx * 0.4f, hy + 0.85f, -hz * 0.3f),
                new Vector3(0.5f, 0.9f, 0.5f), MatLib.StoneDark, 0.15f);
            Prim.Cube("ChimneyCap", house.transform, new Vector3(hx * 0.4f, hy + 1.35f, -hz * 0.3f),
                new Vector3(0.65f, 0.1f, 0.65f), MatLib.Stone, 0.2f);

            var frontZ = hz + 0.03f;
            Prim.Cube("DoorFrame", house.transform, new Vector3(0f, 1.05f, frontZ),
                new Vector3(1.0f, 2.0f, 0.08f), MatLib.Wood, 0.15f);
            Prim.Cube("Door", house.transform, new Vector3(0f, 1f, frontZ + 0.04f),
                new Vector3(0.8f, 1.85f, 0.06f), new Color(0.22f, 0.14f, 0.1f), 0.2f);
            Prim.Sphere("DoorKnob", house.transform, new Vector3(0.28f, 1f, frontZ + 0.1f), 0.08f,
                MatLib.GoldTrim, 0.55f);

            var winMat = lit
                ? MatLib.GetEmissive(new Color(0.85f, 0.7f, 0.4f), new Color(1f, 0.75f, 0.35f), 1.2f, 0.55f)
                : MatLib.Get(new Color(0.22f, 0.32f, 0.38f), 0.65f);

            var winY = hy * 0.55f;
            var winL = Prim.Cube("WinL", house.transform, new Vector3(-hx * 0.42f, winY, frontZ),
                new Vector3(0.65f, 0.7f, 0.06f), new Color(0.25f, 0.35f, 0.4f), 0.55f);
            winL.GetComponent<MeshRenderer>().sharedMaterial = winMat;
            Prim.Cube("WinLFrame", house.transform, new Vector3(-hx * 0.42f, winY, frontZ - 0.02f),
                new Vector3(0.78f, 0.82f, 0.05f), MatLib.Wood, 0.2f);
            Prim.Cube("WinLCross", house.transform, new Vector3(-hx * 0.42f, winY, frontZ + 0.04f),
                new Vector3(0.65f, 0.05f, 0.03f), MatLib.Wood, 0.2f);

            var winR = Prim.Cube("WinR", house.transform, new Vector3(hx * 0.42f, winY, frontZ),
                new Vector3(0.65f, 0.7f, 0.06f), new Color(0.25f, 0.35f, 0.4f), 0.55f);
            winR.GetComponent<MeshRenderer>().sharedMaterial = winMat;
            Prim.Cube("WinRFrame", house.transform, new Vector3(hx * 0.42f, winY, frontZ - 0.02f),
                new Vector3(0.78f, 0.82f, 0.05f), MatLib.Wood, 0.2f);
            Prim.Cube("WinRCross", house.transform, new Vector3(hx * 0.42f, winY, frontZ + 0.04f),
                new Vector3(0.65f, 0.05f, 0.03f), MatLib.Wood, 0.2f);

            Prim.Cube("Trim", house.transform, new Vector3(0f, hy * 0.88f, frontZ),
                new Vector3(scale.x + 0.05f, 0.1f, 0.06f), MatLib.NpcTrim, 0.4f);
            Prim.Cube("Step", house.transform, new Vector3(0f, 0.1f, frontZ + 0.4f),
                new Vector3(1.25f, 0.18f, 0.5f), MatLib.Stone, 0.15f);

            if (lit)
            {
                var glow = winL.AddComponent<Light>();
                glow.type = LightType.Point;
                glow.range = 4.2f;
                glow.intensity = 0.45f;
                glow.color = new Color(1f, 0.72f, 0.4f);
            }
        }

        private static void BuildWell(Transform parent, Vector3 pos)
        {
            var wellRoot = new GameObject("Well");
            wellRoot.transform.SetParent(parent, false);
            wellRoot.transform.position = pos;

            var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.name = "WellBody";
            body.transform.SetParent(wellRoot.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            body.transform.localScale = new Vector3(1.7f, 0.9f, 1.7f);
            body.GetComponent<MeshRenderer>().sharedMaterial = MatLib.Get(MatLib.DungeonWall, 0.15f);

            Prim.Cyl("WellRim", wellRoot.transform, new Vector3(0f, 0.95f, 0f), new Vector3(1.95f, 0.14f, 1.95f),
                MatLib.Stone, 0.2f);

            var water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            water.name = "Water";
            water.transform.SetParent(wellRoot.transform, false);
            water.transform.localPosition = new Vector3(0f, 0.72f, 0f);
            water.transform.localScale = new Vector3(1.2f, 0.06f, 1.2f);
            water.GetComponent<MeshRenderer>().sharedMaterial =
                MatLib.Get(new Color(0.28f, 0.5f, 0.68f), 0.85f);
            Object.Destroy(water.GetComponent<Collider>());

            Prim.Cyl("PostL", wellRoot.transform, new Vector3(-0.65f, 1.5f, 0f), new Vector3(0.1f, 0.65f, 0.1f),
                MatLib.Wood);
            Prim.Cyl("PostR", wellRoot.transform, new Vector3(0.65f, 1.5f, 0f), new Vector3(0.1f, 0.65f, 0.1f),
                MatLib.Wood);
            Prim.Cube("Beam", wellRoot.transform, new Vector3(0f, 2.1f, 0f), new Vector3(1.55f, 0.12f, 0.12f),
                MatLib.Wood);
            Prim.Cube("WellRoof", wellRoot.transform, new Vector3(0f, 2.35f, 0f), new Vector3(1.8f, 0.12f, 1.2f),
                MatLib.Roof, 0.15f);
            Prim.Cyl("Bucket", wellRoot.transform, new Vector3(0.2f, 1.45f, 0.5f), new Vector3(0.32f, 0.25f, 0.32f),
                new Color(0.28f, 0.2f, 0.14f), 0.2f);
            // WellBody CreatePrimitive 콜라이더 유지
        }

        private static void BuildFence(Transform parent, Vector3 start, int posts, bool alongZ)
        {
            Vector3 prev = default;
            for (var i = 0; i < posts; i++)
            {
                var p = alongZ
                    ? start + new Vector3(0f, 0f, i * 1.2f - posts * 0.6f)
                    : start + new Vector3(i * 1.2f - posts * 0.6f, 0f, 0f);
                Prim.Cyl($"Fence{i}", parent, p + Vector3.up * 0.55f, new Vector3(0.1f, 0.55f, 0.1f), MatLib.Wood);
                if (i > 0)
                {
                    var mid = (prev + p) * 0.5f;
                    var len = Vector3.Distance(prev, p) - 0.05f;
                    Prim.Cube($"FenceRail{i}", parent, mid + Vector3.up * 0.7f,
                        alongZ ? new Vector3(0.06f, 0.08f, len) : new Vector3(len, 0.08f, 0.06f),
                        MatLib.Wood, 0.15f);
                    Prim.Cube($"FenceRailLow{i}", parent, mid + Vector3.up * 0.35f,
                        alongZ ? new Vector3(0.05f, 0.06f, len) : new Vector3(len, 0.06f, 0.05f),
                        new Color(0.28f, 0.2f, 0.13f), 0.12f);
                }

                prev = p;
            }

            var span = Mathf.Max(1.2f, (posts - 1) * 1.2f);
            SolidBox(parent, "FenceBlock", start + Vector3.up * 0.55f,
                alongZ ? new Vector3(0.35f, 1.15f, span + 0.6f) : new Vector3(span + 0.6f, 1.15f, 0.35f));
        }

        private static void BuildBush(Transform parent, Vector3 pos)
        {
            Prim.Sphere("Bush", parent, pos + Vector3.up * 0.4f, 0.9f, MatLib.Leaf, 0.12f);
            Prim.Sphere("BushB", parent, pos + new Vector3(0.28f, 0.32f, 0.1f), 0.6f, MatLib.GrassDark, 0.12f);
            SolidBox(parent, "BushBlock", pos + Vector3.up * 0.4f, new Vector3(0.85f, 0.8f, 0.85f));
        }

        private static void BuildStall(Transform parent, Vector3 pos, Color canopyColor, float yaw = 0f)
        {
            if (ModelCatalog.TrySpawn(ModelCatalog.TownStall, pos, parent) != null)
            {
                SolidBox(parent, "StallBlock", pos + Vector3.up * 0.7f, new Vector3(2.3f, 1.4f, 1.3f));
                return;
            }

            var stall = new GameObject("Stall");
            stall.transform.SetParent(parent, false);
            stall.transform.position = pos;
            stall.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            Prim.Cube("Counter", stall.transform, new Vector3(0f, 0.55f, 0f), new Vector3(2.2f, 1f, 1.2f),
                MatLib.Wood, 0.18f);
            Prim.Cyl("PoleL", stall.transform, new Vector3(-0.95f, 1.55f, 0f), new Vector3(0.08f, 0.7f, 0.08f),
                MatLib.Wood);
            Prim.Cyl("PoleR", stall.transform, new Vector3(0.95f, 1.55f, 0f), new Vector3(0.08f, 0.7f, 0.08f),
                MatLib.Wood);
            Prim.Cube("Canopy", stall.transform, new Vector3(0f, 2.2f, 0f), new Vector3(2.5f, 0.12f, 1.5f),
                canopyColor, 0.2f);
            Prim.Sphere("GoodsA", stall.transform, new Vector3(-0.45f, 1.2f, 0.15f), 0.26f,
                new Color(0.7f, 0.35f, 0.25f), 0.25f);
            Prim.Cube("GoodsB", stall.transform, new Vector3(0.4f, 1.18f, 0.1f), new Vector3(0.35f, 0.25f, 0.3f),
                MatLib.AccentCopper, 0.2f);
            SolidBox(stall.transform, "StallBlock", new Vector3(0f, 0.7f, 0f), new Vector3(2.3f, 1.4f, 1.3f));
        }

        private static void BuildTree(Transform parent, Vector3 pos, float scale = 1f)
        {
            if (ModelCatalog.TrySpawn(ModelCatalog.TownTree, pos, parent) != null)
            {
                SolidBox(parent, "TreeBlock", pos + Vector3.up * 1f, new Vector3(0.9f * scale, 2f * scale, 0.9f * scale));
                return;
            }

            var tree = new GameObject("Tree");
            tree.transform.SetParent(parent, false);
            tree.transform.position = pos;
            tree.transform.localScale = Vector3.one * scale;

            Prim.Cyl("Trunk", tree.transform, new Vector3(0f, 0.85f, 0f),
                new Vector3(0.4f, 0.85f, 0.4f), MatLib.Wood, 0.15f);
            Prim.Sphere("Leaves", tree.transform, new Vector3(0f, 2.4f, 0f), 2.2f, MatLib.Leaf, 0.12f);
            Prim.Sphere("LeavesB", tree.transform, new Vector3(0.4f, 2.15f, 0.25f), 1.4f, MatLib.GrassDark, 0.1f);
            SolidBox(tree.transform, "TreeBlock", new Vector3(0f, 1f, 0f), new Vector3(0.85f, 2f, 0.85f));
        }

        private static void BuildLamp(Transform parent, Vector3 pos, float intensity = 1f)
        {
            var lampRoot = new GameObject("Lamp");
            lampRoot.transform.SetParent(parent, false);
            lampRoot.transform.position = pos;

            Prim.Cyl("Pole", lampRoot.transform, new Vector3(0f, 1.25f, 0f), new Vector3(0.14f, 1.25f, 0.14f),
                MatLib.MetalDark, 0.4f);
            Prim.Cube("Base", lampRoot.transform, new Vector3(0f, 0.08f, 0f), new Vector3(0.4f, 0.16f, 0.4f),
                MatLib.StoneDark, 0.2f);
            Prim.Cube("Arm", lampRoot.transform, new Vector3(0.4f, 2.35f, 0f), new Vector3(0.8f, 0.08f, 0.08f),
                MatLib.MetalDark, 0.4f);

            var lantern = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lantern.name = "Lantern";
            lantern.transform.SetParent(lampRoot.transform, false);
            lantern.transform.localPosition = new Vector3(0.75f, 1.95f, 0f);
            lantern.transform.localScale = new Vector3(0.32f, 0.38f, 0.32f);
            lantern.GetComponent<MeshRenderer>().sharedMaterial =
                MatLib.GetEmissive(new Color(0.85f, 0.55f, 0.28f), new Color(1f, 0.65f, 0.3f), 1.5f, 0.4f);
            Object.Destroy(lantern.GetComponent<Collider>());

            Prim.Cube("LampRoof", lampRoot.transform, new Vector3(0.75f, 2.18f, 0f), new Vector3(0.42f, 0.08f, 0.42f),
                MatLib.MetalDark, 0.35f);
            SolidBox(lampRoot.transform, "LampBlock", new Vector3(0f, 1.2f, 0f), new Vector3(0.35f, 2.4f, 0.35f));

            var light = lantern.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 7.5f * intensity;
            light.intensity = 1.25f * intensity;
            light.color = new Color(1f, 0.72f, 0.42f);
        }

        public static GameObject CreateDungeonGate(Vector3 position, Transform parent = null)
        {
            var ai = ModelCatalog.TrySpawn(ModelCatalog.DungeonGate, position, parent);
            if (ai != null)
            {
                ai.name = "DungeonEntrance";
                var aiTrigger = ai.GetComponent<SphereCollider>();
                if (aiTrigger == null)
                {
                    aiTrigger = ai.AddComponent<SphereCollider>();
                }

                aiTrigger.isTrigger = true;
                aiTrigger.radius = 3.6f;
                aiTrigger.center = new Vector3(0f, 1f, 0f);
                // AI 메시에도 프레임 차단 추가 (스케일 달라도 대략 맞춤)
                if (ai.transform.Find("BlockPillarL") == null)
                {
                    AddGateBlockers(ai.transform);
                }

                // 동쪽에서 들어오도록 문 방향 맞춤
                ai.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                return ai;
            }

            var root = new GameObject("DungeonEntrance");
            if (parent != null)
            {
                root.transform.SetParent(parent, false);
            }

            root.transform.position = position;
            // 문 구멍이 ±Z → 90° 돌려 동쪽(+X)으로 통과하게
            root.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            Prim.Cube("PillarL", root.transform, new Vector3(-1.35f, 1.7f, 0f), new Vector3(0.55f, 3.4f, 0.7f),
                MatLib.DungeonWall, 0.25f);
            Prim.Cube("PillarR", root.transform, new Vector3(1.35f, 1.7f, 0f), new Vector3(0.55f, 3.4f, 0.7f),
                MatLib.DungeonWall, 0.25f);
            Prim.Cube("Lintel", root.transform, new Vector3(0f, 3.5f, 0f), new Vector3(3.4f, 0.45f, 0.75f),
                MatLib.AccentCopper, 0.5f);
            Prim.Cube("Keystone", root.transform, new Vector3(0f, 3.85f, 0.05f), new Vector3(0.5f, 0.4f, 0.55f),
                MatLib.AccentWine, 0.45f);
            Prim.Cube("Threshold", root.transform, new Vector3(0f, 0.1f, 0.2f), new Vector3(2.8f, 0.2f, 1.1f),
                MatLib.StoneDark, 0.2f);

            Prim.Cube("Gate", root.transform, new Vector3(0f, 1.55f, 0f), new Vector3(2.2f, 2.9f, 0.28f),
                new Color(0.28f, 0.14f, 0.18f), 0.3f);
            Prim.Cube("BandA", root.transform, new Vector3(0f, 0.7f, 0.16f), new Vector3(2.25f, 0.12f, 0.08f),
                MatLib.MetalDark, 0.6f);
            Prim.Cube("BandB", root.transform, new Vector3(0f, 1.55f, 0.16f), new Vector3(2.25f, 0.12f, 0.08f),
                MatLib.MetalDark, 0.6f);
            Prim.Cube("BandC", root.transform, new Vector3(0f, 2.4f, 0.16f), new Vector3(2.25f, 0.12f, 0.08f),
                MatLib.MetalDark, 0.6f);
            var seal = Prim.Sphere("Seal", root.transform, new Vector3(0f, 1.7f, 0.28f), 0.35f, MatLib.AccentWine,
                0.5f);
            seal.GetComponent<MeshRenderer>().sharedMaterial =
                MatLib.GetEmissive(MatLib.AccentWine, new Color(1f, 0.3f, 0.25f), 2f);
            var sealLight = seal.AddComponent<Light>();
            sealLight.type = LightType.Point;
            sealLight.color = new Color(1f, 0.35f, 0.28f);
            sealLight.range = 4.5f;
            sealLight.intensity = 1.6f;

            var gateTrigger = root.AddComponent<SphereCollider>();
            gateTrigger.isTrigger = true;
            gateTrigger.radius = 3.6f;
            gateTrigger.center = new Vector3(0f, 1f, 0f);

            // Prim은 콜라이더를 제거하므로 입구 프레임·문을 따로 막아 캐릭터와 겹치지 않게
            AddGateBlockers(root.transform);
            return root;
        }

        private static void AddGateBlockers(Transform root)
        {
            // 기둥·상인방만 두껍게, 문짝은 얇게 — 앞에서 E 누를 공간 확보
            AddBoxBlock(root, "BlockPillarL", new Vector3(-1.4f, 1.6f, 0f), new Vector3(0.7f, 3.4f, 0.85f));
            AddBoxBlock(root, "BlockPillarR", new Vector3(1.4f, 1.6f, 0f), new Vector3(0.7f, 3.4f, 0.85f));
            AddBoxBlock(root, "BlockLintel", new Vector3(0f, 3.45f, 0f), new Vector3(3.5f, 0.55f, 0.85f));
            AddBoxBlock(root, "BlockDoor", new Vector3(0f, 1.55f, 0.05f), new Vector3(2.0f, 2.85f, 0.28f));
        }

        private static void AddBoxBlock(Transform parent, string name, Vector3 localPos, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.layer = 0;
            var box = go.AddComponent<BoxCollider>();
            box.size = size;
            box.center = Vector3.zero;
        }

        public static GameObject CreateExitPortal(Vector3 position, Transform parent)
        {
            var ai = ModelCatalog.TrySpawn(ModelCatalog.ExitPortal, position, parent);
            if (ai != null)
            {
                ai.name = "ExitPortal";
                if (ai.GetComponent<Collider>() == null)
                {
                    var aiCol = ai.AddComponent<SphereCollider>();
                    aiCol.isTrigger = true;
                    aiCol.radius = 1.4f;
                    aiCol.center = new Vector3(0f, 1.2f, 0f);
                }

                ai.AddComponent<PortalSpin3D>();
                return ai;
            }

            var root = new GameObject("ExitPortal");
            if (parent != null)
            {
                root.transform.SetParent(parent, false);
            }

            root.transform.position = position;

            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Ring";
            ring.transform.SetParent(root.transform, false);
            ring.transform.localPosition = new Vector3(0f, 1.35f, 0f);
            ring.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            ring.transform.localScale = new Vector3(2.4f, 0.1f, 2.4f);
            ring.GetComponent<MeshRenderer>().sharedMaterial =
                MatLib.GetEmissive(MatLib.AccentTeal, new Color(0.4f, 1f, 1f), 1.8f, 0.6f);
            Object.Destroy(ring.GetComponent<Collider>());

            var ring2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring2.name = "RingInner";
            ring2.transform.SetParent(root.transform, false);
            ring2.transform.localPosition = new Vector3(0f, 1.35f, 0f);
            ring2.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            ring2.transform.localScale = new Vector3(1.7f, 0.08f, 1.7f);
            ring2.GetComponent<MeshRenderer>().sharedMaterial =
                MatLib.GetEmissive(new Color(0.25f, 0.55f, 0.65f), new Color(0.5f, 1f, 1f), 1.2f);
            Object.Destroy(ring2.GetComponent<Collider>());

            var core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "Core";
            core.transform.SetParent(root.transform, false);
            core.transform.localPosition = new Vector3(0f, 1.35f, 0f);
            core.transform.localScale = Vector3.one * 1.05f;
            core.GetComponent<MeshRenderer>().sharedMaterial =
                MatLib.GetEmissive(new Color(0.35f, 0.8f, 0.9f), new Color(0.5f, 1f, 1f), 2.2f, 0.7f);
            Object.Destroy(core.GetComponent<Collider>());

            Prim.Cyl("PortalBase", root.transform, new Vector3(0f, 0.08f, 0f), new Vector3(2.6f, 0.06f, 2.6f),
                MatLib.StoneDark, 0.25f);
            for (var i = 0; i < 6; i++)
            {
                var a = i * 60f * Mathf.Deg2Rad;
                Prim.Cube($"Rune{i}", root.transform,
                    new Vector3(Mathf.Cos(a) * 1.15f, 0.12f, Mathf.Sin(a) * 1.15f),
                    new Vector3(0.28f, 0.05f, 0.28f), MatLib.AccentTeal, 0.5f);
            }

            var light = core.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.5f, 1f, 1f);
            light.range = 9f;
            light.intensity = 2.8f;

            var portalCol = root.AddComponent<SphereCollider>();
            portalCol.isTrigger = true;
            portalCol.radius = 1.4f;
            portalCol.center = new Vector3(0f, 1.2f, 0f);

            root.AddComponent<PortalSpin3D>();
            return root;
        }

        private static void EnsureDirectionalLight(Color color, Vector3 euler)
        {
            Light sun = null;
            foreach (var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (light.type == LightType.Directional && light.name == "Sun")
                {
                    sun = light;
                    break;
                }

                if (sun == null && light.type == LightType.Directional)
                {
                    sun = light;
                }
            }

            if (sun != null)
            {
                sun.name = "Sun";
                sun.color = color;
                sun.transform.rotation = Quaternion.Euler(euler);
                sun.shadows = LightShadows.Soft;
                sun.intensity = 1.5f;
                return;
            }

            var go = new GameObject("Sun");
            var created = go.AddComponent<Light>();
            created.type = LightType.Directional;
            created.color = color;
            created.intensity = 1.5f;
            created.shadows = LightShadows.Soft;
            go.transform.rotation = Quaternion.Euler(euler);
        }

        private static void EnsureFillLight(Color color, Vector3 euler, float intensity)
        {
            var existing = GameObject.Find("FillLight");
            if (existing != null)
            {
                var light = existing.GetComponent<Light>();
                if (light != null)
                {
                    light.color = color;
                    light.intensity = intensity;
                    existing.transform.rotation = Quaternion.Euler(euler);
                }

                return;
            }

            var go = new GameObject("FillLight");
            var fill = go.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = color;
            fill.intensity = intensity;
            fill.shadows = LightShadows.None;
            go.transform.rotation = Quaternion.Euler(euler);
        }

        private class PortalSpin3D : MonoBehaviour
        {
            private Transform _ring;
            private Transform _core;
            private Vector3 _coreBase = Vector3.one * 1.05f;

            private void Start()
            {
                _ring = transform.Find("Ring");
                _core = transform.Find("Core");
            }

            private void Update()
            {
                transform.Rotate(0f, 55f * Time.deltaTime, 0f, Space.World);
                if (_ring != null)
                {
                    _ring.Rotate(0f, 0f, -80f * Time.deltaTime, Space.Self);
                }

                if (_core != null)
                {
                    var pulse = 1f + Mathf.Sin(Time.time * 3.5f) * 0.08f;
                    _core.localScale = _coreBase * pulse;
                }
            }
        }
    }
}

