using DungeonOdyssey.Combat;
using DungeonOdyssey.Core;
using DungeonOdyssey.Player;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonOdyssey.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        private GameObject _panel;
        private Slider _volume;
        private Slider _sfx;
        private Slider _shake;
        private Text _metaText;
        private HudUI _hud;

        public void Bind(GameObject panel, Slider volume, Button resume, Button title, Button quit,
            Text metaText = null, Slider sfx = null, Slider shake = null)
        {
            _panel = panel;
            _volume = volume;
            _sfx = sfx;
            _shake = shake;
            _metaText = metaText;
            panel.SetActive(false);

            resume.onClick.AddListener(Resume);
            title.onClick.AddListener(() =>
            {
                Resume();
                GameManager.Instance?.ReturnToTitle();
            });
            quit.onClick.AddListener(() => GameManager.Instance?.QuitGame());

            WireSlider(_volume, v => GameManager.Instance?.SetMasterVolume(v),
                () => GameManager.Instance != null
                    ? GameManager.Instance.CurrentSave.masterVolume
                    : AudioListener.volume);
            WireSlider(_sfx, v => GameManager.Instance?.SetSfxVolume(v),
                () => GameManager.Instance?.CurrentSave?.sfxVolume ?? 1f);
            WireSlider(_shake, v => GameManager.Instance?.SetShakeIntensity(v),
                () => GameManager.Instance?.CurrentSave?.shakeIntensity ?? 1f);
        }

        private static void WireSlider(Slider slider, System.Action<float> onChanged, System.Func<float> getValue)
        {
            if (slider == null)
            {
                return;
            }

            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = getValue();
            slider.onValueChanged.AddListener(v => onChanged(v));
        }

        private void Update()
        {
            if (GameInput.PauseDown)
            {
                var bag = FindFirstObjectByType<BagUI>();
                if (bag != null && bag.IsOpen)
                {
                    bag.Close();
                    return;
                }

                var quest = FindFirstObjectByType<QuestLogUI>();
                if (quest != null && quest.IsOpen)
                {
                    quest.Close();
                    return;
                }

                var forge = FindFirstObjectByType<ForgeShopUI>();
                if (forge != null && forge.IsOpen)
                {
                    forge.Hide();
                    return;
                }

                if (GameUi.IsPaused)
                {
                    Resume();
                }
                else if (!GameUi.IsBlocking)
                {
                    Pause();
                }
            }
        }

        public void Pause()
        {
            if (_panel == null)
            {
                return;
            }

            FindFirstObjectByType<PlayerStats>()?.SyncToSave();
            GameUi.IsPaused = true;
            Time.timeScale = 0f;
            RefreshMeta();
            SyncSliders();
            StartCoroutine(UiPanelMotion.FadeIn(_panel, 0.18f, 10f));
            CombatAudio.UiClick();
        }

        public void Resume()
        {
            GameUi.IsPaused = false;
            if (_panel != null)
            {
                StartCoroutine(UiPanelMotion.FadeOut(_panel, 0.12f));
            }

            if (!GameUi.IsBlocking)
            {
                Time.timeScale = 1f;
            }

            FindFirstObjectByType<PlayerStats>()?.SyncToSave();
            if (GameManager.Instance != null)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                GameManager.Instance.Persist(scene == SceneNames.Dungeon ? SceneNames.Town : scene);
                CombatAudio.Save();
            }

            _hud ??= FindFirstObjectByType<HudUI>();
            _hud?.SetHint("저장됨");
            CombatAudio.UiClick();
        }

        private void SyncSliders()
        {
            if (GameManager.Instance?.CurrentSave == null)
            {
                return;
            }

            var s = GameManager.Instance.CurrentSave;
            _volume?.SetValueWithoutNotify(s.masterVolume);
            _sfx?.SetValueWithoutNotify(s.sfxVolume);
            _shake?.SetValueWithoutNotify(s.shakeIntensity);
        }

        private void RefreshMeta()
        {
            if (_metaText == null || GameManager.Instance?.CurrentSave == null)
            {
                return;
            }

            var s = GameManager.Instance.CurrentSave;
            var mins = Mathf.FloorToInt(s.playTimeSeconds / 60f);
            var player = FindFirstObjectByType<PlayerStats>();
            var weapon = player != null ? player.WeaponLabel : $"무기 #{s.equippedWeaponId}";
            var skill = player != null
                ? player.SkillLabel
                : SkillCatalog.WeaponSkillName((WeaponId)Mathf.Clamp(s.equippedWeaponId, 0, WeaponCatalog.WeaponCount - 1));
            var skillDesc = player != null
                ? SkillCatalog.ForWeaponDef(player.SkillSourceWeapon).Description
                : "";
            var ach = AchievementCatalog.CountUnlocked(s);
            var quest = QuestCatalog.HudLine(s);
            var achLine = AchievementCatalog.CodexLine(s);
            var session = SessionStats.Kills > 0 || SessionStats.RoomsVisited > 0
                ? $"\n── 이번 탐험 ──\n{SessionStats.PauseSummary()}"
                : "";
            var style = StyleCombo.Combo > 0 ? $"\n스타일  {StyleCombo.Rank} ×{StyleCombo.Combo}" : "";

            _metaText.text =
                $"Lv {s.level}   ATK {s.attackPower}   HP {s.currentHp}/{s.maxHp}\n" +
                $"{weapon}\n" +
                $"스킬  {skill}" + (string.IsNullOrEmpty(skillDesc) ? "" : $"\n  · {skillDesc}") + "\n" +
                $"클리어 {s.dungeonClears}  ·  깊이 {s.deepestDepth}  ·  축복 +{s.metaBlessing}\n" +
                $"{quest}\n" +
                $"업적 {ach}/6  ·  {achLine}\n" +
                $"{s.gold} G  ·  포션 ×{s.potions}  ·  스트릭 {s.bestKillStreak}  ·  {mins}분" +
                style + session;
        }
    }
}
