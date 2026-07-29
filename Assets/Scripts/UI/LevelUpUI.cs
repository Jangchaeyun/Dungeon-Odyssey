using DungeonOdyssey.Combat;
using DungeonOdyssey.Core;
using DungeonOdyssey.Player;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonOdyssey.UI
{
    public class LevelUpUI : MonoBehaviour
    {
        private GameObject _panel;
        private Text _title;
        private Text _hpLabel;
        private Text _atkLabel;
        private Text _spdLabel;
        private Text _critLabel;
        private Text _weaponLabel;
        private PlayerStats _stats;
        private int _pendingChoices;

        public void Bind(GameObject panel, Button hp, Button atk, Button spd, Button crit, Text title = null,
            Button weapon = null)
        {
            _panel = panel;
            _title = title;
            panel.SetActive(false);
            _hpLabel = hp.GetComponentInChildren<Text>();
            _atkLabel = atk.GetComponentInChildren<Text>();
            _spdLabel = spd.GetComponentInChildren<Text>();
            _critLabel = crit.GetComponentInChildren<Text>();
            if (weapon != null)
            {
                _weaponLabel = weapon.GetComponentInChildren<Text>();
                weapon.onClick.AddListener(() => Choose(PlayerStats.LevelBonus.WeaponEnhance));
            }

            hp.onClick.AddListener(() => Choose(PlayerStats.LevelBonus.MaxHp));
            atk.onClick.AddListener(() => Choose(PlayerStats.LevelBonus.Attack));
            spd.onClick.AddListener(() => Choose(PlayerStats.LevelBonus.Speed));
            crit.onClick.AddListener(() => Choose(PlayerStats.LevelBonus.Crit));
        }

        public void Show(PlayerStats stats)
        {
            _stats = stats;
            _pendingChoices = Mathf.Max(1, _pendingChoices + 1);
            GameUi.IsBlocking = true;
            Time.timeScale = 0f;
            RefreshCards();
            StartCoroutine(UiPanelMotion.FadeIn(_panel, 0.22f, 18f));
            RefreshTitle();
            CombatAudio.LevelUp();
        }

        private void RefreshCards()
        {
            if (_stats == null)
            {
                return;
            }

            var hp = _stats.Health.MaxHp;
            var atk = _stats.AttackPower;
            StyleCard(_hpLabel, $"♥ 체력 강화\nHP {hp}→{hp + 36}  ·  ATK +2");
            StyleCard(_atkLabel, $"⚔ 공격 특화\nATK {atk}→{atk + 7}  ·  HP +12");
            StyleCard(_spdLabel, "➤ 신속\n이속 +0.9  ·  ATK +2  ·  HP +12");
            var critPct = Mathf.RoundToInt((_stats.CritChanceBonus + 0.12f) * 100f);
            StyleCard(_critLabel, $"✧ 치명\n치명 {critPct}%→{critPct + 12}%  ·  ATK +3");
            StyleCard(_weaponLabel, _stats.DescribeLevelWeaponBonus());
        }

        private static void StyleCard(Text label, string body)
        {
            if (label == null)
            {
                return;
            }

            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.resizeTextForBestFit = false;
            label.fontSize = 14;
            label.text = body;
        }

        private void RefreshTitle()
        {
            if (_title == null)
            {
                return;
            }

            _title.text = _pendingChoices > 1
                ? $"성장 보너스 선택  ·  남은 {_pendingChoices}"
                : "성장 보너스 선택";
        }

        private void Choose(PlayerStats.LevelBonus bonus)
        {
            if (_stats == null || _pendingChoices <= 0)
            {
                return;
            }

            _stats.ApplyLevelBonus(bonus);
            _pendingChoices--;
            CombatAudio.UiClick();

            if (_pendingChoices > 0)
            {
                RefreshTitle();
                RefreshCards();
                FindFirstObjectByType<HudUI>()?.SetHint($"추가 성장 선택 — 남은 {_pendingChoices}");
                return;
            }

            if (_panel != null)
            {
                StartCoroutine(UiPanelMotion.FadeOut(_panel, 0.12f));
            }

            GameUi.IsBlocking = false;
            if (!GameUi.IsPaused)
            {
                Time.timeScale = 1f;
            }

            FindFirstObjectByType<ClearBannerUI>()
                ?.Show($"성장 완료  ·  Lv {_stats.Level}  ATK {_stats.AttackPower}", ToastKind.Success, 1.8f);
            FindFirstObjectByType<HudUI>()?.SetHint($"성장 완료! Lv {_stats.Level}  ATK {_stats.AttackPower}");
            _stats = null;
        }
    }
}
