using DungeonOdyssey.Combat;
using DungeonOdyssey.Core;
using DungeonOdyssey.Player;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonOdyssey.UI
{
    public class HudUI : MonoBehaviour
    {
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Slider expSlider;
        [SerializeField] private Slider skillCdSlider;
        [SerializeField] private Text hpText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text expText;
        [SerializeField] private Text hintText;
        [SerializeField] private Text roomTitleText;
        [SerializeField] private Text goldText;
        [SerializeField] private Text potionText;
        [SerializeField] private Text clearsText;
        [SerializeField] private Text skillText;
        [SerializeField] private Text questText;
        [SerializeField] private Text styleText;
        [SerializeField] private Text threatText;
        [SerializeField] private Text nextWeaponText;
        [SerializeField] private Image hpFillImage;
        [SerializeField] private Image lowHpVignette;

        private PlayerStats _stats;
        private PlayerSkill _skill;
        private GameObject _hintRoot;
        private float _hpFlash;

        public void Bind(PlayerStats stats)
        {
            if (_stats != null)
            {
                _stats.Health.OnHealthChanged -= HandleHealth;
                _stats.OnExperienceChanged -= HandleExp;
                _stats.OnLevelUp -= HandleLevelUp;
                _stats.OnMetaChanged -= HandleMeta;
            }

            _stats = stats;
            _skill = stats != null ? stats.GetComponent<PlayerSkill>() : null;
            if (_stats == null)
            {
                return;
            }

            _stats.Health.OnHealthChanged += HandleHealth;
            _stats.OnExperienceChanged += HandleExp;
            _stats.OnLevelUp += HandleLevelUp;
            _stats.OnMetaChanged += HandleMeta;

            HandleHealth(_stats.Health.CurrentHp, _stats.Health.MaxHp);
            HandleExp(_stats.Experience, _stats.ExperienceToNext, _stats.Level);
            HandleMeta();
            RefreshQuest();
            if (hintText != null && string.IsNullOrEmpty(hintText.text))
            {
                SetHint("상단 [가방]·[퀘스트]·[상점]  ·  WASD · Space · R · Q · E");
            }
        }

        public void BindWidgets(Slider hp, Text hpLabel, Text level, Text exp, Text hint,
            Text gold = null, Text potion = null, Text clears = null, Text skill = null,
            Text roomTitle = null, Slider expBar = null, Text quest = null,
            Text style = null, Text threat = null, Slider skillCd = null,
            Text nextWeapon = null, Image hpFill = null, Image vignette = null)
        {
            hpSlider = hp;
            expSlider = expBar;
            skillCdSlider = skillCd;
            hpText = hpLabel;
            levelText = level;
            expText = exp;
            hintText = hint;
            goldText = gold;
            potionText = potion;
            clearsText = clears;
            skillText = skill;
            roomTitleText = roomTitle;
            questText = quest;
            styleText = style;
            threatText = threat;
            nextWeaponText = nextWeapon;
            hpFillImage = hpFill;
            lowHpVignette = vignette;
            _hintRoot = hint != null ? hint.transform.parent.gameObject : null;
        }

        public void SetQuestLine(string line)
        {
            if (questText != null)
            {
                questText.text = string.IsNullOrEmpty(line) ? "" : line;
            }

            FindFirstObjectByType<QuestLogUI>()?.Refresh();
        }

        public void RefreshQuest()
        {
            var save = GameManager.Instance?.CurrentSave;
            if (save != null && questText != null)
            {
                questText.text = QuestCatalog.HudLine(save);
            }

            FindFirstObjectByType<QuestLogUI>()?.Refresh();
        }

        public void SetHint(string message)
        {
            if (hintText != null)
            {
                var text = (message ?? "").Replace("\r\n", "\n").Trim();
                // 긴 한 줄은 박스 밖으로 새지 않게 접음
                if (!text.Contains('\n') && text.Length > 56)
                {
                    var cut = text.LastIndexOf(" · ", 52, System.StringComparison.Ordinal);
                    if (cut < 20)
                    {
                        cut = 52;
                    }

                    text = text.Substring(0, cut).TrimEnd(' ', '·') + "…";
                }
                else if (text.Contains('\n'))
                {
                    var nl = text.IndexOf('\n');
                    text = text.Substring(0, nl).Trim();
                    if (text.Length > 56)
                    {
                        text = text.Substring(0, 54) + "…";
                    }
                }

                hintText.text = text;
            }

            RefreshHintVisibility();
        }

        public void RefreshHintVisibility()
        {
            if (_hintRoot == null)
            {
                return;
            }

            var hasText = hintText != null && !string.IsNullOrWhiteSpace(hintText.text);
            _hintRoot.SetActive(hasText && !GameUi.IsBlocking && !GameUi.IsPaused);
        }

        public void SetRoomTitle(string title)
        {
            if (roomTitleText != null)
            {
                roomTitleText.text = title ?? "";
            }
        }

        public void SetThreatChip(string label)
        {
            if (threatText != null)
            {
                threatText.text = string.IsNullOrEmpty(label) ? "" : $"위협  ·  {label}";
            }
        }

        public void SetStyleCombo(int combo, string rank)
        {
            if (styleText == null)
            {
                return;
            }

            if (combo <= 0)
            {
                styleText.text = "";
                return;
            }

            styleText.text = $"{rank}  ×{combo}";
            styleText.color = combo >= 8
                ? new Color(0.95f, 0.55f, 1f)
                : combo >= 5
                    ? new Color(0.85f, 0.7f, 1f)
                    : new Color(0.75f, 0.78f, 0.95f);
        }

        private void Update()
        {
            RefreshHintVisibility();
            StyleCombo.Tick();
            UpdateSkillWidget();
            UpdateLowHpFx();
        }

        private void UpdateSkillWidget()
        {
            if (_skill == null)
            {
                return;
            }

            if (skillText != null)
            {
                skillText.text = _skill.IsReady
                    ? $"R {_skill.EquippedLabel}"
                    : $"R {_skill.EquippedLabel} {_skill.CooldownRemaining:0.0}s";
                skillText.color = _skill.IsReady
                    ? new Color(0.55f, 0.85f, 0.78f)
                    : new Color(0.55f, 0.52f, 0.5f);
            }

            if (skillCdSlider != null)
            {
                if (_skill.IsReady)
                {
                    skillCdSlider.value = 1f;
                }
                else
                {
                    var max = Mathf.Max(0.01f, _skill.CooldownMax);
                    skillCdSlider.value = 1f - Mathf.Clamp01(_skill.CooldownRemaining / max);
                }
            }
        }

        private void UpdateLowHpFx()
        {
            if (lowHpVignette == null || _stats == null)
            {
                return;
            }

            var ratio = _stats.Health.MaxHp > 0
                ? _stats.Health.CurrentHp / (float)_stats.Health.MaxHp
                : 1f;
            var target = ratio < 0.28f ? Mathf.Lerp(0.55f, 0.22f, ratio / 0.28f) : 0f;
            var c = lowHpVignette.color;
            c.a = Mathf.MoveTowards(c.a, target, Time.unscaledDeltaTime * 2.2f);
            // 맥박
            if (target > 0.05f)
            {
                c.a += Mathf.Sin(Time.unscaledTime * 6f) * 0.04f;
            }

            lowHpVignette.color = c;

            if (hpFillImage != null)
            {
                // 평소: 밝은 코랄 · 위험: 더 선명한 적색 (어두운 와인색으로 덮지 않음)
                hpFillImage.color = ratio < 0.3f
                    ? Color.Lerp(new Color(1f, 0.22f, 0.28f), new Color(1f, 0.55f, 0.25f),
                        Mathf.PingPong(Time.unscaledTime * 3.5f, 1f) * (1f - ratio / 0.3f))
                    : new Color(1f, 0.32f, 0.38f, 1f);
            }
        }

        private void OnDestroy()
        {
            if (_stats == null)
            {
                return;
            }

            _stats.Health.OnHealthChanged -= HandleHealth;
            _stats.OnExperienceChanged -= HandleExp;
            _stats.OnLevelUp -= HandleLevelUp;
            _stats.OnMetaChanged -= HandleMeta;
        }

        private void HandleHealth(int current, int max)
        {
            if (hpSlider != null)
            {
                hpSlider.maxValue = max;
                hpSlider.value = current;
            }

            if (hpText != null)
            {
                hpText.text = $"{current}/{max}";
            }
        }

        private void HandleExp(int exp, int toNext, int level)
        {
            if (levelText != null)
            {
                levelText.text = $"Lv {level}";
            }

            if (expText != null && expText.gameObject.activeSelf)
            {
                expText.text = $"{exp}/{toNext}";
            }

            if (expSlider != null)
            {
                expSlider.maxValue = Mathf.Max(1, toNext);
                expSlider.value = Mathf.Clamp(exp, 0, toNext);
            }
        }

        private void HandleLevelUp(int level)
        {
            SetHint($"레벨 업  ·  Lv {level}  ·  추가 성장을 선택하세요");
            FindFirstObjectByType<ClearBannerUI>()
                ?.Show($"LEVEL UP  ·  Lv {level}", ToastKind.Success, 1.8f);
        }

        private void HandleMeta()
        {
            if (_stats == null)
            {
                return;
            }

            if (goldText != null)
            {
                goldText.text = GoldFormat.ForHud(_stats.Gold);
            }

            if (potionText != null)
            {
                potionText.text = $"Q × {_stats.Potions}";
            }

            if (clearsText != null)
            {
                // 긴 무기명은 한 줄만
                var label = _stats.WeaponLabel;
                if (label != null && label.Length > 18)
                {
                    label = label.Substring(0, 17) + "…";
                }

                clearsText.text = $"{label}  ·  ATK {_stats.AttackPower}";
            }

            if (nextWeaponText != null)
            {
                // 등급/별 문구는 HUD에서 제외 — 교체 힌트만
                nextWeaponText.text = _stats.TryPeekNextWeapon(out var next)
                    ? $"휠 / [V]  {next}"
                    : "";
            }
        }
    }
}
