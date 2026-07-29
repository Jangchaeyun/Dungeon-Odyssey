using DungeonOdyssey.Combat;
using DungeonOdyssey.Core;
using DungeonOdyssey.Player;
using DungeonOdyssey.UI;
using DungeonOdyssey.View3D;
using UnityEngine;

namespace DungeonOdyssey.Dungeon
{
    public class TreasureChest : MonoBehaviour
    {
        private bool _opened;
        private bool _playerInside;
        private int _goldReward = 35;
        private int _potionChance = 50;
        private WorldLabel _label;

        public void Configure(int gold, int potionChancePercent = 50)
        {
            _goldReward = gold;
            _potionChance = potionChancePercent;
        }

        private void Update()
        {
            if (_opened || !_playerInside || GameUi.IsBlocking || GameUi.IsPaused)
            {
                return;
            }

            if (GameInput.InteractDown)
            {
                if (DungeonManager.Instance?.CurrentRoom != null && !DungeonManager.Instance.CurrentRoom.Cleared)
                {
                    FindFirstObjectByType<HudUI>()?.SetHint("상자는 방을 클리어한 뒤 열 수 있다");
                    return;
                }

                Open();
            }
        }

        private void Open()
        {
            _opened = true;
            var gold = QuestCatalog.ApplyGoldBonus(_goldReward);
            GameManager.Instance?.AddGold(gold);
            FloatingText.Gold(transform.position + Vector3.up * 1.3f, gold);
            QuestCatalog.NotifyChest();
            FindFirstObjectByType<HudUI>()?.RefreshQuest();
            var stats = FindFirstObjectByType<PlayerStats>();
            var gotPotion = Random.Range(0, 100) < _potionChance;
            if (gotPotion && stats != null)
            {
                stats.AddPotions(1);
            }

            stats?.NotifyMetaChanged();
            CombatAudio.Coin();
            CombatAudio.HitCrit();
            AttackFx.PlayImpact3D(transform.position + Vector3.up * 0.8f, true);

            var hud = FindFirstObjectByType<HudUI>();
            hud?.SetHint(gotPotion
                ? $"상자 획득! +{gold}G · 포션 +1"
                : $"상자 획득! +{gold}G");

            // 열림 연출
            transform.localScale = new Vector3(1.1f, 0.7f, 1.1f);
            var label = GetComponentInChildren<WorldLabel>();
            label?.SetText("열림");
            label?.SetColor(new Color(0.6f, 0.6f, 0.6f));
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_opened || !other.CompareTag("Player"))
            {
                return;
            }

            _playerInside = true;
            _label ??= GetComponentInChildren<WorldLabel>();
            _label?.SetHighlight(true);
            FindFirstObjectByType<HudUI>()?.SetHint("E — 보물상자 열기 (방 클리어 후)");
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInside = false;
                _label?.SetHighlight(false);
            }
        }
    }
}
