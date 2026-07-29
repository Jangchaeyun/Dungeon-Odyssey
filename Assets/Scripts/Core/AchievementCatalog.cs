using DungeonOdyssey.Save;
using DungeonOdyssey.UI;
using UnityEngine;

namespace DungeonOdyssey.Core
{
    public enum AchievementId
    {
        FirstBlood = 0,      // 첫 몬스터
        FirstBoss = 1,       // 첫 보스
        ClearOnce = 2,       // 첫 클리어
        ClearFive = 3,       // 5클리어
        FullForge = 4,       // 무기 최대 강화
        SkillMaster = 5      // 스킬 전부 습득
    }

    public static class AchievementCatalog
    {
        public static int RewardGold(AchievementId id) => id switch
        {
            AchievementId.FirstBlood => 30,
            AchievementId.FirstBoss => 80,
            AchievementId.ClearOnce => 100,
            AchievementId.ClearFive => 250,
            AchievementId.FullForge => 120,
            AchievementId.SkillMaster => 150,
            _ => 40
        };

        public static int CountUnlocked(SaveData save)
        {
            if (save == null)
            {
                return 0;
            }

            var n = 0;
            for (var i = 0; i < 6; i++)
            {
                if ((save.achievementMask & (1 << i)) != 0)
                {
                    n++;
                }
            }

            return n;
        }

        public static bool Has(SaveData save, AchievementId id) =>
            save != null && (save.achievementMask & (1 << (int)id)) != 0;

        public static bool TryUnlock(AchievementId id, string toast)
        {
            var gm = GameManager.Instance;
            if (gm?.CurrentSave == null || Has(gm.CurrentSave, id))
            {
                return false;
            }

            gm.CurrentSave.achievementMask |= 1 << (int)id;
            var gold = RewardGold(id);
            if (gold > 0)
            {
                gm.AddGold(gold);
                SessionStats.NoteGold(gold);
            }

            var reward = gold > 0 ? $"  ·  +{gold}G" : "";
            Object.FindFirstObjectByType<ClearBannerUI>()
                ?.Show($"업적 · {toast}{reward}", ToastKind.Achievement, 2.8f);
            Object.FindFirstObjectByType<HudUI>()?.SetHint($"업적 해금 · {toast}{reward}");
            Combat.CombatAudio.LevelUp();
            return true;
        }

        public static string CodexLine(SaveData save)
        {
            if (save == null)
            {
                return "없음";
            }

            var names = new[] { "첫피", "보스", "클리어", "5회", "강화", "스킬" };
            var parts = new System.Collections.Generic.List<string>();
            for (var i = 0; i < 6; i++)
            {
                var done = (save.achievementMask & (1 << i)) != 0;
                parts.Add(done ? $"✓{names[i]}" : $"·{names[i]}");
            }

            return string.Join(" ", parts);
        }
    }
}
