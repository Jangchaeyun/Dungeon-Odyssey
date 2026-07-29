using UnityEngine;

namespace DungeonOdyssey.Player
{
    /// <summary>저장/호환용 대분류. 실제 효과는 무기별 WeaponSkillSpec.</summary>
    public enum SkillId
    {
        DashSlash = 0,
        Whirlwind = 1,
        GuardianWard = 2,
        GroundSlam = 3
    }

    /// <summary>무기 스킬의 실제 전투 모션 (무기마다 조합·수치가 다름).</summary>
    public enum SkillMotion
    {
        DashCut,
        LinePierce,
        TripleStab,
        BlinkCut,
        ChaosRush,
        Spin,
        Crescent,
        StormBolt,
        SolarSpin,
        Ward,
        FrostAura,
        HolyLight,
        VoidBurst,
        Slam,
        Quake,
        BloodSlam,
        ConeCleave
    }

    public readonly struct SkillDef
    {
        public readonly SkillId Id;
        public readonly string Name;
        public readonly string ShortName;
        public readonly string Role;
        public readonly string Description;
        public readonly string Advantage;
        public readonly float Cooldown;
        public readonly int ShopCost;
        public readonly int UnlockClears;
        public readonly int UnlockLevel;

        public SkillDef(SkillId id, string name, string shortName, string role, string description,
            string advantage, float cooldown, int shopCost = 0, int unlockClears = 0, int unlockLevel = 1)
        {
            Id = id;
            Name = name;
            ShortName = shortName;
            Role = role;
            Description = description;
            Advantage = advantage;
            Cooldown = cooldown;
            ShopCost = shopCost;
            UnlockClears = unlockClears;
            UnlockLevel = unlockLevel;
        }

        public string DetailBody(bool fromWeapon, string weaponName = null) =>
            (fromWeapon && !string.IsNullOrEmpty(weaponName)
                ? $"장착 무기  ·  {weaponName}\n"
                : "") +
            $"역할  ·  {Role}\n" +
            $"{Description}\n" +
            $"장점  ·  {Advantage}\n" +
            $"쿨다운 {Cooldown:0.#}초  ·  전투 중 R 키로 발동\n" +
            "스킬은 장착한 무기에 따라 자동으로 정해집니다";
    }

    /// <summary>무기 1자루 = 고유 스킬 1개.</summary>
    public readonly struct WeaponSkillSpec
    {
        public readonly WeaponId Weapon;
        public readonly SkillId Family;
        public readonly SkillMotion Motion;
        public readonly string Name;
        public readonly string ShortName;
        public readonly string Role;
        public readonly string Description;
        public readonly string Advantage;
        public readonly float Cooldown;
        public readonly float Dash;
        public readonly float Radius;
        public readonly int Hits;
        public readonly int BonusDamage;
        public readonly float HealPct;
        public readonly float Invuln;
        public readonly float SlowMul;
        public readonly float SlowDur;

        public WeaponSkillSpec(
            WeaponId weapon, SkillId family, SkillMotion motion,
            string name, string shortName, string role, string description, string advantage,
            float cooldown, float dash, float radius, int hits, int bonusDamage,
            float healPct = 0f, float invuln = 0f, float slowMul = 1f, float slowDur = 0f)
        {
            Weapon = weapon;
            Family = family;
            Motion = motion;
            Name = name;
            ShortName = shortName;
            Role = role;
            Description = description;
            Advantage = advantage;
            Cooldown = cooldown;
            Dash = dash;
            Radius = radius;
            Hits = hits;
            BonusDamage = bonusDamage;
            HealPct = healPct;
            Invuln = invuln;
            SlowMul = slowMul;
            SlowDur = slowDur;
        }

        public SkillDef AsSkillDef() =>
            new(Family, Name, ShortName, Role, Description, Advantage, Cooldown);
    }

    public static class SkillCatalog
    {
        public const int SkillCount = 4;

        private static readonly SkillDef[] FamilyDefs =
        {
            new(SkillId.DashSlash, "돌진 계열", "돌진", "기동 · 돌입",
                "전방으로 파고들며 공격한다.", "접근·이탈에 유리하다.", 4.2f),
            new(SkillId.Whirlwind, "선풍 계열", "선풍", "광역 · 정리",
                "주변을 쓸어 피해를 준다.", "다수 전투에 강하다.", 6.2f),
            new(SkillId.GuardianWard, "장막 계열", "장막", "생존 · 방어",
                "몸을 지키며 위기를 넘긴다.", "생존력이 오른다.", 8.5f),
            new(SkillId.GroundSlam, "강타 계열", "강타", "광역 · 폭딜",
                "내려찍어 넓은 충격을 준다.", "한 방 화력이 높다.", 7f)
        };

        /// <summary>WeaponId 순서와 동일한 인덱스. 설명·장점은 무기마다 완전히 다르게 작성.</summary>
        private static readonly WeaponSkillSpec[] WeaponSkills =
        {
            // 0 IronSword
            new(WeaponId.IronSword, SkillId.DashSlash, SkillMotion.DashCut,
                "견습 돌진", "견습", "기동 · 돌입",
                "전방으로 짧게 돌진한 뒤 한 번 벤다. 추가 피해 +6, 범위 작음.",
                "쿨 3.8초로 가장 자주 쓸 수 있는 입문용 돌진.",
                3.8f, 2.1f, 1.25f, 1, 6),
            // 1 SteelBlade
            new(WeaponId.SteelBlade, SkillId.DashSlash, SkillMotion.DashCut,
                "강철 돌진", "강철", "기동 · 돌입",
                "긴 돌진 후 전방을 강하게 가른다. 추가 피해 +11, 돌진·범위가 견습보다 큼.",
                "돌입 거리와 화력이 함께 커서 첫 접촉에 유리하다.",
                4.4f, 2.7f, 1.4f, 1, 11),
            // 2 FlameSaber
            new(WeaponId.FlameSaber, SkillId.Whirlwind, SkillMotion.Spin,
                "작열 선풍", "작열", "광역 · 화염",
                "제자리에서 3회전 선풍. 매번 주변 전체에 피해(추가 +8).",
                "몰려든 잡몹을 한자리에서 빠르게 깎는다.",
                6f, 0f, 2f, 3, 8),
            // 3 FrostEdge
            new(WeaponId.FrostEdge, SkillId.GuardianWard, SkillMotion.FrostAura,
                "빙결 장막", "빙결", "생존 · 둔화",
                "2초 무적 + 최대체력 12% 회복. 주변 적을 2초간 강하게 둔화(이동 55%).",
                "위기에서 멈추고, 적의 추격을 끊는 생존기.",
                8.2f, 0f, 2.6f, 0, 0, 0.12f, 2f, 0.55f, 2f),
            // 4 StormCleaver
            new(WeaponId.StormCleaver, SkillId.Whirlwind, SkillMotion.Spin,
                "뇌명 선풍", "뇌명", "광역 · 뇌전",
                "4연속 회전 광역. 범위가 넓고 타수가 많아 다수전 특화(추가 +7).",
                "타수형 정리 — 방 안 적을 연속으로 깎는다.",
                6.4f, 0f, 2.25f, 4, 7),
            // 5 VoidReaper
            new(WeaponId.VoidReaper, SkillId.GuardianWard, SkillMotion.VoidBurst,
                "심연 장막", "심연", "생존 · 흡혈",
                "1.4초 무적 직후 주변 폭발(추가 +12). 적중·기본 회복으로 체력 흡수.",
                "버티면서 동시에 딜을 넣는 고위험 구간용.",
                8.6f, 0f, 2.5f, 1, 12, 0.08f, 1.4f),
            // 6 BoneCleaver
            new(WeaponId.BoneCleaver, SkillId.GroundSlam, SkillMotion.Slam,
                "해골 강타", "해골", "광역 · 충격",
                "땅을 2번 찍어 주변 전체에 충격. 안정적인 광역(추가 +12).",
                "실패 없이 방을 정리하는 기본 강타.",
                6.8f, 0f, 2.6f, 2, 12),
            // 7 HunterSpear
            new(WeaponId.HunterSpear, SkillId.DashSlash, SkillMotion.LinePierce,
                "추적 돌진", "추적", "기동 · 관통",
                "긴 직선으로 돌진하며 경로를 4회 관통 타격(추가 +9).",
                "일렬·복도형 배치에서 사거리 우위를 가져간다.",
                4.6f, 3.2f, 0.95f, 4, 9),
            // 8 SilverRapier
            new(WeaponId.SilverRapier, SkillId.DashSlash, SkillMotion.TripleStab,
                "성은 찌르기", "성이", "기동 · 연타",
                "짧은 스텝 후 전방 3연속 찌르기. 돌진보다 연타 화력(추가 +7).",
                "근접 빈틈을 노리는 순간 폭딜.",
                4f, 0.7f, 1.15f, 3, 7),
            // 9 CrimsonAxe
            new(WeaponId.CrimsonAxe, SkillId.GroundSlam, SkillMotion.ConeCleave,
                "혈귀 강타", "혈귀", "광역 · 부채",
                "전방 부채꼴만 강하게 찍는다. 측면·후방은 맞지 않음(추가 +15).",
                "정면 돌파·전방 밀집에 특화된 방향성 강타.",
                7f, 0f, 2.7f, 1, 15),
            // 10 StoneMaul
            new(WeaponId.StoneMaul, SkillId.GroundSlam, SkillMotion.Quake,
                "암반 강타", "암반", "광역 · 지진",
                "충격파 3연발. 범위가 갈수록 넓어져 흩어진 적도 맞춤(추가 +10).",
                "퍼진 적을 한 번에 주워 담는 지진형.",
                7.4f, 0f, 2.2f, 3, 10),
            // 11 ShadowDagger
            new(WeaponId.ShadowDagger, SkillId.DashSlash, SkillMotion.BlinkCut,
                "암영 돌진", "암영", "기동 · 암습",
                "전방 돌진 베기 후 즉시 뒤로 빠져 한 번 더 벤다(2단 히트, +10).",
                "돌입과 이탈을 한 스킬로 — 포위 탈출에 최적.",
                5f, 2.5f, 1.2f, 2, 10),
            // 12 EmberPike
            new(WeaponId.EmberPike, SkillId.DashSlash, SkillMotion.LinePierce,
                "잉걸 돌진", "잉걸", "기동 · 화염",
                "추적보다 더 긴 화염 관통 돌진. 4회 경로 타격·추가 피해 +14.",
                "중거리 직선 폭딜 — 창 계열 최고 관통력.",
                5.2f, 3.4f, 1.05f, 4, 14),
            // 13 GlacierAxe
            new(WeaponId.GlacierAxe, SkillId.GuardianWard, SkillMotion.FrostAura,
                "빙하 장막", "빙하", "생존 · 반격",
                "2.3초 무적 + 10% 회복 + 강한 둔화 후, 주변에 한기 타격(+10).",
                "방어만 하지 않고 바로 반격까지 넣는 장막.",
                8.8f, 0f, 2.8f, 1, 10, 0.1f, 2.3f, 0.48f, 2.4f),
            // 14 ThunderFang
            new(WeaponId.ThunderFang, SkillId.Whirlwind, SkillMotion.StormBolt,
                "뇌아 선풍", "뇌아", "광역 · 돌진",
                "짧은 선풍 1회 후 전방으로 뇌전 돌진 관통. 광역+추격 콤보(+10).",
                "정리 후 도망치는 적까지 이어서 때린다.",
                6.6f, 2.8f, 2f, 2, 10),
            // 15 HolyLongsword
            new(WeaponId.HolyLongsword, SkillId.GuardianWard, SkillMotion.HolyLight,
                "성역 장막", "성역", "생존 · 회복",
                "2.4초 무적 + 최대체력 28% 대회복. 공격 판정 없음.",
                "보스 패턴 직후 체력을 크게 되돌리는 순수 생존기.",
                9f, 0f, 0f, 0, 0, 0.28f, 2.4f),
            // 16 VenomFang
            new(WeaponId.VenomFang, SkillId.DashSlash, SkillMotion.DashCut,
                "맹독 돌진", "맹독", "기동 · 둔화",
                "돌진 베기(+8)에 맞은 적을 2.2초간 둔화(이동 50%).",
                "맞힌 뒤 추격·도주·패턴 회피가 쉬워진다.",
                4.8f, 2.3f, 1.3f, 1, 8, 0f, 0f, 0.5f, 2.2f),
            // 17 Bloodreaver
            new(WeaponId.Bloodreaver, SkillId.GroundSlam, SkillMotion.BloodSlam,
                "혈식 강타", "혈식", "광역 · 흡혈",
                "2연타 광역 강타(+13). 맞힌 적 수만큼 체력 회복.",
                "딜과 유지력을 동시에 — 장기전에 강하다.",
                7.5f, 0f, 2.55f, 2, 13, 0.04f),
            // 18 ObsidianGreatsword
            new(WeaponId.ObsidianGreatsword, SkillId.GroundSlam, SkillMotion.Slam,
                "흑요 강타", "흑요", "광역 · 폭딜",
                "좁은 범위 2연타지만 추가 피해 +20으로 가장 무거운 일격.",
                "엘리트·보스 근접 폭딜용. 범위보다 화력.",
                7.6f, 0f, 2.1f, 2, 20),
            // 19 SolarCleaver
            new(WeaponId.SolarCleaver, SkillId.Whirlwind, SkillMotion.SolarSpin,
                "태양 선풍", "태양", "광역 · 회복",
                "3회전 선풍(+9) 종료 시 최대체력 10% 회복.",
                "정리하면서 체력을 채워 긴 전투를 버틴다.",
                6.8f, 0f, 2.15f, 3, 9, 0.1f),
            // 20 MoonScythe
            new(WeaponId.MoonScythe, SkillId.Whirlwind, SkillMotion.Crescent,
                "월영 선풍", "월영", "광역 · 부채",
                "전방 반원(약 110°)만 2회 베기. 후방 적은 맞지 않음(+11).",
                "앞만 보고 쓸어내는 낫형 — 전방 밀집에 최적.",
                6.5f, 0f, 2.4f, 2, 11),
            // 21 ChaosBrand
            new(WeaponId.ChaosBrand, SkillId.DashSlash, SkillMotion.ChaosRush,
                "혼돈 돌진", "혼돈", "기동 · 혼돈",
                "좌→우→전방 3단 지그재그 돌진. 경로마다 타격(+12).",
                "불규칙 궤적으로 포위를 깨고 다단 히트를 넣는다.",
                5.5f, 2.2f, 1.25f, 2, 12),
            // 22 TitanHammer
            new(WeaponId.TitanHammer, SkillId.GroundSlam, SkillMotion.Quake,
                "거신 강타", "거신", "광역 · 초광역",
                "충격파 4연발. 범위가 가장 넓게 퍼지는 초광역(+14).",
                "방 전체를 짓밟는 정리용 최종 강타.",
                8.2f, 0f, 2.5f, 4, 14),
            // 23 Dragonfang
            new(WeaponId.Dragonfang, SkillId.Whirlwind, SkillMotion.Spin,
                "용염 선풍", "용염", "광역 · 폭딜",
                "넓은 범위 2회만 강하게 회전. 타수 적고 한 방 큼(+18).",
                "엘리트·보스 주변 광역 폭딜에 특화.",
                7.2f, 0f, 2.55f, 2, 18)
        };

        public static SkillDef Get(SkillId id) => FamilyDefs[Mathf.Clamp((int)id, 0, SkillCount - 1)];

        public static WeaponSkillSpec Spec(WeaponId weapon)
        {
            var i = (int)weapon;
            if (i >= 0 && i < WeaponSkills.Length)
            {
                return WeaponSkills[i];
            }

            return WeaponSkills[0];
        }

        public static SkillId ForWeapon(WeaponId weapon) => Spec(weapon).Family;

        public static SkillDef ForWeaponDef(WeaponId weapon) => Spec(weapon).AsSkillDef();

        public static string WeaponSkillName(WeaponId weapon) => Spec(weapon).Name;

        /// <summary>스킬 VFX 고유 색 — 무기마다 다르게.</summary>
        public static Color FxColor(WeaponId weapon) => weapon switch
        {
            WeaponId.IronSword => new Color(0.82f, 0.84f, 0.9f, 0.85f),
            WeaponId.SteelBlade => new Color(0.7f, 0.78f, 0.95f, 0.9f),
            WeaponId.FlameSaber => new Color(1f, 0.45f, 0.15f, 0.9f),
            WeaponId.FrostEdge => new Color(0.45f, 0.85f, 1f, 0.9f),
            WeaponId.StormCleaver => new Color(0.55f, 0.75f, 1f, 0.92f),
            WeaponId.VoidReaper => new Color(0.55f, 0.25f, 0.85f, 0.9f),
            WeaponId.BoneCleaver => new Color(0.85f, 0.78f, 0.55f, 0.9f),
            WeaponId.HunterSpear => new Color(0.55f, 0.9f, 0.45f, 0.9f),
            WeaponId.SilverRapier => new Color(0.9f, 0.95f, 1f, 0.95f),
            WeaponId.CrimsonAxe => new Color(0.95f, 0.2f, 0.28f, 0.92f),
            WeaponId.StoneMaul => new Color(0.65f, 0.55f, 0.4f, 0.9f),
            WeaponId.ShadowDagger => new Color(0.35f, 0.2f, 0.55f, 0.88f),
            WeaponId.EmberPike => new Color(1f, 0.35f, 0.1f, 0.95f),
            WeaponId.GlacierAxe => new Color(0.35f, 0.7f, 1f, 0.92f),
            WeaponId.ThunderFang => new Color(0.85f, 0.9f, 0.35f, 0.95f),
            WeaponId.HolyLongsword => new Color(1f, 0.95f, 0.55f, 0.95f),
            WeaponId.VenomFang => new Color(0.45f, 0.95f, 0.35f, 0.9f),
            WeaponId.Bloodreaver => new Color(0.85f, 0.12f, 0.22f, 0.92f),
            WeaponId.ObsidianGreatsword => new Color(0.25f, 0.2f, 0.35f, 0.95f),
            WeaponId.SolarCleaver => new Color(1f, 0.75f, 0.2f, 0.95f),
            WeaponId.MoonScythe => new Color(0.7f, 0.8f, 1f, 0.9f),
            WeaponId.ChaosBrand => new Color(0.95f, 0.35f, 0.85f, 0.95f),
            WeaponId.TitanHammer => new Color(0.75f, 0.55f, 0.3f, 0.95f),
            WeaponId.Dragonfang => new Color(1f, 0.25f, 0.1f, 0.95f),
            _ => new Color(0.9f, 0.9f, 1f, 0.85f)
        };

        public static string DetailForWeapon(WeaponId weapon)
        {
            var sk = Spec(weapon);
            var wName = WeaponCatalog.Get(weapon).Name;
            return
                $"무기 스킬  ·  {sk.Name}\n" +
                $"장착 무기  ·  {wName}\n" +
                $"역할  ·  {sk.Role}\n" +
                $"요약  ·  {sk.Description}\n" +
                $"효과\n{EffectLines(sk)}\n" +
                $"장점  ·  {sk.Advantage}\n" +
                $"쿨다운 {sk.Cooldown:0.#}초  ·  R 키로 발동\n" +
                "무기를 바꾸면 스킬 효과도 완전히 바뀝니다";
        }

        /// <summary>모션·수치 기반 — 스킬마다 다른 효과 목록.</summary>
        public static string EffectLines(WeaponSkillSpec sk)
        {
            var lines = sk.Motion switch
            {
                SkillMotion.DashCut =>
                    $"· 전방 돌진 {sk.Dash:0.#}m 후 1회 베기\n" +
                    $"· 타격 범위 반경 {sk.Radius:0.#}  ·  추가 피해 +{sk.BonusDamage}",
                SkillMotion.LinePierce =>
                    $"· 직선 돌진 {sk.Dash:0.#}m  ·  경로 {Mathf.Max(2, sk.Hits)}회 관통\n" +
                    $"· 좁은 직선 판정(반경 {sk.Radius:0.#})  ·  추가 피해 +{sk.BonusDamage}",
                SkillMotion.TripleStab =>
                    $"· 짧은 스텝 {sk.Dash:0.#}m 후 전방 {Mathf.Max(2, sk.Hits)}연속 찌르기\n" +
                    $"· 연타형 근접 폭딜  ·  추가 피해 +{sk.BonusDamage}",
                SkillMotion.BlinkCut =>
                    $"· 전방 돌진 {sk.Dash:0.#}m 베기 → 즉시 후방 추가 베기\n" +
                    $"· 2단 히트(돌입+이탈)  ·  추가 피해 +{sk.BonusDamage}",
                SkillMotion.ChaosRush =>
                    $"· 좌·우·전방 3단 지그재그 돌진(각 {sk.Dash:0.#}m)\n" +
                    $"· 경로마다 타격  ·  추가 피해 +{sk.BonusDamage}",
                SkillMotion.Spin =>
                    $"· 제자리 {Mathf.Max(2, sk.Hits)}회전 전방위 선풍\n" +
                    $"· 반경 {sk.Radius:0.#}  ·  추가 피해 +{sk.BonusDamage}",
                SkillMotion.SolarSpin =>
                    $"· 제자리 {Mathf.Max(2, sk.Hits)}회전 선풍(반경 {sk.Radius:0.#})\n" +
                    $"· 종료 시 최대 체력 {Pct(sk.HealPct)} 회복  ·  추가 피해 +{sk.BonusDamage}",
                SkillMotion.Crescent =>
                    $"· 전방 반원(약 110°) {Mathf.Max(1, sk.Hits)}회 베기\n" +
                    $"· 후방 판정 없음  ·  반경 {sk.Radius:0.#}  ·  추가 +{sk.BonusDamage}",
                SkillMotion.StormBolt =>
                    $"· 짧은 선풍 1회 → 전방 뇌전 돌진 {sk.Dash:0.#}m\n" +
                    $"· 광역+직선 추격  ·  추가 피해 +{sk.BonusDamage}",
                SkillMotion.FrostAura =>
                    $"· 무적 {sk.Invuln:0.#}초  ·  최대 체력 {Pct(sk.HealPct)} 회복\n" +
                    $"· 주변 둔화 {sk.SlowDur:0.#}초(이속 ×{sk.SlowMul:0.##})" +
                    (sk.Hits > 0 ? $"\n· 이어서 한기 타격(추가 +{sk.BonusDamage})" : ""),
                SkillMotion.HolyLight =>
                    $"· 무적 {sk.Invuln:0.#}초  ·  최대 체력 {Pct(sk.HealPct > 0f ? sk.HealPct : 0.18f)} 대회복\n" +
                    "· 공격 판정 없음 — 순수 생존·회복",
                SkillMotion.VoidBurst =>
                    $"· 무적 {sk.Invuln:0.#}초 직후 주변 폭발(반경 {sk.Radius:0.#})\n" +
                    $"· 추가 피해 +{sk.BonusDamage}  ·  적중 시 흡혈·회복",
                SkillMotion.Slam =>
                    $"· 지면 {Mathf.Max(1, sk.Hits)}연타 충격(반경 {sk.Radius:0.#})\n" +
                    $"· 추가 피해 +{sk.BonusDamage}",
                SkillMotion.Quake =>
                    $"· 충격파 {Mathf.Max(2, sk.Hits)}연발 — 갈수록 범위 확대\n" +
                    $"· 시작 반경 {sk.Radius:0.#}  ·  추가 피해 +{sk.BonusDamage}",
                SkillMotion.BloodSlam =>
                    $"· 지면 {Mathf.Max(1, sk.Hits)}연타 광역(반경 {sk.Radius:0.#})\n" +
                    $"· 추가 +{sk.BonusDamage}  ·  맞힌 적 수만큼 체력 회복",
                SkillMotion.ConeCleave =>
                    $"· 전방 부채꼴(약 95°) 강타 — 측면·후방 제외\n" +
                    $"· 반경 {sk.Radius:0.#}  ·  추가 피해 +{sk.BonusDamage}",
                _ =>
                    $"· 추가 피해 +{sk.BonusDamage}  ·  반경 {sk.Radius:0.#}"
            };

            if (sk.SlowDur > 0f && sk.Motion is SkillMotion.DashCut)
            {
                lines += $"\n· 적중 시 둔화 {sk.SlowDur:0.#}초(이속 ×{sk.SlowMul:0.##})";
            }

            return lines;
        }

        private static string Pct(float ratio) => $"{Mathf.RoundToInt(Mathf.Max(0f, ratio) * 100f)}%";

        public static bool Owns(int mask, SkillId id) => (mask & (1 << (int)id)) != 0;

        public static int WithOwned(int mask, SkillId id) => mask | (1 << (int)id);

        public static int DefaultOwnedMask => FullMask;

        public static int FullMask => (1 << SkillCount) - 1;

        public static int OwnedCount(int mask)
        {
            var n = 0;
            for (var i = 0; i < SkillCount; i++)
            {
                if ((mask & (1 << i)) != 0)
                {
                    n++;
                }
            }

            return n;
        }

        public static int ApplyMetaUnlocks(int mask, int dungeonClears, int level = 1) => FullMask;

        public static SkillId? NextUnowned(int mask) => null;

        public static SkillId CycleNext(int mask, SkillId current) => current;

        public static SkillId? NthOwned(int mask, int oneBasedIndex)
        {
            if (oneBasedIndex < 1 || oneBasedIndex > SkillCount)
            {
                return null;
            }

            return (SkillId)(oneBasedIndex - 1);
        }
    }
}
