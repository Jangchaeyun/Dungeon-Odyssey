using System;
using DungeonOdyssey.Combat;
using DungeonOdyssey.Save;
using DungeonOdyssey.UI;
using UnityEngine;

namespace DungeonOdyssey.Core
{
    public enum QuestId
    {
        None = -1,
        FirstClear = 0,       // 던전 1회 클리어
        SlayEight = 1,        // 몬스터 8마리
        BossBounty = 2,       // 보스 처치
        ShrineVisit = 3,      // 성소 이용
        TreasureHunt = 4,     // 상자 열기
        DeepDive = 5,         // 깊이 6 도달
        ForgeOnce = 6,        // 무기 1회 강화
        ComboKing = 7         // 킬 스트릭 5
    }

    public enum QuestGoal
    {
        ClearDungeon,
        KillMonsters,
        KillBoss,
        UseShrine,
        OpenChest,
        ReachDepth,
        UpgradeWeapon,
        KillStreak
    }

    public readonly struct QuestDef
    {
        public readonly QuestId Id;
        public readonly string Title;
        public readonly string Brief;
        public readonly QuestGoal Goal;
        public readonly int Target;
        public readonly int GoldReward;
        public readonly int PotionReward;
        public readonly bool GrantsLuck; // 다음 탐험 골드 +25%

        public QuestDef(QuestId id, string title, string brief, QuestGoal goal, int target,
            int gold, int potions = 0, bool luck = false)
        {
            Id = id;
            Title = title;
            Brief = brief;
            Goal = goal;
            Target = target;
            GoldReward = gold;
            PotionReward = potions;
            GrantsLuck = luck;
        }
    }

    /// <summary>미라 퀘스트 · 일일 현상금 · 런 중 진행도.</summary>
    public static class QuestCatalog
    {
        public static readonly QuestDef[] StoryQuests =
        {
            new(QuestId.FirstClear, "첫 귀환", "던전을 클리어하고 포탈로 마을에 돌아와.",
                QuestGoal.ClearDungeon, 1, 40, 1),
            new(QuestId.SlayEight, "사냥 의뢰", "괴수 8마리를 처치해.",
                QuestGoal.KillMonsters, 8, 35),
            new(QuestId.BossBounty, "현상금 · 보스", "보스 방에서 우두머리를 쓰러뜨려.",
                QuestGoal.KillBoss, 1, 60, 1, true),
            new(QuestId.ShrineVisit, "잊힌 성소", "성소에서 축복·도박·안식 중 하나를 선택해.",
                QuestGoal.UseShrine, 1, 30),
            new(QuestId.TreasureHunt, "보물 탐색", "보물상자를 열어.",
                QuestGoal.OpenChest, 1, 45, 0, true),
            new(QuestId.DeepDive, "심연 발걸음", "한 번의 탐험에서 깊이 6까지 나아가.",
                QuestGoal.ReachDepth, 6, 55, 1),
            new(QuestId.ForgeOnce, "대장간의 숨결", "Borin에게 무기를 한 번 강화해.",
                QuestGoal.UpgradeWeapon, 1, 25, 1),
            new(QuestId.ComboKing, "연속 타격", "킬 스트릭 5를 달성해.",
                QuestGoal.KillStreak, 5, 50, 0, true)
        };

        // 런 세션 (던전 한 판)
        public static int RunKills { get; private set; }
        public static int RunBossKills { get; private set; }
        public static int KillStreak { get; private set; }
        public static int BestStreakThisRun { get; private set; }
        public static bool UsedShrine { get; private set; }
        public static bool OpenedChest { get; private set; }
        public static int MaxDepthThisRun { get; private set; }
        public static bool LuckActiveThisRun { get; private set; }

        public static QuestDef Get(QuestId id)
        {
            foreach (var q in StoryQuests)
            {
                if (q.Id == id)
                {
                    return q;
                }
            }

            return StoryQuests[0];
        }

        public static bool IsCompleted(SaveData save, QuestId id) =>
            save != null && id != QuestId.None && (save.completedQuestMask & (1 << (int)id)) != 0;

        public static void BeginDungeonRun()
        {
            RunKills = 0;
            RunBossKills = 0;
            KillStreak = 0;
            BestStreakThisRun = 0;
            UsedShrine = false;
            OpenedChest = false;
            MaxDepthThisRun = 1;

            var save = GameManager.Instance?.CurrentSave;
            var stats = UnityEngine.Object.FindFirstObjectByType<Player.PlayerStats>();
            LuckActiveThisRun = false;
            if (stats != null && stats.CountItem(Player.InvItemKind.Luck) > 0)
            {
                LuckActiveThisRun = stats.TryConsumeLuck(1);
            }
            else if (save != null && save.luckCharges > 0)
            {
                LuckActiveThisRun = true;
                save.luckCharges = Mathf.Max(0, save.luckCharges - 1);
            }
        }

        public static int GoldMultiplierPercent() => LuckActiveThisRun ? 125 : 100;

        public static int ApplyGoldBonus(int baseGold)
        {
            if (!LuckActiveThisRun || baseGold <= 0)
            {
                return baseGold;
            }

            return Mathf.RoundToInt(baseGold * 1.25f);
        }

        public static void EnsureDaily(SaveData save)
        {
            if (save == null)
            {
                return;
            }

            var day = DateTime.UtcNow.DayOfYear + DateTime.UtcNow.Year * 1000;
            if (save.dailyQuestDay == day)
            {
                return;
            }

            save.dailyQuestDay = day;
            save.dailyProgress = 0;
            save.dailyClaimed = false;
            // 일일은 스토리 퀘스트 풀에서 회전 (첫 클리어 제외)
            var idx = (day % (StoryQuests.Length - 1)) + 1;
            save.dailyQuestId = (int)StoryQuests[idx].Id;
        }

        public static string HudLine(SaveData save)
        {
            if (save == null)
            {
                return "";
            }

            // 수락한 의뢰만 HUD에 표시 (일일·제안은 자동 등록되지 않음)
            if (save.activeQuestId >= 0)
            {
                var q = Get((QuestId)save.activeQuestId);
                var p = Mathf.Clamp(save.questProgress, 0, q.Target);
                return $"퀘스트  {q.Title}  {p}/{q.Target}";
            }

            return "미라에게 의뢰 수락 [E]";
        }

        /// <summary>플레이어용 퀘스트 로그 카드 데이터.</summary>
        public readonly struct LogCard
        {
            public readonly string Tag;
            public readonly string Title;
            public readonly string Detail;
            public readonly string Progress;
            public readonly string Status;
            public readonly Color Accent;

            public LogCard(string tag, string title, string detail, string progress, string status, Color accent)
            {
                Tag = tag;
                Title = title;
                Detail = detail;
                Progress = progress;
                Status = status;
                Accent = accent;
            }
        }

        public static void BuildPlayerLog(SaveData save, out LogCard active, out LogCard daily, out LogCard offer,
            out string footer)
        {
            EnsureDaily(save);

            if (save.activeQuestId >= 0)
            {
                var q = Get((QuestId)save.activeQuestId);
                var p = Mathf.Clamp(save.questProgress, 0, q.Target);
                var done = p >= q.Target;
                active = new LogCard(
                    "진행 중",
                    q.Title,
                    q.Brief,
                    $"{p} / {q.Target}",
                    done ? "완료 · 미라에게 보고" : "탐험 중",
                    done ? new Color(0.55f, 0.9f, 0.7f) : new Color(0.45f, 0.85f, 0.9f));
            }
            else
            {
                // 수락 전에는 진행 카드 자체를 숨김
                active = default;
            }

            var d = Get((QuestId)Mathf.Max(0, save.dailyQuestId));
            var dp = Mathf.Clamp(save.dailyProgress, 0, d.Target);
            var dailyDone = dp >= d.Target;
            if (save.dailyClaimed)
            {
                daily = new LogCard(
                    "일일",
                    d.Title,
                    "오늘 보상을 이미 받았다. 내일 다시 갱신된다.",
                    $"{d.Target} / {d.Target}",
                    "완료",
                    new Color(0.5f, 0.72f, 0.65f));
            }
            else if (dailyDone)
            {
                daily = new LogCard(
                    "일일",
                    d.Title,
                    d.Brief,
                    $"{dp} / {d.Target}",
                    $"보상 대기 · {d.GoldReward}G",
                    new Color(1f, 0.78f, 0.4f));
            }
            else
            {
                daily = new LogCard(
                    "일일",
                    d.Title,
                    d.Brief,
                    $"{dp} / {d.Target}",
                    $"보상 {d.GoldReward}G",
                    new Color(0.55f, 0.82f, 0.75f));
            }

            var next = NextOffer(save);
            if (next.HasValue)
            {
                var o = next.Value;
                var reward = $"보상 {o.GoldReward}G";
                if (o.PotionReward > 0)
                {
                    reward += $" · 포션 +{o.PotionReward}";
                }

                if (o.GrantsLuck)
                {
                    reward += " · 행운부적";
                }

                offer = new LogCard(
                    "다음 의뢰",
                    o.Title,
                    o.Brief,
                    "",
                    reward,
                    new Color(0.7f, 0.78f, 0.95f));
            }
            else
            {
                offer = new LogCard(
                    "다음 의뢰",
                    "모든 의뢰 완료",
                    "스토리 의뢰를 모두 마쳤다. 일일 현상금을 확인하자.",
                    "",
                    "완료",
                    new Color(0.55f, 0.6f, 0.65f));
            }

            var extras = new System.Text.StringBuilder();
            extras.Append("마을 미라에게 [E] 로 수락 · 보상 수령");
            if (save.luckCharges > 0)
            {
                extras.Append($"  ·  행운 ×{save.luckCharges}");
            }

            if (save.bestKillStreak > 0)
            {
                extras.Append($"  ·  최고 스트릭 {save.bestKillStreak}");
            }

            footer = extras.ToString();
        }

        public static string BoardText(SaveData save)
        {
            BuildBoardUi(save, out var status, out var actions);
            var sb = status + "\n\n";
            for (var i = 0; i < actions.Count; i++)
            {
                sb += $"[{i + 1}] {actions[i].label}\n";
            }

            return sb.TrimEnd();
        }

        /// <summary>미라 의뢰판 UI용 — 상태 카드 + 1~3 액션.</summary>
        public static void BuildBoardUi(SaveData save, out string status,
            out System.Collections.Generic.List<(string label, bool enabled)> actions,
            string notice = null)
        {
            EnsureDaily(save);
            var active = save.activeQuestId >= 0
                ? ProgressLine(Get((QuestId)save.activeQuestId), save.questProgress)
                : "없음 — 의뢰 수락 후 진행에 표시";
            var daily = Get((QuestId)Mathf.Max(0, save.dailyQuestId));
            var dailyDone = save.dailyProgress >= daily.Target;
            var dailyLine = save.dailyClaimed
                ? $"완료 · 내일 갱신 ({daily.Title})"
                : dailyDone
                    ? $"{ProgressLine(daily, save.dailyProgress)} · 보상 대기!"
                    : $"{ProgressLine(daily, save.dailyProgress)} · {daily.GoldReward}G";

            var offer = NextOffer(save);
            var offerBlock = offer.HasValue
                ? $"{offer.Value.Title} — {offer.Value.Brief}\n" +
                  $"보상 {offer.Value.GoldReward}G" +
                  (offer.Value.PotionReward > 0 ? $" · 포션+{offer.Value.PotionReward}" : "") +
                  (offer.Value.GrantsLuck ? " · 행운부적" : "")
                : "모든 의뢰 완료. 일일 현상금을 확인해.";

            var extras = "";
            if (save.luckCharges > 0)
            {
                extras += $"행운 ×{save.luckCharges}";
            }

            if (save.bestKillStreak > 0)
            {
                extras += extras.Length > 0
                    ? $"  ·  스트릭 {save.bestKillStreak}"
                    : $"스트릭 {save.bestKillStreak}";
            }

            status =
                (string.IsNullOrEmpty(notice) ? "" : notice.TrimEnd() + "\n\n") +
                $"진행  {active}\n" +
                $"일일  {dailyLine}" +
                (extras.Length > 0 ? $"\n{extras}" : "") +
                $"\n제안  {offerBlock}";

            string acceptLabel;
            bool acceptOk;
            if (offer.HasValue)
            {
                acceptLabel = save.activeQuestId >= 0
                    ? $"의뢰 교체  ·  {offer.Value.Title}"
                    : $"의뢰 수락  ·  {offer.Value.Title}";
                acceptOk = true;
            }
            else
            {
                acceptLabel = "의뢰 수락  ·  남은 의뢰 없음";
                acceptOk = false;
            }

            string dailyLabel;
            bool dailyOk;
            if (save.dailyClaimed)
            {
                dailyLabel = "일일 보상  ·  내일 다시";
                dailyOk = false;
            }
            else if (dailyDone)
            {
                dailyLabel = $"일일 보상 수령  ·  +{daily.GoldReward}G" +
                             (daily.GrantsLuck ? " · 행운" : "");
                dailyOk = true;
            }
            else
            {
                dailyLabel = $"일일 보상  ·  {save.dailyProgress}/{daily.Target}";
                dailyOk = false;
            }

            actions = new System.Collections.Generic.List<(string, bool)>
            {
                (acceptLabel, acceptOk),
                (dailyLabel, dailyOk),
                ("모험 조언  ·  Mira의 팁", true)
            };
        }

        public static bool TryAcceptOffer(SaveData save, out string msg)
        {
            var offer = NextOffer(save);
            if (!offer.HasValue)
            {
                msg = "더 줄 의뢰가 없다. 일일 현상금을 확인해.";
                return false;
            }

            save.activeQuestId = (int)offer.Value.Id;
            save.questProgress = SeedProgress(offer.Value, save);
            msg = $"의뢰 수락 · {offer.Value.Title}\n{offer.Value.Brief}";
            Toast($"퀘스트 · {offer.Value.Title}");
            CombatAudio.UiClick();
            return true;
        }

        public static bool TryClaimDaily(SaveData save, out string msg)
        {
            EnsureDaily(save);
            if (save.dailyClaimed)
            {
                msg = "오늘 일일 보상은 이미 받았다. 내일 다시 오도록.";
                return false;
            }

            var q = Get((QuestId)save.dailyQuestId);
            if (save.dailyProgress < q.Target)
            {
                msg = $"아직이다. {q.Title} — {save.dailyProgress}/{q.Target}";
                return false;
            }

            GrantReward(save, q, daily: true);
            save.dailyClaimed = true;
            msg = $"일일 완료! +{q.GoldReward}G" +
                  (q.PotionReward > 0 ? $"  포션+{q.PotionReward}" : "") +
                  (q.GrantsLuck ? "  행운부적" : "");
            Toast($"일일 · {q.Title}");
            return true;
        }

        public static string Advice(SaveData save)
        {
            var clears = save?.dungeonClears ?? 0;
            return clears switch
            {
                0 => "방의 적을 모두 쓰러뜨려야 문이 열린다.\n" +
                     "보스·성소·보물을 찾아, 출구 포탈로 돌아와.\n" +
                     "왼쪽 Borin에게서 회복·강화·스킬을 익힐 수 있다.",
                <= 2 => "성소의 도박은 조심하고, 보스는 부하를 먼저 정리해.\n" +
                        "연속으로 처치하면 킬 스트릭 보너스가 붙는다.\n" +
                        "무기를 바꿔가며 스킬(R)을 활용해봐.",
                _ => $"클리어 {clears}회… 심연이 널 기억한다.\n" +
                     "의뢰판과 일일 현상금으로 성장 속도를 올려.\n" +
                     "행운부적이 있으면 다음 탐험 골드가 늘어난다."
            };
        }

        // —— 진행 이벤트 ——

        public static void NotifyKill(bool isBoss)
        {
            RunKills++;
            KillStreak++;
            BestStreakThisRun = Mathf.Max(BestStreakThisRun, KillStreak);
            if (isBoss)
            {
                RunBossKills++;
            }

            var save = GameManager.Instance?.CurrentSave;
            if (save != null && KillStreak > save.bestKillStreak)
            {
                save.bestKillStreak = KillStreak;
            }

            if (KillStreak == 3 || KillStreak == 5 || KillStreak == 8)
            {
                Toast($"킬 스트릭 ×{KillStreak}!");
                CombatAudio.LevelUp();
            }

            if (KillStreak == 8)
            {
                var stats = UnityEngine.Object.FindFirstObjectByType<Player.PlayerStats>();
                if (stats != null)
                {
                    stats.AddPotions(1);
                }
                else if (save != null)
                {
                    save.potions += 1;
                }

                Toast("스트릭 보너스 · 포션 +1");
            }

            Bump(QuestGoal.KillMonsters, 1);
            if (isBoss)
            {
                Bump(QuestGoal.KillBoss, 1);
            }

            BumpAbsolute(QuestGoal.KillStreak, KillStreak);
        }

        public static void NotifyPlayerHurt()
        {
            KillStreak = 0;
        }

        public static void NotifyDepth(int depth)
        {
            MaxDepthThisRun = Mathf.Max(MaxDepthThisRun, depth);
            BumpAbsolute(QuestGoal.ReachDepth, MaxDepthThisRun);
        }

        public static void NotifyShrine()
        {
            UsedShrine = true;
            Bump(QuestGoal.UseShrine, 1);
        }

        public static void NotifyChest()
        {
            OpenedChest = true;
            Bump(QuestGoal.OpenChest, 1);
        }

        public static void NotifyUpgrade()
        {
            Bump(QuestGoal.UpgradeWeapon, 1);
        }

        public static void NotifyDungeonClear()
        {
            Bump(QuestGoal.ClearDungeon, 1);
            // 스트릭 보너스 정산은 DungeonManager / GameManager에서 골드에 합산
        }

        public static int StreakBonusGold()
        {
            if (BestStreakThisRun >= 8)
            {
                return 25;
            }

            if (BestStreakThisRun >= 5)
            {
                return 15;
            }

            if (BestStreakThisRun >= 3)
            {
                return 8;
            }

            return 0;
        }

        private static void Bump(QuestGoal goal, int amount)
        {
            var save = GameManager.Instance?.CurrentSave;
            if (save == null)
            {
                return;
            }

            EnsureDaily(save);

            if (save.activeQuestId >= 0)
            {
                var q = Get((QuestId)save.activeQuestId);
                if (q.Goal == goal && save.questProgress < q.Target)
                {
                    save.questProgress = Mathf.Min(q.Target, save.questProgress + amount);
                    if (save.questProgress >= q.Target)
                    {
                        CompleteActive(save, q);
                    }
                    else
                    {
                        RefreshHud(save);
                    }
                }
            }

            if (!save.dailyClaimed && save.dailyQuestId >= 0)
            {
                var d = Get((QuestId)save.dailyQuestId);
                if (d.Goal == goal && save.dailyProgress < d.Target)
                {
                    save.dailyProgress = Mathf.Min(d.Target, save.dailyProgress + amount);
                    if (save.dailyProgress >= d.Target)
                    {
                        Toast($"일일 달성 · {d.Title} — Mira에게 수령");
                        CombatAudio.Coin();
                    }

                    RefreshHud(save);
                }
            }
        }

        private static void BumpAbsolute(QuestGoal goal, int value)
        {
            var save = GameManager.Instance?.CurrentSave;
            if (save == null)
            {
                return;
            }

            EnsureDaily(save);

            if (save.activeQuestId >= 0)
            {
                var q = Get((QuestId)save.activeQuestId);
                if (q.Goal == goal)
                {
                    var before = save.questProgress;
                    save.questProgress = Mathf.Max(save.questProgress, Mathf.Min(q.Target, value));
                    if (before < q.Target && save.questProgress >= q.Target)
                    {
                        CompleteActive(save, q);
                    }
                    else if (save.questProgress != before)
                    {
                        RefreshHud(save);
                    }
                }
            }

            if (!save.dailyClaimed && save.dailyQuestId >= 0)
            {
                var d = Get((QuestId)save.dailyQuestId);
                if (d.Goal == goal)
                {
                    var before = save.dailyProgress;
                    save.dailyProgress = Mathf.Max(save.dailyProgress, Mathf.Min(d.Target, value));
                    if (before < d.Target && save.dailyProgress >= d.Target)
                    {
                        Toast($"일일 달성 · {d.Title} — Mira에게 수령");
                        CombatAudio.Coin();
                    }

                    if (save.dailyProgress != before)
                    {
                        RefreshHud(save);
                    }
                }
            }
        }

        private static void CompleteActive(SaveData save, QuestDef q)
        {
            if (IsCompleted(save, q.Id))
            {
                save.activeQuestId = -1;
                save.questProgress = 0;
                RefreshHud(save);
                return;
            }

            save.completedQuestMask |= 1 << (int)q.Id;
            GrantReward(save, q, daily: false);
            save.activeQuestId = -1;
            save.questProgress = 0;
            Toast($"퀘스트 완료 · {q.Title}!");
            CombatAudio.LevelUp();
            RefreshHud(save);
        }

        private static void GrantReward(SaveData save, QuestDef q, bool daily)
        {
            GameManager.Instance?.AddGold(q.GoldReward);
            if (q.PotionReward > 0)
            {
                var stats = UnityEngine.Object.FindFirstObjectByType<Player.PlayerStats>();
                if (stats != null)
                {
                    stats.AddPotions(q.PotionReward);
                }
                else
                {
                    save.potions += q.PotionReward;
                }
            }

            if (q.GrantsLuck)
            {
                var statsLuck = UnityEngine.Object.FindFirstObjectByType<Player.PlayerStats>();
                if (statsLuck != null)
                {
                    statsLuck.TryAddInvItem(Player.InvSlot.MakeLuck(1), out _);
                }
                else
                {
                    save.luckCharges = Mathf.Min(9, save.luckCharges + 1);
                }
            }

            UnityEngine.Object.FindFirstObjectByType<ClearBannerUI>()
                ?.Show($"{(daily ? "일일" : "퀘스트")} · {q.Title}  +{q.GoldReward}G", ToastKind.Success, 2.3f);
            UnityEngine.Object.FindFirstObjectByType<Player.PlayerStats>()?.NotifyMetaChanged();
        }

        private static QuestDef? NextOffer(SaveData save)
        {
            foreach (var q in StoryQuests)
            {
                if (!IsCompleted(save, q.Id) && save.activeQuestId != (int)q.Id)
                {
                    return q;
                }
            }

            return null;
        }

        private static int SeedProgress(QuestDef q, SaveData save) => q.Goal switch
        {
            QuestGoal.ClearDungeon => 0,
            QuestGoal.UpgradeWeapon => 0,
            QuestGoal.KillMonsters => 0,
            QuestGoal.KillBoss => 0,
            QuestGoal.UseShrine => UsedShrine ? 1 : 0,
            QuestGoal.OpenChest => OpenedChest ? 1 : 0,
            QuestGoal.ReachDepth => Mathf.Min(q.Target, MaxDepthThisRun),
            QuestGoal.KillStreak => Mathf.Min(q.Target, BestStreakThisRun),
            _ => 0
        };

        private static string ProgressLine(QuestDef q, int progress) =>
            $"[{q.Title}] {Mathf.Clamp(progress, 0, q.Target)}/{q.Target}";

        private static void Toast(string msg)
        {
            UnityEngine.Object.FindFirstObjectByType<HudUI>()?.SetHint(msg);
        }

        private static void RefreshHud(SaveData save)
        {
            UnityEngine.Object.FindFirstObjectByType<HudUI>()?.SetQuestLine(HudLine(save));
        }
    }
}
