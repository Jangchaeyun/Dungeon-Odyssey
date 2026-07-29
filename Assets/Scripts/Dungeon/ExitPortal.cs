using DungeonOdyssey.Core;
using DungeonOdyssey.UI;
using UnityEngine;

namespace DungeonOdyssey.Dungeon
{
    public class ExitPortal : MonoBehaviour
    {
        private bool _playerInside;

        private void Update()
        {
            if (!_playerInside || !GameInput.InteractDown)
            {
                return;
            }

            if (DungeonManager.Instance != null && DungeonManager.Instance.IsCleared)
            {
                DungeonManager.Instance.TryReturnToTown();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            _playerInside = true;
            FindFirstObjectByType<HudUI>()?.SetHint("마을 복귀 [E]");
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _playerInside = false;
            }
        }
    }
}
