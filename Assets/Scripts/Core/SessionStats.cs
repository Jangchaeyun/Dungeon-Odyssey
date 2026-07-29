using UnityEngine;

namespace DungeonOdyssey.Core
{
    /// <summary>탐험 중 세션 통계 — 일시정지·클리어 리포트에 사용.</summary>
    public static class SessionStats
    {
        public static int Kills { get; private set; }
        public static int BossKills { get; private set; }
        public static int DamageDealt { get; private set; }
        public static int DamageTaken { get; private set; }
        public static int Crits { get; private set; }
        public static int PotionsUsed { get; private set; }
        public static int GoldGained { get; private set; }
        public static int RoomsVisited { get; private set; }
        public static float RunStartedAt { get; private set; }
        public static string ThreatLabel { get; private set; } = "";
        public static int HighestCombo { get; private set; }

        public static void BeginRun(string threatLabel = "")
        {
            Kills = 0;
            BossKills = 0;
            DamageDealt = 0;
            DamageTaken = 0;
            Crits = 0;
            PotionsUsed = 0;
            GoldGained = 0;
            RoomsVisited = 0;
            HighestCombo = 0;
            RunStartedAt = Time.unscaledTime;
            ThreatLabel = threatLabel ?? "";
        }

        public static void NoteRoom() => RoomsVisited++;

        public static void NoteKill(bool boss)
        {
            Kills++;
            if (boss)
            {
                BossKills++;
            }
        }

        public static void NoteDamageDealt(int amount, bool crit)
        {
            DamageDealt += Mathf.Max(0, amount);
            if (crit)
            {
                Crits++;
            }
        }

        public static void NoteDamageTaken(int amount) => DamageTaken += Mathf.Max(0, amount);

        public static void NotePotion() => PotionsUsed++;

        public static void NoteGold(int amount) => GoldGained += Mathf.Max(0, amount);

        public static void NoteCombo(int combo) =>
            HighestCombo = Mathf.Max(HighestCombo, combo);

        public static string ElapsedLabel()
        {
            var secs = Mathf.Max(0f, Time.unscaledTime - RunStartedAt);
            var m = Mathf.FloorToInt(secs / 60f);
            var s = Mathf.FloorToInt(secs % 60f);
            return $"{m:00}:{s:00}";
        }

        public static string PauseSummary()
        {
            var threat = string.IsNullOrEmpty(ThreatLabel) ? "" : $"  ·  {ThreatLabel}";
            return
                $"세션  {ElapsedLabel()}{threat}\n" +
                $"처치 {Kills}  ·  보스 {BossKills}  ·  방 {RoomsVisited}\n" +
                $"가한 피해 {DamageDealt}  ·  받은 피해 {DamageTaken}  ·  치명 {Crits}\n" +
                $"획득 골드 +{GoldGained}  ·  포션 {PotionsUsed}" +
                (HighestCombo > 1 ? $"  ·  최고콤보 ×{HighestCombo}" : "");
        }

        /// <summary>클리어 배너용 — 2줄 리포트.</summary>
        public static string BuildClearReport(int bonusGold, int clearCount)
        {
            NoteGold(bonusGold);
            var threat = string.IsNullOrEmpty(ThreatLabel) ? "탐험" : ThreatLabel;
            var line1 =
                $"클리어 #{clearCount}  ·  {threat}  ·  {ElapsedLabel()}  ·  +{bonusGold}G(보너스)  합 +{GoldGained}G";
            var line2 =
                $"처치 {Kills}" +
                (BossKills > 0 ? $"  ·  보스 {BossKills}" : "") +
                $"  ·  방 {RoomsVisited}  ·  피해 {DamageDealt}/{DamageTaken}" +
                (Crits > 0 ? $"  ·  치명 {Crits}" : "") +
                (HighestCombo > 1 ? $"  ·  콤보 ×{HighestCombo}" : "");
            return line1 + "\n" + line2;
        }
    }
}
