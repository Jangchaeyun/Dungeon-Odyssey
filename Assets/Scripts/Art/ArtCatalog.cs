using UnityEngine;

namespace DungeonOdyssey.Art
{
    public static class ArtCatalog
    {
        private static bool _loaded;

        public static Sprite PlayerIdle { get; private set; }
        public static Sprite PlayerWalk { get; private set; }
        public static Sprite PlayerWalkB { get; private set; }
        public static Sprite PlayerAtkA { get; private set; }
        public static Sprite PlayerAtkB { get; private set; }
        public static Sprite PlayerAtkC { get; private set; }
        public static Sprite PlayerHurt { get; private set; }
        public static Sprite Impact { get; private set; }
        public static Sprite HitSpark { get; private set; }
        public static Sprite SlimeA { get; private set; }
        public static Sprite SlimeB { get; private set; }
        public static Sprite SlimeC { get; private set; }
        public static Sprite SlimeHurt { get; private set; }
        public static Sprite SlimeDeath { get; private set; }
        public static Sprite UiPanel { get; private set; }
        public static Sprite UiButton { get; private set; }
        public static Sprite NpcGuide { get; private set; }
        public static Sprite DungeonDoor { get; private set; }
        public static Sprite Portal { get; private set; }
        public static Sprite PortalB { get; private set; }
        public static Sprite TileGrass { get; private set; }
        public static Sprite TileGrassB { get; private set; }
        public static Sprite TilePath { get; private set; }
        public static Sprite TileStone { get; private set; }
        public static Sprite TileDungeon { get; private set; }
        public static Sprite TileDungeonB { get; private set; }
        public static Sprite WallDungeon { get; private set; }
        public static Sprite WallSide { get; private set; }
        public static Sprite House { get; private set; }
        public static Sprite Tree { get; private set; }
        public static Sprite Slash { get; private set; }
        public static Sprite SlashB { get; private set; }
        public static Sprite SlashC { get; private set; }
        public static Sprite TitleBanner { get; private set; }
        public static Sprite Shadow { get; private set; }
        public static Sprite Torch { get; private set; }
        public static Sprite PropFence { get; private set; }
        public static Sprite PropWell { get; private set; }
        public static Sprite PropCrate { get; private set; }
        public static Sprite PropBarrel { get; private set; }
        public static Sprite PropChest { get; private set; }
        public static Sprite PropFlower { get; private set; }
        public static Sprite PropPillar { get; private set; }
        public static Sprite PropBones { get; private set; }
        public static Sprite PropRug { get; private set; }
        public static Sprite PropCarpet { get; private set; }
        public static Sprite PropGrass { get; private set; }
        public static Sprite PropStall { get; private set; }
        public static Sprite PropLantern { get; private set; }
        public static Sprite PropBanner { get; private set; }
        public static Sprite PropBench { get; private set; }
        public static Sprite PropLamp { get; private set; }
        public static Sprite PropRune { get; private set; }
        public static Sprite PropFloorAccent { get; private set; }
        public static Sprite PropPlazaDecal { get; private set; }
        public static Sprite PropMist { get; private set; }
        public static Sprite PropBookshelf { get; private set; }
        public static Sprite PropAltar { get; private set; }

        public static Sprite[] PlayerWalkFrames => new[] { PlayerIdle, PlayerWalk, PlayerWalkB, PlayerWalk };
        public static Sprite[] PlayerAttackFrames => new[] { PlayerAtkA, PlayerAtkB, PlayerAtkC };
        public static Sprite[] SlashFrames => new[] { Slash, SlashB, SlashC };
        public static Sprite[] SlimeFrames => new[] { SlimeA, SlimeB, SlimeC, SlimeB };
        public static Sprite[] PortalFrames => new[] { Portal, PortalB };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _loaded = false;

        public static void EnsureLoaded()
        {
            if (_loaded)
            {
                return;
            }

            PlayerIdle = Load("Art/player_idle");
            PlayerWalk = Load("Art/player_walk");
            PlayerWalkB = Load("Art/player_walk_b");
            PlayerAtkA = Load("Art/player_atk_a");
            PlayerAtkB = Load("Art/player_atk_b");
            PlayerAtkC = Load("Art/player_atk_c");
            PlayerHurt = Load("Art/player_hurt");
            Impact = Load("Art/impact");
            HitSpark = Load("Art/hit_spark");
            SlimeA = Load("Art/slime_a");
            SlimeB = Load("Art/slime_b");
            SlimeC = Load("Art/slime_c");
            SlimeHurt = Load("Art/slime_hurt");
            SlimeDeath = Load("Art/slime_death");
            UiPanel = Load("Art/ui_panel");
            UiButton = Load("Art/ui_button");
            NpcGuide = Load("Art/npc_guide");
            DungeonDoor = Load("Art/dungeon_door");
            Portal = Load("Art/portal");
            PortalB = Load("Art/portal_b");
            TileGrass = Load("Art/tile_grass");
            TileGrassB = Load("Art/tile_grass_b");
            TilePath = Load("Art/tile_path");
            TileStone = Load("Art/tile_stone");
            TileDungeon = Load("Art/tile_dungeon");
            TileDungeonB = Load("Art/tile_dungeon_b");
            WallDungeon = Load("Art/wall_dungeon");
            WallSide = Load("Art/wall_side");
            House = Load("Art/house");
            Tree = Load("Art/tree");
            Slash = Load("Art/slash");
            SlashB = Load("Art/slash_b");
            SlashC = Load("Art/slash_c");
            TitleBanner = Load("Art/title_banner");
            Shadow = Load("Art/shadow");
            Torch = Load("Art/torch");
            PropFence = Load("Art/prop_fence");
            PropWell = Load("Art/prop_well");
            PropCrate = Load("Art/prop_crate");
            PropBarrel = Load("Art/prop_barrel");
            PropChest = Load("Art/prop_chest");
            PropFlower = Load("Art/prop_flower");
            PropPillar = Load("Art/prop_pillar");
            PropBones = Load("Art/prop_bones");
            PropRug = Load("Art/prop_rug");
            PropCarpet = Load("Art/prop_carpet");
            PropGrass = Load("Art/prop_grass");
            PropStall = Load("Art/prop_stall");
            PropLantern = Load("Art/prop_lantern");
            PropBanner = Load("Art/prop_banner");
            PropBench = Load("Art/prop_bench");
            PropLamp = Load("Art/prop_lamp");
            PropRune = Load("Art/prop_rune");
            PropFloorAccent = Load("Art/prop_floor_accent");
            PropPlazaDecal = Load("Art/prop_plaza_decal");
            PropMist = Load("Art/prop_mist");
            PropBookshelf = Load("Art/prop_bookshelf");
            PropAltar = Load("Art/prop_altar");
            _loaded = true;
        }

        private static Sprite Load(string path)
        {
            var sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
            {
                return sprite;
            }

            var tex = Resources.Load<Texture2D>(path);
            if (tex == null)
            {
                Debug.LogWarning($"[Art] Missing resource: {path}");
                return null;
            }

            tex.filterMode = FilterMode.Point;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 16f);
        }
    }
}
