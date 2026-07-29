using UnityEngine;

namespace DungeonOdyssey.Combat
{
    public interface IDamageable
    {
        bool IsDead { get; }
        void TakeDamage(int amount);
        void TakeDamage(int amount, Vector2 hitDirection);
        void TakeDamage(int amount, Vector2 hitDirection, bool critical);
    }
}
