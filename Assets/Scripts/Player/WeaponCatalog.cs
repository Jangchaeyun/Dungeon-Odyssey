using System.Collections.Generic;
using UnityEngine;

namespace DungeonOdyssey.Player
{
    public enum WeaponId
    {
        IronSword = 0,
        SteelBlade = 1,
        FlameSaber = 2,
        FrostEdge = 3,
        StormCleaver = 4,
        VoidReaper = 5,
        BoneCleaver = 6,
        HunterSpear = 7,
        SilverRapier = 8,
        CrimsonAxe = 9,
        StoneMaul = 10,
        ShadowDagger = 11,
        EmberPike = 12,
        GlacierAxe = 13,
        ThunderFang = 14,
        HolyLongsword = 15,
        VenomFang = 16,
        Bloodreaver = 17,
        ObsidianGreatsword = 18,
        SolarCleaver = 19,
        MoonScythe = 20,
        ChaosBrand = 21,
        TitanHammer = 22,
        Dragonfang = 23
    }

    public enum WeaponRarity
    {
        Common = 0,    // 일반
        Uncommon = 1,  // 고급
        Rare = 2,      // 희귀
        Epic = 3,      // 영웅
        Legendary = 4  // 전설
    }

    public enum WeaponElement
    {
        None,
        Steel,
        Flame,
        Frost,
        Storm,
        Void,
        Holy,
        Nature,
        Blood,
        Earth
    }

    public readonly struct WeaponDef
    {
        public readonly WeaponId Id;
        public readonly string Name;
        public readonly string Title;
        public readonly string ElementName;
        public readonly WeaponElement Element;
        public readonly string Passive;
        public readonly string SigilName;
        public readonly string SigilDesc;
        public readonly int BaseBonus;
        public readonly int ShopCost;
        public readonly int MaxUpgrade;
        public readonly int UnlockLevel;
        public readonly float CritBonus;
        public readonly float ElementPower;

        public WeaponDef(WeaponId id, string name, string title, string elementName, WeaponElement element,
            string passive, string sigilName, string sigilDesc,
            int baseBonus, int shopCost, int maxUpgrade, int unlockLevel,
            float critBonus = 0f, float elementPower = 0.2f)
        {
            Id = id;
            Name = name;
            Title = title;
            ElementName = elementName;
            Element = element;
            Passive = passive;
            SigilName = sigilName;
            SigilDesc = sigilDesc;
            BaseBonus = baseBonus;
            ShopCost = shopCost;
            MaxUpgrade = maxUpgrade;
            UnlockLevel = unlockLevel;
            CritBonus = critBonus;
            ElementPower = elementPower;
        }
    }

    public enum ForgeResult
    {
        Fail,
        Success,
        Great,
        Perfect
    }

    public static class WeaponCatalog
    {
        public const int UpgradeAtkPerLevel = 2;
        public const int TemperAtkPerLevel = 1;
        public const int WeaponCount = 24;
        public const int MaxTemper = 5;
        /// <summary>하드 상한. 실제 연마 가능치는 캐릭터 레벨.</summary>
        public const int AbsoluteMaxUpgrade = 99;

        private static readonly WeaponDef[] Defs =
        {
            new(WeaponId.IronSword, "무쇠 검", "견습", "무속성", WeaponElement.None,
                "모험의 시작을 함께하는 기본 검. 특별한 속성은 없지만 안정적인 일격과 균형 잡힌 손맛을 제공한다. 강화와 각인으로 성장 폭이 크다.",
                "단련의 각인", "공격력 +4 · 받는 피해가 소폭 감소한다.",
                0, 0, 8, 1, 0f, 0f),
            new(WeaponId.SteelBlade, "강철 검", "숙련", "강철", WeaponElement.Steel,
                "단련된 강철로 벼린 실전용 검. 치명타가 터질 때 강철의 무게가 실린 추가 피해를 준다. 안정적인 딜 상승을 원하는 모험가에게 적합하다.",
                "강철 각인", "치명타 피해가 +18% 증가한다.",
                4, 70, 8, 3, 0.02f, 0.22f),
            new(WeaponId.FlameSaber, "화염 검", "작열", "화염", WeaponElement.Flame,
                "칼날에 불씨가 깃든 사베르. 타격마다 적에게 화상 피해를 남기며, 화염이 강할수록 지속 피해가 커진다. 무리 전투와 보스 모두에서 화력을 끌어올린다.",
                "작열 각인", "화상 피해 +35% · 타격 시 소량의 체력을 흡수한다.",
                9, 140, 8, 5, 0.03f, 0.28f),
            new(WeaponId.FrostEdge, "서리 검", "빙결", "서리", WeaponElement.Frost,
                "차갑게 벼려진 빙결의 검. 맞은 적의 이동 속도를 낮춰 추격과 도주를 유리하게 만든다. 군중 제어가 필요한 던전에서 특히 빛난다.",
                "빙결 각인", "감속 효과가 강화되고 치명타 확률이 +4% 오른다.",
                6, 100, 8, 4, 0.04f, 0.26f),
            new(WeaponId.StormCleaver, "뇌명 참검", "뇌명", "뇌전", WeaponElement.Storm,
                "번개가 깃든 대형 참검. 일격이 주변 적에게 연쇄로 튀어 여러 대상을 동시에 깎는다. 밀집한 적 무리를 정리하는 데 특화되어 있다.",
                "뇌명 각인", "연쇄 번개 피해가 +40% 증가한다.",
                12, 190, 8, 7, 0.05f, 0.3f),
            new(WeaponId.VoidReaper, "심연의 낫", "심연", "공허", WeaponElement.Void,
                "심연에서 끌어올린 공허의 낫. 가한 피해의 일부를 체력으로 되돌리며, 오래 싸울수록 유리해진다. 보스전에서 생존과 화력을 동시에 챙긴다.",
                "심연 각인", "흡혈 +6% · 보스에게 추가 피해를 준다.",
                16, 260, 8, 9, 0.06f, 0.32f),

            new(WeaponId.BoneCleaver, "뼈 절단검", "해골", "무속성", WeaponElement.None,
                "뼈를 갈아 만든 투박한 절단검. 섬세함은 없지만 묵직한 타격감과 넉백으로 적의 접근을 끊기 좋다. 초반 화력을 빠르게 올리는 실속형 무기.",
                "해골 각인", "공격력 +5 · 타격 시 넉백이 소폭 증가한다.",
                3, 55, 8, 2, 0.01f, 0.1f),
            new(WeaponId.HunterSpear, "사냥꾼의 창", "추적", "자연", WeaponElement.Nature,
                "숲의 사냥꾼이 쓰던 장창. 창끝에 자연독이 스며 있어 타격 후 독 도트가 지속된다. 거리를 유지하며 적을 서서히 녹이는 운용에 맞다.",
                "추적 각인", "독 지속 피해가 +40% 증가한다.",
                5, 85, 8, 3, 0.03f, 0.24f),
            new(WeaponId.SilverRapier, "은빛 레이피어", "성은", "신성", WeaponElement.Holy,
                "성스러운 은으로 벼린 레이피어. 빠른 찌르기와 함께 언데드·보스에게 신성 추가 피해를 준다. 치명타를 노리는 기교형 전투에 어울린다.",
                "성은 각인", "보스에게 주는 피해가 +15% 증가한다.",
                5, 95, 8, 4, 0.05f, 0.25f),
            new(WeaponId.CrimsonAxe, "진홍 도끼", "혈귀", "혈기", WeaponElement.Blood,
                "피에 젖은 듯한 진홍빛 도끼. 타격마다 피해의 일부를 흡혈해 전선을 유지하기 쉽다. 공격적인 난전에서 체력 관리를 돕는다.",
                "혈귀 각인", "흡혈량이 +8% 증가한다.",
                7, 110, 8, 4, 0.02f, 0.27f),
            new(WeaponId.StoneMaul, "암석 망치", "대지", "대지", WeaponElement.Earth,
                "바위를 깎아 만든 중망치. 타격이 적의 방어를 일부 무시해 단단한 적에게도 안정적인 피해를 넣는다. 느리지만 확실한 한 방을 선호한다.",
                "암반 각인", "방어 무시 비율이 +12% 증가한다.",
                8, 120, 8, 5, 0.01f, 0.23f),
            new(WeaponId.ShadowDagger, "그림자 단검", "암영", "공허", WeaponElement.Void,
                "그림자 속에 숨는 암영의 단검. 치명타 확률이 높고 약한 흡혈이 붙어 있어, 연속 일격으로 폭딜을 노리기 좋다. 기습과 암살형 플레이에 특화.",
                "암영 각인", "치명타 확률 +6% · 흡혈이 한층 강화된다.",
                4, 105, 8, 5, 0.08f, 0.2f),
            new(WeaponId.EmberPike, "잉걸 창", "작열", "화염", WeaponElement.Flame,
                "끝에서 잉걸불이 피어오르는 장창. 화염 속성이 강하게 붙어 있어 화상 누적에 뛰어나다. 사거리를 살린 견제와 지속 화력에 강점이 있다.",
                "잉걸 각인", "화상 피해가 +45% 증가한다.",
                10, 155, 8, 6, 0.03f, 0.3f),
            new(WeaponId.GlacierAxe, "빙하 도끼", "빙결", "서리", WeaponElement.Frost,
                "빙하의 한기를 담은 대형 도끼. 맞은 적을 강하게 감속시켜 포지션 싸움을 지배한다. 돌진형 적과 보스 패턴을 늦추는 데 효과적이다.",
                "빙하 각인", "감속 지속 시간이 +0.6초 늘어난다.",
                9, 150, 8, 6, 0.03f, 0.29f),
            new(WeaponId.ThunderFang, "뇌아 검", "뇌명", "뇌전", WeaponElement.Storm,
                "이빨처럼 번쩍이는 뇌전의 검. 짧은 거리의 연쇄 번개로 주변 적을 함께 가격한다. 참검보다 가볍지만 연쇄 효율은 여전히 우수하다.",
                "뇌아 각인", "연쇄 번개의 범위와 피해가 증가한다.",
                11, 175, 8, 7, 0.04f, 0.31f),
            new(WeaponId.HolyLongsword, "성역의 장검", "성역", "신성", WeaponElement.Holy,
                "성역의 축복을 받은 장검. 타격 시 자신의 상처를 소량 회복하며, 신성 속성으로 강한 적에게도 안정적인 압박을 가한다. 생존형 딜러에게 추천.",
                "성역 각인", "자힐량 +50% · 보스 피해 +10%.",
                11, 200, 8, 8, 0.04f, 0.28f),
            new(WeaponId.VenomFang, "독아 검", "맹독", "자연", WeaponElement.Nature,
                "맹독이 스민 독아의 검. 타격마다 독이 중첩되어 시간이 갈수록 피해가 폭증한다. 장기전과 엘리트 처치에 특히 강력하다.",
                "맹독 각인", "독 틱 피해가 대폭 증가한다.",
                10, 185, 8, 8, 0.05f, 0.33f),
            new(WeaponId.Bloodreaver, "혈식자", "흡혈", "혈기", WeaponElement.Blood,
                "피를 갈구하는 혈기의 검. 흡혈 효율이 높아 공격이 곧 회복이 된다. 체력이 낮을수록 추가 피해가 붙어, 위기일수록 반격의 날이 선다.",
                "혈식 각인", "흡혈 +12% · 저체력일수록 추가 피해.",
                13, 220, 8, 9, 0.03f, 0.34f),
            new(WeaponId.ObsidianGreatsword, "흑요 대검", "암흑", "대지", WeaponElement.Earth,
                "흑요석처럼 무거운 대검. 방어를 크게 무시하는 일격으로 장갑 적과 보스를 관통한다. 공격 속도는 느리지만 한 방의 가치가 크다.",
                "흑요 각인", "방어 무시 비율이 +20% 증가한다.",
                14, 230, 8, 10, 0.02f, 0.3f),
            new(WeaponId.SolarCleaver, "태양 참검", "작열", "화염", WeaponElement.Flame,
                "태양의 불꽃을 가둔 참검. 화상과 함께 폭발적인 화염 피해를 일으켜 전장을 태운다. 최고 수준의 화염 화력을 원하는 모험가를 위한 무기.",
                "태양 각인", "화상과 폭발 피해가 크게 강화된다.",
                15, 250, 8, 11, 0.04f, 0.36f),
            new(WeaponId.MoonScythe, "달빛 낫", "월영", "서리", WeaponElement.Frost,
                "달빛에 물든 서리의 낫. 치명타가 터리면 강한 동결로 적을 묶어 둔다. 치명 빌드와 군중 제어를 동시에 노릴 때 최고의 선택이다.",
                "월영 각인", "치명타 시 동결 효과가 강화된다.",
                14, 245, 8, 11, 0.07f, 0.32f),
            new(WeaponId.ChaosBrand, "혼돈의 낙인", "심연", "공허", WeaponElement.Void,
                "혼돈이 새겨진 공허의 병기. 흡혈과 함께 예측할 수 없는 추가 피해가 터진다. 리스크와 보상이 큰 심연의 끝판 무기.",
                "혼돈 각인", "흡혈과 보스 피해가 극대화된다.",
                18, 300, 8, 13, 0.06f, 0.38f),
            new(WeaponId.TitanHammer, "거신 망치", "파쇄", "대지", WeaponElement.Earth,
                "거인의 힘을 담은 파쇄 망치. 타격 시 광역 충격파로 주변 적에게도 피해를 준다. 방 전체를 흔드는 압도적인 존재감의 무기.",
                "거신 각인", "충격파 피해가 +35% 증가한다.",
                17, 290, 8, 12, 0.02f, 0.35f),
            new(WeaponId.Dragonfang, "용아 검", "용염", "화염", WeaponElement.Flame,
                "용의 이빨에서 태어났다는 전설의 검. 최상급 작열 피해와 높은 치명 보정으로 화염 무기의 정점에 선다. 던전 최심부를 겨냥한 자를 위한 선택.",
                "용아 각인", "화상 피해와 치명타 피해가 극대화된다.",
                20, 340, 8, 14, 0.07f, 0.4f)
        };

        public static WeaponDef Get(WeaponId id)
        {
            var i = (int)id;
            if (i < 0 || i >= Defs.Length)
            {
                return Defs[0];
            }

            return Defs[i];
        }

        public static WeaponRarity GetRarity(WeaponId id)
        {
            var u = Get(id).UnlockLevel;
            if (u <= 2)
            {
                return WeaponRarity.Common;
            }

            if (u <= 5)
            {
                return WeaponRarity.Uncommon;
            }

            if (u <= 8)
            {
                return WeaponRarity.Rare;
            }

            if (u <= 11)
            {
                return WeaponRarity.Epic;
            }

            return WeaponRarity.Legendary;
        }

        public static string RarityName(WeaponRarity r) => r switch
        {
            WeaponRarity.Common => "일반",
            WeaponRarity.Uncommon => "고급",
            WeaponRarity.Rare => "희귀",
            WeaponRarity.Epic => "영웅",
            WeaponRarity.Legendary => "전설",
            _ => "일반"
        };

        /// <summary>등급별 카드/상점 슬롯 배경색 (구분 잘 되게).</summary>
        public static Color RarityBg(WeaponRarity r) => r switch
        {
            WeaponRarity.Common => new Color(0.32f, 0.34f, 0.4f, 0.97f),
            WeaponRarity.Uncommon => new Color(0.16f, 0.42f, 0.26f, 0.97f),
            WeaponRarity.Rare => new Color(0.14f, 0.28f, 0.55f, 0.97f),
            WeaponRarity.Epic => new Color(0.38f, 0.18f, 0.52f, 0.97f),
            WeaponRarity.Legendary => new Color(0.55f, 0.36f, 0.12f, 0.97f),
            _ => new Color(0.2f, 0.2f, 0.24f, 0.95f)
        };

        public static Color RarityAccent(WeaponRarity r) => r switch
        {
            WeaponRarity.Common => new Color(0.78f, 0.8f, 0.85f),
            WeaponRarity.Uncommon => new Color(0.4f, 0.95f, 0.55f),
            WeaponRarity.Rare => new Color(0.45f, 0.7f, 1f),
            WeaponRarity.Epic => new Color(0.85f, 0.5f, 1f),
            WeaponRarity.Legendary => new Color(1f, 0.82f, 0.35f),
            _ => Color.white
        };

        /// <summary>슬롯용 등급 뱃지 배경 (조금 더 진한 톤).</summary>
        public static Color RarityBadgeBg(WeaponRarity r)
        {
            var a = RarityAccent(r);
            return new Color(a.r * 0.25f, a.g * 0.25f, a.b * 0.25f, 0.95f);
        }

        public static float RarityPriceMult(WeaponRarity r) => r switch
        {
            WeaponRarity.Common => 0.65f,
            WeaponRarity.Uncommon => 1f,
            WeaponRarity.Rare => 1.55f,
            WeaponRarity.Epic => 2.4f,
            WeaponRarity.Legendary => 3.6f,
            _ => 1f
        };

        /// <summary>등급 반영 상점가.</summary>
        public static int ShopPrice(WeaponId id)
        {
            var def = Get(id);
            var price = Mathf.RoundToInt(def.ShopCost * RarityPriceMult(GetRarity(id)));
            return Mathf.Max(15, price);
        }

        public static WeaponRarity NextRarity(WeaponRarity r) =>
            (WeaponRarity)Mathf.Min((int)r + 1, (int)WeaponRarity.Legendary);

        public static WeaponId? RandomUnownedOfRarity(int ownedMask, WeaponRarity rarity, int playerLevel)
        {
            var pool = new List<WeaponId>();
            for (var i = 0; i < WeaponCount; i++)
            {
                var id = (WeaponId)i;
                if (Owns(ownedMask, id) || !IsUnlocked(id, playerLevel))
                {
                    continue;
                }

                if (GetRarity(id) == rarity)
                {
                    pool.Add(id);
                }
            }

            if (pool.Count == 0)
            {
                return null;
            }

            return pool[Random.Range(0, pool.Count)];
        }

        /// <summary>등급 풀에서 아무 무기 (이미 보유해도 가능 · 합성용 중복 인스턴스).</summary>
        public static WeaponId? RandomOfRarity(WeaponRarity rarity, int playerLevel)
        {
            var pool = new List<WeaponId>();
            for (var i = 0; i < WeaponCount; i++)
            {
                var id = (WeaponId)i;
                if (!IsUnlocked(id, playerLevel))
                {
                    continue;
                }

                if (GetRarity(id) == rarity)
                {
                    pool.Add(id);
                }
            }

            if (pool.Count == 0)
            {
                // 레벨 제한 무시하고 등급만
                for (var i = 0; i < WeaponCount; i++)
                {
                    var id = (WeaponId)i;
                    if (GetRarity(id) == rarity)
                    {
                        pool.Add(id);
                    }
                }
            }

            if (pool.Count == 0)
            {
                return null;
            }

            return pool[Random.Range(0, pool.Count)];
        }

        public static int WithoutOwned(int ownedMask, WeaponId id) =>
            ownedMask & ~(1 << (int)id);

        public static int GetWeaponBonus(WeaponId id, int upgradeLevel, int temperLevel = 0)
        {
            var def = Get(id);
            var lv = Mathf.Clamp(upgradeLevel, 0, AbsoluteMaxUpgrade);
            var temper = Mathf.Clamp(temperLevel, 0, MaxTemper);
            return def.BaseBonus + lv * UpgradeAtkPerLevel + temper * TemperAtkPerLevel;
        }

        public static float GetCritBonus(WeaponId id, bool hasSigil = false)
        {
            var def = Get(id);
            var c = def.CritBonus;
            if (hasSigil && def.Element == WeaponElement.Frost)
            {
                c += 0.04f;
            }

            if (hasSigil && def.Element == WeaponElement.Void && id == WeaponId.ShadowDagger)
            {
                c += 0.06f;
            }

            return c;
        }

        /// <summary>캐릭터 레벨만큼 강화 가능.</summary>
        public static int MaxUpgradeForLevel(int playerLevel) =>
            Mathf.Clamp(Mathf.Max(1, playerLevel), 1, AbsoluteMaxUpgrade);

        /// <summary>연마 가능 상한 = 캐릭터 레벨.</summary>
        public static int MaxUpgradeAllowed(WeaponId id, int playerLevel) =>
            MaxUpgradeForLevel(playerLevel);

        public static bool IsFullyUpgraded(int upgradeLevel, int playerLevel) =>
            upgradeLevel >= MaxUpgradeForLevel(playerLevel);

        /// <summary>+N 강화에 필요한 캐릭터 레벨 (= N).</summary>
        public static int LevelNeededForUpgrade(int targetUpgradeLevel) =>
            Mathf.Clamp(Mathf.Max(1, targetUpgradeLevel), 1, AbsoluteMaxUpgrade);

        public static int UpgradeCost(int currentLevel, int playerLevel = 1) =>
            28 + currentLevel * 22 + Mathf.Max(0, playerLevel - 1) * 4;

        public static int PrecisionCost(int currentLevel, int playerLevel = 1) =>
            Mathf.RoundToInt(UpgradeCost(currentLevel, playerLevel) * 1.65f);

        /// <summary>가방·대장간 공통 — 강화 시스템 전체 안내.</summary>
        public static string ForgeGuideTitle => "무기 강화 안내";

        public static string ForgeGuideBody =>
            "무기를 연마해 공격 보너스를 올리는 시스템입니다.\n" +
            "강화 단계(+N)와 템퍼(⋆)가 있으며, 올릴 수 있는 상한은 캐릭터 레벨과 같습니다.\n" +
            "\n" +
            "■ 강화 단계 (+N)\n" +
            $"· 1단계마다 무기 보너스 +{UpgradeAtkPerLevel}\n" +
            "· 일반 강화·정밀 강화로 올릴 수 있습니다\n" +
            "· 예: +10이면 강화 보너스 +" + (10 * UpgradeAtkPerLevel) + "\n" +
            "\n" +
            "■ 템퍼 (⋆)\n" +
            "· 정밀 강화로만 오릅니다 (최대 ⋆" + MaxTemper + ")\n" +
            $"· 1단계마다 무기 보너스 +{TemperAtkPerLevel}\n" +
            "· 무기 이름 옆에 ⋆1, ⋆2… 로 표시됩니다\n" +
            "\n" +
            "■ 일반 강화\n" +
            "· 비용이 더 저렴합니다\n" +
            "· 보통 +1, 가끔 대성공(+2)·완벽(+3)\n" +
            "· 템퍼는 오르지 않습니다\n" +
            "· 운에 맡기고 빠르게 키울 때 적합합니다\n" +
            "\n" +
            "■ 정밀 강화\n" +
            "· 비용이 일반의 약 1.65배입니다\n" +
            "· 항상 +1만 확정 (대성공·완벽 없음)\n" +
            "· 동시에 템퍼 ⋆ +1\n" +
            "· 템퍼가 이미 ⋆" + MaxTemper + "이면 사용할 수 없습니다\n" +
            "· 안정적으로 +1과 템퍼를 쌓을 때 적합합니다\n" +
            "\n" +
            "■ 횟수 · 최대\n" +
            "· − / + 로 원하는 강화 횟수를 고른 뒤 한 번에 실행합니다\n" +
            "· [최대]는 지금 골드로 가능한 일반 강화 횟수로 맞춥니다\n" +
            "· 연속 강화 중 상한·골드 부족이면 거기서 멈춥니다";

        public static string ForgeGuideShortNormal =>
            $"일반 강화  ·  저렴  ·  보통 +1 / 대성공 +2 / 완벽 +3  ·  템퍼 없음  ·  단계당 보너스 +{UpgradeAtkPerLevel}";

        public static string ForgeGuideShortPrecision =>
            $"정밀 강화  ·  약 1.65배 비용  ·  항상 +1 확정  ·  템퍼 ⋆+1 (최대 ⋆{MaxTemper})  ·  템퍼당 보너스 +{TemperAtkPerLevel}";

        /// <summary>
        /// 연속 강화 예상 비용 (+1씩 성공 가정). 대성공·완벽 시 실제 비용은 더 적을 수 있다.
        /// </summary>
        public static int EstimateSequentialForgeCost(
            int fromUpgrade, int attempts, int playerLevel, int forgeCap,
            bool precision, int fromTemper = 0)
        {
            var total = 0;
            var lv = Mathf.Max(0, fromUpgrade);
            var temper = Mathf.Clamp(fromTemper, 0, MaxTemper);
            var n = Mathf.Max(0, attempts);
            for (var i = 0; i < n && lv < forgeCap; i++)
            {
                if (precision && temper >= MaxTemper)
                {
                    break;
                }

                total += precision
                    ? PrecisionCost(lv, playerLevel)
                    : UpgradeCost(lv, playerLevel);
                lv += 1;
                if (precision)
                {
                    temper++;
                }
            }

            return total;
        }

        /// <summary>보유 골드로 가능한 최대 강화 시도 횟수 (+1씩 성공 가정).</summary>
        public static int MaxAffordableForgeAttempts(
            int gold, int fromUpgrade, int playerLevel, int forgeCap,
            bool precision, int fromTemper = 0)
        {
            var count = 0;
            var lv = Mathf.Max(0, fromUpgrade);
            var temper = Mathf.Clamp(fromTemper, 0, MaxTemper);
            var remain = Mathf.Max(0, gold);
            while (lv < forgeCap && count < AbsoluteMaxUpgrade)
            {
                if (precision && temper >= MaxTemper)
                {
                    break;
                }

                var cost = precision
                    ? PrecisionCost(lv, playerLevel)
                    : UpgradeCost(lv, playerLevel);
                if (remain < cost)
                {
                    break;
                }

                remain -= cost;
                lv += 1;
                if (precision)
                {
                    temper++;
                }

                count++;
            }

            return count;
        }

        public static int SigilCost(WeaponId id, int playerLevel) =>
            120 + Get(id).ShopCost / 2 + playerLevel * 8;

        public static string Stars(int level, int max)
        {
            max = Mathf.Clamp(max, 1, AbsoluteMaxUpgrade);
            level = Mathf.Clamp(level, 0, max);
            if (max > 10)
            {
                return $"강화 {level}/{max}";
            }

            var s = "";
            for (var i = 0; i < max; i++)
            {
                s += i < level ? "◆" : "◇";
            }

            return s;
        }

        public static string Label(WeaponId id, int upgradeLevel, int temper = 0, bool sigil = false)
        {
            var def = Get(id);
            var name = upgradeLevel > 0 ? $"{def.Name}+{upgradeLevel}" : def.Name;
            if (temper > 0)
            {
                name += $"⋆{temper}";
            }

            if (sigil)
            {
                name = $"『{name}』";
            }

            return name;
        }

        /// <summary>상점·가방용 상세 설명.</summary>
        public static string FullDescription(WeaponId id)
        {
            var def = Get(id);
            var rarity = GetRarity(id);
            var price = ShopPrice(id);
            var critPct = Mathf.RoundToInt(def.CritBonus * 100f);
            var elemPow = Mathf.RoundToInt(def.ElementPower * 100f);
            return
                $"{def.Name}   [{RarityName(rarity)}]  ·  {def.Title}  ·  {def.ElementName}\n" +
                $"{def.Passive}\n" +
                $"무기 보너스 +{def.BaseBonus}" +
                (critPct > 0 ? $"  ·  치명 +{critPct}%" : "") +
                (elemPow > 0 ? $"  ·  속성 위력 {elemPow}%" : "") +
                $"  ·  해금 Lv {def.UnlockLevel}  ·  가격 {price}G\n" +
                $"각인 『{def.SigilName}』 — {def.SigilDesc}";
        }

        public static string GradeLine(WeaponId id, int upgrade, int temper, bool sigil)
        {
            return GradeLine(id, upgrade, temper, sigil, AbsoluteMaxUpgrade);
        }

        public static string GradeLine(WeaponId id, int upgrade, int temper, bool sigil, int maxForDisplay)
        {
            var def = Get(id);
            maxForDisplay = Mathf.Max(1, maxForDisplay);
            var rarity = GetRarity(id);
            var grade = upgrade >= maxForDisplay
                ? (sigil ? "전설각인" : "극한")
                : upgrade >= Mathf.Max(6, maxForDisplay * 2 / 3) ? "강화영웅"
                : upgrade >= 3 ? def.Title : RarityName(rarity);
            return $"[{RarityName(rarity)}]  {grade}  ·  {def.ElementName}  ·  {Stars(upgrade, maxForDisplay)}";
        }

        public static ForgeResult RollForge(int currentLevel, bool precision)
        {
            if (precision)
            {
                return ForgeResult.Success;
            }

            var r = Random.value;
            var great = Mathf.Lerp(0.16f, 0.08f, currentLevel / 8f);
            var perfect = Mathf.Lerp(0.04f, 0.015f, currentLevel / 8f);
            if (r < perfect)
            {
                return ForgeResult.Perfect;
            }

            if (r < perfect + great)
            {
                return ForgeResult.Great;
            }

            return ForgeResult.Success;
        }

        public static int ForgeSteps(ForgeResult result) => result switch
        {
            ForgeResult.Perfect => 3,
            ForgeResult.Great => 2,
            _ => 1
        };

        public static bool Owns(int ownedMask, WeaponId id) => (ownedMask & (1 << (int)id)) != 0;

        public static int WithOwned(int ownedMask, WeaponId id) => ownedMask | (1 << (int)id);

        public static bool HasSigil(int sigilMask, WeaponId id) => (sigilMask & (1 << (int)id)) != 0;

        public static int WithSigil(int sigilMask, WeaponId id) => sigilMask | (1 << (int)id);

        public static int WithoutSigil(int sigilMask, WeaponId id) => sigilMask & ~(1 << (int)id);

        public static bool IsUnlocked(WeaponId id, int playerLevel) =>
            playerLevel >= Get(id).UnlockLevel;

        public static WeaponId? NextUnowned(int ownedMask, int playerLevel = 99)
        {
            WeaponId? best = null;
            var bestCost = int.MaxValue;
            for (var i = 0; i < WeaponCount; i++)
            {
                var id = (WeaponId)i;
                if (Owns(ownedMask, id) || !IsUnlocked(id, playerLevel))
                {
                    continue;
                }

                var cost = Get(id).ShopCost;
                if (cost < bestCost)
                {
                    bestCost = cost;
                    best = id;
                }
            }

            return best;
        }

        public static WeaponId? NextLocked(int ownedMask, int playerLevel)
        {
            WeaponId? best = null;
            var bestLv = int.MaxValue;
            for (var i = 0; i < WeaponCount; i++)
            {
                var id = (WeaponId)i;
                if (Owns(ownedMask, id) || IsUnlocked(id, playerLevel))
                {
                    continue;
                }

                var lv = Get(id).UnlockLevel;
                if (lv < bestLv)
                {
                    bestLv = lv;
                    best = id;
                }
            }

            return best;
        }

        public static WeaponId NextOwned(int ownedMask, WeaponId current)
        {
            var start = ((int)current + 1) % WeaponCount;
            for (var n = 0; n < WeaponCount; n++)
            {
                var i = (start + n) % WeaponCount;
                if ((ownedMask & (1 << i)) != 0)
                {
                    return (WeaponId)i;
                }
            }

            return WeaponId.IronSword;
        }

        public static WeaponId PrevOwned(int ownedMask, WeaponId current)
        {
            var start = ((int)current - 1 + WeaponCount) % WeaponCount;
            for (var n = 0; n < WeaponCount; n++)
            {
                var i = (start - n + WeaponCount) % WeaponCount;
                if ((ownedMask & (1 << i)) != 0)
                {
                    return (WeaponId)i;
                }
            }

            return WeaponId.IronSword;
        }

        /// <summary>보유 무기를 슬롯 순서(해금 순)로 나열.</summary>
        public static void FillOwnedList(int ownedMask, List<WeaponId> into)
        {
            into.Clear();
            for (var i = 0; i < WeaponCount; i++)
            {
                if ((ownedMask & (1 << i)) != 0)
                {
                    into.Add((WeaponId)i);
                }
            }
        }

        public static WeaponId? NthOwned(int ownedMask, int oneBasedIndex)
        {
            if (oneBasedIndex < 1)
            {
                return null;
            }

            var n = 0;
            for (var i = 0; i < WeaponCount; i++)
            {
                if ((ownedMask & (1 << i)) == 0)
                {
                    continue;
                }

                n++;
                if (n == oneBasedIndex)
                {
                    return (WeaponId)i;
                }
            }

            return null;
        }

        public static int OwnedIndexOf(int ownedMask, WeaponId id)
        {
            var n = 0;
            for (var i = 0; i < WeaponCount; i++)
            {
                if ((ownedMask & (1 << i)) == 0)
                {
                    continue;
                }

                n++;
                if ((WeaponId)i == id)
                {
                    return n;
                }
            }

            return 0;
        }

        public static int OwnedCount(int mask)
        {
            var n = 0;
            for (var i = 0; i < WeaponCount; i++)
            {
                if ((mask & (1 << i)) != 0)
                {
                    n++;
                }
            }

            return n;
        }

        public static string NewUnlockNameAtLevel(int level)
        {
            string first = null;
            var count = 0;
            for (var i = 0; i < WeaponCount; i++)
            {
                if (Defs[i].UnlockLevel == level && Defs[i].ShopCost > 0)
                {
                    first ??= Defs[i].Name;
                    count++;
                }
            }

            if (count <= 0)
            {
                return null;
            }

            return count == 1 ? first : $"{first} 외 {count - 1}종";
        }

        public static int[] EnsureUpgradeArray(int[] arr, int legacyLevel, int equippedId)
        {
            if (arr == null || arr.Length < WeaponCount)
            {
                var next = new int[WeaponCount];
                if (arr != null)
                {
                    for (var i = 0; i < Mathf.Min(arr.Length, WeaponCount); i++)
                    {
                        next[i] = arr[i];
                    }
                }

                var eq = Mathf.Clamp(equippedId, 0, WeaponCount - 1);
                if (next[eq] <= 0 && legacyLevel > 0)
                {
                    next[eq] = legacyLevel;
                }

                return next;
            }

            return arr;
        }

        public static int[] EnsureTemperArray(int[] arr)
        {
            if (arr == null || arr.Length < WeaponCount)
            {
                var next = new int[WeaponCount];
                if (arr != null)
                {
                    for (var i = 0; i < Mathf.Min(arr.Length, WeaponCount); i++)
                    {
                        next[i] = arr[i];
                    }
                }

                return next;
            }

            return arr;
        }
    }
}
