using DungeonOdyssey.Combat;
using DungeonOdyssey.Core;
using DungeonOdyssey.Player;
using DungeonOdyssey.UI;
using UnityEngine;

namespace DungeonOdyssey.Dungeon
{
    /// <summary>
    /// 이벤트 방 성소 — 리스크/리워드 선택.
    /// </summary>
    public class DungeonEventInteractable : MonoBehaviour
    {
        private bool _used;
        private bool _inside;
        private PlayerStats _player;
        private DialogueUI _dialogue;
        private WorldLabel _label;

        public void Configure(DialogueUI ui, WorldLabel label)
        {
            _dialogue = ui;
            _label = label;
        }

        private void Update()
        {
            if (!_inside || _used || GameUi.IsPaused || _player == null)
            {
                return;
            }

            if (GameInput.InteractDown)
            {
                Open();
            }

            if (!GameUi.IsBlocking)
            {
                return;
            }

            if (GameInput.MenuDown(1))
            {
                ChooseBless();
            }
            else if (GameInput.MenuDown(2))
            {
                ChooseGamble();
            }
            else if (GameInput.MenuDown(3))
            {
                ChooseRest();
            }
        }

        private void Open()
        {
            _dialogue ??= FindFirstObjectByType<DialogueUI>();
            _dialogue?.Show("잊힌 성소",
                "차가운 룬이 맥동한다. 무엇을 바치겠는가?\n\n" +
                "  1    축복 — ATK +2 · 치명 +4%\n" +
                "  2    도박 — 50% 대성공 / 50% 체력 손실\n" +
                "  3    안식 — 체력 풀회복 · 포션 +1\n\n" +
                "숫자 키로 선택 · Space 닫기");
        }

        private void ChooseBless()
        {
            if (_used || _player == null)
            {
                return;
            }

            _player.AddAttack(2);
            _player.AddCritBonus(0.04f);
            Finish("룬이 칼끝에 스며든다. ATK +2, 치명타 +4%");
        }

        private void ChooseGamble()
        {
            if (_used || _player == null)
            {
                return;
            }

            if (Random.value < 0.5f)
            {
                _player.AddAttack(4);
                GameManager.Instance?.AddGold(45);
                FloatingText.Gold(_player.transform.position + Vector3.up * 1.4f, 45);
                _player.NotifyMetaChanged();
                Finish("대성공! ATK +4, +45G");
            }
            else
            {
                var loss = Mathf.Max(8, _player.Health.MaxHp / 4);
                _player.Health.TakeDamage(loss, Vector2.zero);
                Finish($"저주가 스쳤다… HP -{loss}");
            }
        }

        private void ChooseRest()
        {
            if (_used || _player == null)
            {
                return;
            }

            _player.FullHeal();
            FloatingText.Heal(_player.transform.position + Vector3.up * 1.4f, _player.Health.MaxHp);
            _player.AddPotions(1);
            Finish("따뜻한 안식이 감싼다. 체력 회복, 포션 +1");
        }

        private void Finish(string msg)
        {
            _used = true;
            QuestCatalog.NotifyShrine();
            FindFirstObjectByType<HudUI>()?.RefreshQuest();
            CombatAudio.Heal();
            _dialogue ??= FindFirstObjectByType<DialogueUI>();
            _dialogue?.Show("잊힌 성소", msg + "\n\nSpace 로 닫기");
            if (_label != null)
            {
                _label.SetText("성소 · 소진됨");
            }

            FindFirstObjectByType<HudUI>()?.SetHint(msg);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") && other.GetComponentInParent<PlayerStats>() == null)
            {
                return;
            }

            _inside = true;
            _player = other.GetComponentInParent<PlayerStats>();
            if (!_used)
            {
                FindFirstObjectByType<HudUI>()?.SetHint("성소 [E] — 축복·도박·안식");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponentInParent<PlayerStats>() == null)
            {
                return;
            }

            _inside = false;
        }
    }
}
