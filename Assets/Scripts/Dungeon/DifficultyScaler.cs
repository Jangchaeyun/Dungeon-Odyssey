using DungeonOdyssey.Save;
using UnityEngine;

namespace DungeonOdyssey.Dungeon
{
    public enum ThreatStyle
    {
        Balanced,
        Swarm,      // 약한 적 다수
        Brutal,     // 강한 일격
        Swift,      // 빠른 적
        EliteHunt,  // 엘리트 비중↑
        Siege       // 보스형 체력
    }

    public readonly struct RunThreat
    {
        public readonly int Tier;
        public readonly ThreatStyle Style;
        public readonly string Label;
        public readonly float HpMul;
        public readonly float DmgMul;
        public readonly float SpdMul;
        public readonly float EliteChance;
        public readonly float ExpMul;
        public readonly float GoldMul;
        public readonly int ExtraSpawns;
        public readonly float SwarmHpScale; // swarm일 때 개체 HP 배율

        public RunThreat(int tier, ThreatStyle style, string label,
            float hp, float dmg, float spd, float elite, float exp, float gold, int extras, float swarmHp = 1f)
        {
            Tier = tier;
            Style = style;
            Label = label;
            HpMul = hp;
            DmgMul = dmg;
            SpdMul = spd;
            EliteChance = elite;
            ExpMul = exp;
            GoldMul = gold;
            ExtraSpawns = extras;
            SwarmHpScale = swarmHp;
        }
    }

    /// <summary>캐릭터 레벨 + 클리어 수로 던전 위협도와 테마를 결정.</summary>
    public static class DifficultyScaler
    {
        public static RunThreat Current { get; private set; } =
            new(1, ThreatStyle.Balanced, "균형", 1f, 1f, 1f, 0.28f, 1f, 1f, 0);

        public static RunThreat BeginRun(SaveData save)
        {
            var level = Mathf.Max(1, save?.level ?? 1);
            var clears = Mathf.Max(0, save?.dungeonClears ?? 0);
            var tier = Mathf.Max(1, level + clears);
            var style = (ThreatStyle)((level + clears * 2) % 5);
            // 레벨 5마다 시즈 비중 상승
            if (level >= 8 && (level + clears) % 7 == 0)
            {
                style = ThreatStyle.Siege;
            }

            // 플레이어 기본성장(ATK+3·HP+20/레벨)보다 느리게 — 레벨업이 이득으로 느껴지게
            var levelScale = 1f + (level - 1) * 0.045f;
            var clearScale = 1f + clears * 0.05f;
            var baseMul = 1f + (levelScale - 1f) * 0.8f + (clearScale - 1f) * 0.85f;

            RunThreat threat = style switch
            {
                ThreatStyle.Swarm => new RunThreat(tier, style, "위협 · 무리",
                    baseMul * 0.72f, baseMul * 0.82f, baseMul * 1.03f,
                    0.14f + level * 0.006f, 1.15f, 1.1f,
                    1 + level / 7, 0.75f),
                ThreatStyle.Brutal => new RunThreat(tier, style, "위협 · 맹공",
                    baseMul * 1.0f, baseMul * 1.12f, baseMul * 0.95f,
                    0.22f + level * 0.01f, 1.2f, 1.15f, level / 8),
                ThreatStyle.Swift => new RunThreat(tier, style, "위협 · 질주",
                    baseMul * 0.88f, baseMul * 0.95f, baseMul * 1.22f,
                    0.24f + level * 0.008f, 1.1f, 1.05f, 1 + level / 8),
                ThreatStyle.EliteHunt => new RunThreat(tier, style, "위협 · 정예",
                    baseMul * 1.06f, baseMul * 1.02f, baseMul * 1.03f,
                    0.36f + level * 0.012f, 1.25f, 1.2f, level / 8),
                ThreatStyle.Siege => new RunThreat(tier, style, "위협 · 공성",
                    baseMul * 1.25f, baseMul * 1.0f, baseMul * 0.9f,
                    0.26f + level * 0.01f, 1.3f, 1.25f, 1),
                _ => new RunThreat(tier, style, "위협 · 균형",
                    baseMul, baseMul * 0.95f, baseMul,
                    0.2f + level * 0.008f + clears * 0.012f, 1f + level * 0.012f, 1f + clears * 0.02f,
                    Mathf.Min(2, level / 6))
            };

            Current = threat;
            return threat;
        }

        public static int ScaleHp(int baseHp, bool elite, bool boss)
        {
            var t = Current;
            var hp = baseHp * t.HpMul;
            if (t.Style == ThreatStyle.Swarm && !boss)
            {
                hp *= t.SwarmHpScale;
            }

            if (elite)
            {
                hp *= 1.55f;
            }

            if (boss)
            {
                hp = hp * 2.85f + 35f + Current.Tier * 4f;
            }

            return Mathf.Max(20, Mathf.RoundToInt(hp));
        }

        public static int ScaleDamage(int baseDmg, bool elite, bool boss)
        {
            var dmg = baseDmg * Current.DmgMul;
            if (elite)
            {
                dmg += 2 + Current.Tier * 0.22f;
            }

            if (boss)
            {
                dmg += 6 + Current.Tier * 0.32f;
            }

            return Mathf.Max(5, Mathf.RoundToInt(dmg));
        }

        public static int ScaleExp(int baseExp, bool elite, bool boss)
        {
            var exp = baseExp * Current.ExpMul * (1f + (Current.Tier - 1) * 0.04f);
            if (elite)
            {
                exp += 10;
            }

            if (boss)
            {
                exp += 40 + Current.Tier * 2;
            }

            return Mathf.Max(5, Mathf.RoundToInt(exp));
        }

        public static float ScaleSpeed(float baseSpd, bool elite, bool boss)
        {
            var spd = baseSpd * Current.SpdMul;
            if (elite)
            {
                spd += 0.22f;
            }

            if (boss)
            {
                spd += 0.12f;
            }

            return Mathf.Clamp(spd, 2.2f, 6.5f);
        }

        public static int ScaleGold(int amount) =>
            Mathf.Max(1, Mathf.RoundToInt(amount * Current.GoldMul));

        public static int RoomEnemyCount(RoomType type, int rollBase)
        {
            var extra = Current.ExtraSpawns;
            if (Current.Style == ThreatStyle.Swarm)
            {
                extra += 1;
            }

            var count = rollBase + Mathf.Min(3, Current.Tier / 3) + Mathf.Min(2, extra);
            if (type == RoomType.Treasure)
            {
                count = Mathf.Max(1, count - 1);
            }

            return Mathf.Clamp(count, 1, 8);
        }

        public static bool RollElite(RoomType type, int index)
        {
            if (type != RoomType.Combat && type != RoomType.Exit)
            {
                return false;
            }

            var chance = Current.EliteChance;
            if (index == 0)
            {
                chance += 0.12f;
            }

            return Random.value < Mathf.Clamp01(chance);
        }

        public static int BossAdds() =>
            1 + Mathf.Min(3, Current.Tier / 4) + (Current.Style == ThreatStyle.Swarm ? 1 : 0);

        public static int SuggestedRoomCount(int level, int clears) =>
            Mathf.Clamp(8 + (level - 1) / 4 + clears / 3, 8, 12);
    }
}
