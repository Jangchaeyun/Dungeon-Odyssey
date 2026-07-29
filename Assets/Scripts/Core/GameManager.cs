using DungeonOdyssey.Player;
using DungeonOdyssey.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonOdyssey.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private int activeSlot = 0;

        public SaveData CurrentSave { get; private set; }
        public ISaveService SaveService { get; private set; }
        public int LastDeathGoldLost { get; private set; }
        /// <summary>마을 귀환 직후 배너용 짧은 클리어 요약 (1회성).</summary>
        public string PendingTownBanner { get; set; }

        /// <summary>마을 귀환 직후 HUD 힌트용 상세 런 리포트 (1회성).</summary>
        public string PendingRunReportDetail { get; set; }

        /// <summary>마을→던전 입구 연출(걷기·암전) 이어받기.</summary>
        public bool PendingDungeonEntryCinematic { get; set; }

        /// <summary>던전→마을 복귀 연출 이어받기.</summary>
        public bool PendingTownArrivalCinematic { get; set; }

        /// <summary>마을에서 사망 안내를 한 번 표시한 뒤 값을 소비합니다.</summary>
        public int ConsumeLastDeathGoldLost()
        {
            var v = LastDeathGoldLost;
            LastDeathGoldLost = 0;
            return v;
        }

        private float _sessionStartTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                enabled = false;
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureServices();
            _sessionStartTime = Time.realtimeSinceStartup;
            Combat.CharacterPhysics.EnsureLayersCollide();
            ApplyVolumeFromSave();
        }

        private void EnsureServices()
        {
            SaveService ??= new JsonSaveService();
        }

        public void EnsureSaveData()
        {
            if (CurrentSave == null)
            {
                CurrentSave = SaveData.CreateNew(activeSlot);
            }

            CurrentSave.Normalize();
        }

        public void StartNewGame(int slotIndex = 0)
        {
            activeSlot = slotIndex;
            CurrentSave = SaveData.CreateNew(slotIndex);
            if (LocalAccountService.IsLoggedIn)
            {
                CurrentSave.accountUsername = LocalAccountService.CurrentUsername;
                CurrentSave.playerName = LocalAccountService.CurrentDisplayName;
            }

            Persist(SceneNames.Town);
            ApplyVolumeFromSave();
            LoadScene(SceneNames.Town);
        }

        public bool TryContinueGame(int slotIndex = 0)
        {
            var data = SaveService.Load(slotIndex);
            if (data == null)
            {
                return false;
            }

            data.Normalize();
            if (!string.IsNullOrEmpty(data.accountUsername) && LocalAccountService.IsLoggedIn
                && !string.Equals(data.accountUsername, LocalAccountService.CurrentUsername,
                    System.StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (string.IsNullOrEmpty(data.accountUsername) && LocalAccountService.IsLoggedIn)
            {
                data.accountUsername = LocalAccountService.CurrentUsername;
                if (string.IsNullOrEmpty(data.playerName) || data.playerName == "Adventurer")
                {
                    data.playerName = LocalAccountService.CurrentDisplayName;
                }
            }

            activeSlot = slotIndex;
            CurrentSave = data;
            ApplyVolumeFromSave();
            var scene = string.IsNullOrEmpty(data.lastScene) ? SceneNames.Town : data.lastScene;
            if (scene == SceneNames.Dungeon)
            {
                scene = SceneNames.Town;
            }

            LoadScene(scene);
            return true;
        }

        public void EnterDungeon(bool cinematic = false)
        {
            EnsureSaveData();
            PendingDungeonEntryCinematic = cinematic;
            Persist(SceneNames.Dungeon);
            LoadScene(SceneNames.Dungeon, keepFade: cinematic);
        }

        public void ReturnToTownAfterClear(int bonusGold = 40, bool cinematic = false)
        {
            EnsureSaveData();
            CurrentSave.dungeonClears += 1;
            CurrentSave.gold += bonusGold;
            CurrentSave.currentHp = CurrentSave.maxHp;
            var gainedBless = false;
            if (CurrentSave.metaBlessing < 12)
            {
                CurrentSave.metaBlessing += 1;
                gainedBless = true;
            }

            var beforeSkills = CurrentSave.ownedSkillsMask;
            CurrentSave.ownedSkillsMask =
                Player.SkillCatalog.ApplyMetaUnlocks(
                    CurrentSave.ownedSkillsMask, CurrentSave.dungeonClears, CurrentSave.level);
            // 스킬은 무기 귀속(전부 해금) — 클리어 시 SkillMaster 업적만 연결
            AchievementCatalog.TryUnlock(AchievementId.ClearOnce, "첫 클리어");
            if (CurrentSave.dungeonClears >= 5)
            {
                AchievementCatalog.TryUnlock(AchievementId.ClearFive, "클리어 5회");
            }

            if (CurrentSave.ownedSkillsMask == Player.SkillCatalog.FullMask
                || beforeSkills == Player.SkillCatalog.FullMask)
            {
                AchievementCatalog.TryUnlock(AchievementId.SkillMaster, "스킬 마스터");
            }

            PendingRunReportDetail = SessionStats.BuildClearReport(bonusGold, CurrentSave.dungeonClears);
            if (gainedBless)
            {
                PendingRunReportDetail += $"  ·  축복 +{CurrentSave.metaBlessing}";
            }

            PendingTownBanner =
                $"클리어 #{CurrentSave.dungeonClears}  ·  {SessionStats.ElapsedLabel()}  ·  +{bonusGold}G  ·  처치 {SessionStats.Kills}";

            PendingTownArrivalCinematic = cinematic;
            Persist(SceneNames.Town, syncLivePlayer: false);
            LoadScene(SceneNames.Town, keepFade: cinematic);
        }

        public void ReturnToTownFromDeath(float goldLossPercent = 0.2f)
        {
            EnsureSaveData();
            LastDeathGoldLost = Mathf.RoundToInt(CurrentSave.gold * goldLossPercent);
            CurrentSave.gold = Mathf.Max(0, CurrentSave.gold - LastDeathGoldLost);
            CurrentSave.currentHp = CurrentSave.maxHp;
            // 던전에 남은 시체(0 HP)가 SyncToSave로 풀피를 덮어쓰지 않게 함
            Persist(SceneNames.Town, syncLivePlayer: false);
            LoadScene(SceneNames.Town);
        }

        public void AddGold(int amount)
        {
            EnsureSaveData();
            CurrentSave.gold = Mathf.Max(0, CurrentSave.gold + amount);
        }

        public bool TrySpendGold(int amount)
        {
            EnsureSaveData();
            if (CurrentSave.gold < amount)
            {
                return false;
            }

            CurrentSave.gold -= amount;
            return true;
        }

        public void ApplyPlayerStats(int level, int exp, int expToNext, int maxHp, int currentHp, int attackPower)
        {
            EnsureSaveData();
            CurrentSave.level = level;
            CurrentSave.experience = exp;
            CurrentSave.experienceToNext = expToNext;
            CurrentSave.maxHp = maxHp;
            CurrentSave.currentHp = currentHp;
            CurrentSave.attackPower = attackPower;
        }

        public void SetMasterVolume(float volume)
        {
            EnsureSaveData();
            CurrentSave.masterVolume = Mathf.Clamp01(volume);
            CurrentSave.hasAudioPrefs = true;
            AudioListener.volume = CurrentSave.masterVolume;
        }

        public void SetSfxVolume(float volume)
        {
            EnsureSaveData();
            CurrentSave.sfxVolume = Mathf.Clamp01(volume);
            CurrentSave.hasAudioPrefs = true;
        }

        public void SetShakeIntensity(float intensity)
        {
            EnsureSaveData();
            CurrentSave.shakeIntensity = Mathf.Clamp01(intensity);
            CurrentSave.hasAudioPrefs = true;
        }

        public void NoteDepth(int depth)
        {
            EnsureSaveData();
            if (depth > CurrentSave.deepestDepth)
            {
                CurrentSave.deepestDepth = depth;
            }
        }

        public void ApplyVolumeFromSave()
        {
            if (CurrentSave != null)
            {
                AudioListener.volume = Mathf.Clamp01(CurrentSave.masterVolume);
            }
        }

        /// <param name="syncLivePlayer">
        /// true면 씬의 PlayerStats로 세이브를 덮어씀.
        /// 사망 귀환처럼 세이브 HP를 먼저 고친 뒤에는 false로 두어야 0 HP가 다시 쓰이지 않음.
        /// </param>
        public void Persist(string sceneName, bool syncLivePlayer = true)
        {
            EnsureServices();
            EnsureSaveData();
            if (SaveService == null || CurrentSave == null)
            {
                return;
            }

            if (syncLivePlayer)
            {
                var liveStats = Object.FindFirstObjectByType<PlayerStats>();
                liveStats?.SyncToSave();
            }

            CurrentSave.lastScene = sceneName;
            CurrentSave.playTimeSeconds += Time.realtimeSinceStartup - _sessionStartTime;
            _sessionStartTime = Time.realtimeSinceStartup;
            CurrentSave.lastSavedAtUtc = System.DateTime.UtcNow.ToString("o");
            SaveService.Save(CurrentSave);
        }

        public void ReturnToTitle()
        {
            GameUi.IsBlocking = false;
            GameUi.IsPaused = false;
            Time.timeScale = 1f;
            if (CurrentSave != null)
            {
                var scene = SceneManager.GetActiveScene().name;
                Persist(scene == SceneNames.Dungeon ? SceneNames.Town : scene);
            }

            LoadScene(SceneNames.Title);
        }

        public void QuitGame()
        {
            if (CurrentSave != null)
            {
                var scene = SceneManager.GetActiveScene().name;
                Persist(scene == SceneNames.Dungeon ? SceneNames.Town : scene);
            }

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void LoadScene(string sceneName, bool keepFade = false)
        {
            GameUi.IsBlocking = false;
            GameUi.IsPaused = false;
            Time.timeScale = 1f;
            if (!keepFade)
            {
                UI.ScreenFade.Set(0f);
            }

            SceneManager.LoadScene(sceneName);
        }

        private void OnApplicationQuit()
        {
            // 중복 GameManager(Awake 중 Destroy)나 종료 직전 파괴 오브젝트는 스킵
            if (!isActiveAndEnabled || CurrentSave == null)
            {
                return;
            }

            EnsureServices();
            if (SaveService == null)
            {
                return;
            }

            var scene = SceneManager.GetActiveScene().name;
            Persist(scene == SceneNames.Dungeon ? SceneNames.Town : scene);
        }
    }
}
