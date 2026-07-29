using System.Collections;
using DungeonOdyssey.Core;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonOdyssey.UI
{
    public class GameOverUI : MonoBehaviour
    {
        private GameObject _panel;
        private Text _message;

        public void Bind(GameObject panel, Text message)
        {
            _panel = panel;
            _message = message;
            panel.SetActive(false);
        }

        public void Show(int goldLost)
        {
            GameUi.IsBlocking = true;
            var clears = GameManager.Instance != null ? GameManager.Instance.CurrentSave.dungeonClears : 0;
            var bless = GameManager.Instance != null ? GameManager.Instance.CurrentSave.metaBlessing : 0;
            if (_message != null)
            {
                _message.text = goldLost > 0
                    ? $"쓰러졌다...\n골드 -{goldLost}G\n클리어 {clears} · 축복 +{bless}\n마을로 귀환합니다"
                    : $"쓰러졌다...\n클리어 {clears} · 축복 +{bless}\n마을로 귀환합니다";
            }

            StartCoroutine(UiPanelMotion.FadeIn(_panel, 0.28f, 24f));
        }

        public void Hide()
        {
            if (_panel != null)
            {
                _panel.SetActive(false);
            }

            GameUi.IsBlocking = false;
        }
    }
}
