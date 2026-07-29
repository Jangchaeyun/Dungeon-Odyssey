using System.Globalization;
using UnityEngine;

namespace DungeonOdyssey.UI
{
    /// <summary>골드 HUD 표기 — 천 단위 구분, 박스 초과 시 K/M 요약.</summary>
    public static class GoldFormat
    {
        /// <param name="maxChars">HUD 칩에 들어갈 최대 글자 수 ("11,507G" = 7).</param>
        public static string ForHud(int gold, int maxChars = 7)
        {
            gold = Mathf.Max(0, gold);
            var full = WithCommas(gold);
            if (full.Length <= maxChars)
            {
                return full;
            }

            return Compact(gold);
        }

        public static string WithCommas(int gold)
        {
            gold = Mathf.Max(0, gold);
            return gold.ToString("#,##0", CultureInfo.InvariantCulture) + "G";
        }

        public static string Compact(int gold)
        {
            gold = Mathf.Max(0, gold);
            if (gold >= 1_000_000_000)
            {
                return TrimDecimal(gold / 1_000_000_000d) + "B";
            }

            if (gold >= 1_000_000)
            {
                return TrimDecimal(gold / 1_000_000d) + "M";
            }

            if (gold >= 1_000)
            {
                var k = gold / 1_000d;
                // 999,500+ 는 반올림 시 1000K → 1M
                if (k >= 999.95d)
                {
                    return TrimDecimal(gold / 1_000_000d) + "M";
                }

                return TrimDecimal(k) + "K";
            }

            return gold + "G";
        }

        private static string TrimDecimal(double value)
        {
            // 1 → "1", 1.5 → "1.5", 10.25 → "10.3"
            if (value >= 100d)
            {
                return Mathf.RoundToInt((float)value).ToString(CultureInfo.InvariantCulture);
            }

            var rounded = System.Math.Round(value, value >= 10d ? 1 : 2);
            if (System.Math.Abs(rounded - System.Math.Round(rounded)) < 0.001d)
            {
                return ((long)System.Math.Round(rounded)).ToString(CultureInfo.InvariantCulture);
            }

            return rounded.ToString(value >= 10d ? "0.#" : "0.##", CultureInfo.InvariantCulture);
        }
    }
}
