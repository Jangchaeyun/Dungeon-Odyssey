using DungeonOdyssey.Core;
using DungeonOdyssey.UI;
using UnityEngine;

namespace DungeonOdyssey.Combat
{
    /// <summary>스타일 콤보 — 연속 처치로 등급 상승, 소량 보상·화려한 피드백.</summary>
    public static class StyleCombo
    {
        private static int _combo;
        private static float _expireAt;
        private const float Window = 4.2f;

        public static int Combo => _combo;
        public static string Rank => RankName(_combo);
        public static float TimeLeft => Mathf.Max(0f, _expireAt - Time.time);

        public static void Reset()
        {
            _combo = 0;
            _expireAt = 0f;
        }

        public static void NoteKill(bool boss = false)
        {
            if (Time.time > _expireAt && _combo > 0)
            {
                _combo = 0;
            }

            _combo += boss ? 3 : 1;
            _expireAt = Time.time + Window;
            SessionStats.NoteCombo(_combo);

            if (_combo == 3 || _combo == 5 || _combo == 8 || _combo == 12 || _combo % 10 == 0)
            {
                Object.FindFirstObjectByType<ClearBannerUI>()
                    ?.Show($"{Rank}  ·  COMBO {_combo}", ToastKind.Style, 1.4f);
                CombatAudio.UiClick();
            }

            Object.FindFirstObjectByType<HudUI>()?.SetStyleCombo(_combo, Rank);
        }

        public static void Tick()
        {
            if (_combo > 0 && Time.time > _expireAt)
            {
                _combo = 0;
                Object.FindFirstObjectByType<HudUI>()?.SetStyleCombo(0, "");
            }
        }

        /// <summary>콤보 등급에 따른 골드 보너스 배율 (1.0~1.35).</summary>
        public static float GoldMul => _combo switch
        {
            >= 12 => 1.35f,
            >= 8 => 1.25f,
            >= 5 => 1.15f,
            >= 3 => 1.08f,
            _ => 1f
        };

        private static string RankName(int c) => c switch
        {
            >= 15 => "LEGENDARY",
            >= 12 => "GODLIKE",
            >= 8 => "STYLISH",
            >= 5 => "GREAT",
            >= 3 => "NICE",
            >= 1 => "COMBO",
            _ => ""
        };
    }
}
