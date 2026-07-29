using UnityEngine;

namespace DungeonOdyssey.Core
{
    /// <summary>UI·월드 텍스트 공통 폰트. WebGL에서도 한글이 보이게 Resources 번들 우선.</summary>
    public static class GameFonts
    {
        private static Font _ui;

        public static Font Ui()
        {
            if (_ui != null)
            {
                return _ui;
            }

            _ui = Resources.Load<Font>("Fonts/UiKorean");
            if (_ui == null)
            {
                _ui = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                      ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return _ui;
        }
    }
}
