using DungeonOdyssey.Core;
using DungeonOdyssey.Dungeon;
using DungeonOdyssey.UI;
using UnityEngine;

namespace DungeonOdyssey.Town
{
    public class NpcInteractable : MonoBehaviour
    {
        [SerializeField] private string npcName = "마을 가이드";
        [SerializeField] private string dialogue = "안녕하세요, 모험가님!";
        [SerializeField] private DialogueUI dialogueUI;

        private bool _playerInside;
        private WorldLabel _label;
        private string _prompt = "E — 대화";

        public void Configure(string name, string text, DialogueUI ui, WorldLabel label = null,
            string prompt = null)
        {
            npcName = name;
            dialogue = text;
            dialogueUI = ui;
            _label = label;
            if (!string.IsNullOrEmpty(prompt))
            {
                _prompt = prompt;
            }
        }

        private void Update()
        {
            if (!_playerInside || GameUi.IsBlocking || GameUi.IsPaused)
            {
                return;
            }

            if (GameInput.InteractDown)
            {
                if (dialogueUI == null)
                {
                    dialogueUI = FindFirstObjectByType<DialogueUI>();
                }

                dialogueUI?.Show(npcName, dialogue);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            _playerInside = true;
            _label?.SetHighlight(true);
            FindFirstObjectByType<HudUI>()?.SetHint(_prompt);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            _playerInside = false;
            _label?.SetHighlight(false);
            dialogueUI?.Hide();
        }
    }
}
