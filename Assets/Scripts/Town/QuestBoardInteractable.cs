using DungeonOdyssey.Combat;
using DungeonOdyssey.Core;
using DungeonOdyssey.Dungeon;
using DungeonOdyssey.UI;
using UnityEngine;

namespace DungeonOdyssey.Town
{
    /// <summary>가이드 Mira — 퀘스트/일일 의뢰판 (전용 메뉴 UI).</summary>
    public class QuestBoardInteractable : MonoBehaviour
    {
        [SerializeField] private DialogueUI dialogueUI;
        private ForgeShopUI _board;
        private bool _playerInside;
        private WorldLabel _label;
        private string _prompt = "E — Mira 의뢰판";

        public void Configure(DialogueUI ui, WorldLabel label = null, string prompt = null)
        {
            dialogueUI = ui;
            _label = label;
            if (!string.IsNullOrEmpty(prompt))
            {
                _prompt = prompt;
            }
        }

        private void Update()
        {
            if (!_playerInside || GameUi.IsPaused)
            {
                return;
            }

            if (_board != null && _board.IsOpen)
            {
                return;
            }

            if (GameUi.IsBlocking)
            {
                return;
            }

            if (GameInput.InteractDown)
            {
                OpenBoard();
            }
        }

        private void OpenBoard()
        {
            EnsureBoard();
            var save = GameManager.Instance?.CurrentSave;
            if (save == null)
            {
                return;
            }

            QuestCatalog.EnsureDaily(save);
            FindFirstObjectByType<HudUI>()?.SetQuestLine(QuestCatalog.HudLine(save));

            if (_board == null)
            {
                dialogueUI ??= FindFirstObjectByType<DialogueUI>();
                CombatAudio.ShopOpen();
                dialogueUI?.Show("가이드 · Mira", QuestCatalog.BoardText(save));
                return;
            }

            RefreshBoard();
        }

        private void RefreshBoard(string notice = null)
        {
            EnsureBoard();
            var save = GameManager.Instance?.CurrentSave;
            if (save == null || _board == null)
            {
                return;
            }

            QuestCatalog.BuildBoardUi(save, out var status, out var actions, notice);
            _board.Show("가이드 · Mira", status, actions, OnBoardSelect);
            FindFirstObjectByType<HudUI>()?.SetQuestLine(QuestCatalog.HudLine(save));
        }

        private void OnBoardSelect(int index)
        {
            switch (index)
            {
                case 1:
                    Accept();
                    break;
                case 2:
                    ClaimDaily();
                    break;
                case 3:
                    Advice();
                    break;
            }
        }

        private void Accept()
        {
            var save = GameManager.Instance?.CurrentSave;
            if (save == null)
            {
                return;
            }

            if (QuestCatalog.TryAcceptOffer(save, out var msg))
            {
                GameManager.Instance.Persist(SceneNames.Town);
                Notify(msg, ToastKind.Success);
                RefreshBoard();
            }
            else
            {
                CombatAudio.Locked();
                Notify(msg, ToastKind.Danger);
                RefreshBoard();
            }
        }

        private void ClaimDaily()
        {
            var save = GameManager.Instance?.CurrentSave;
            if (save == null)
            {
                return;
            }

            if (QuestCatalog.TryClaimDaily(save, out var msg))
            {
                CombatAudio.Coin();
                GameManager.Instance.Persist(SceneNames.Town);
                Notify(msg, ToastKind.Success);
                RefreshBoard();
            }
            else
            {
                CombatAudio.Locked();
                Notify(msg, ToastKind.Danger);
                RefreshBoard();
            }
        }

        private void Advice()
        {
            var save = GameManager.Instance?.CurrentSave;
            var tip = QuestCatalog.Advice(save);
            var first = tip;
            var nl = tip.IndexOf('\n');
            if (nl > 0)
            {
                first = tip.Substring(0, nl).Trim();
            }

            FindFirstObjectByType<ClearBannerUI>()?.Show(first, ToastKind.Info, 2.2f);
            RefreshBoard(tip);
        }

        private static void Notify(string message, ToastKind kind)
        {
            var oneLine = message;
            var nl = message.IndexOf('\n');
            if (nl > 0)
            {
                oneLine = message.Substring(0, nl).Trim();
            }

            FindFirstObjectByType<ClearBannerUI>()?.Show(oneLine, kind, 1.8f);
        }

        private void EnsureBoard()
        {
            if (_board == null)
            {
                _board = FindFirstObjectByType<ForgeShopUI>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            _playerInside = true;
            _label?.SetVisible(true);
            _label?.SetHighlight(true);
            var save = GameManager.Instance?.CurrentSave;
            FindFirstObjectByType<HudUI>()?.SetHint(_prompt);
            if (save != null)
            {
                FindFirstObjectByType<HudUI>()?.SetQuestLine(QuestCatalog.HudLine(save));
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            _playerInside = false;
            _label?.SetHighlight(false);
            _label?.SetVisible(false);
            if (_board != null && _board.IsOpen)
            {
                _board.Hide();
            }
            else if (dialogueUI != null && dialogueUI.IsVisible)
            {
                dialogueUI.Hide();
            }
        }
    }
}
