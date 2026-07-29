using UnityEngine;

namespace DungeonOdyssey.Combat
{
    /// <summary>호환용 — 내부적으로 FloatingText를 사용합니다.</summary>
    public class DamagePopup : MonoBehaviour
    {
        public static void Spawn(Vector3 position, int amount, bool critical = false) =>
            FloatingText.Damage(position, amount, critical);
    }
}
