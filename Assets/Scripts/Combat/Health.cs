using System;
using UnityEngine;

namespace DungeonOdyssey.Combat
{
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHp = 100;
        [SerializeField] private int currentHp = 100;
        [SerializeField] private float invulnDuration = 0.25f;

        public int MaxHp => maxHp;
        public int CurrentHp => currentHp;
        public bool IsDead { get; private set; }
        public bool IsInvulnerable => Time.time < _invulnUntil;

        public event Action<int, int> OnHealthChanged;
        public event Action<int, Vector2> OnDamaged;
        public event Action OnDied;

        private float _invulnUntil;
        private CombatReactor _reactor;

        private void Awake()
        {
            _reactor = GetComponent<CombatReactor>();
        }

        public void Initialize(int max, int current)
        {
            maxHp = Mathf.Max(1, max);
            currentHp = Mathf.Clamp(current, 0, maxHp);
            IsDead = currentHp <= 0;
            OnHealthChanged?.Invoke(currentHp, maxHp);
        }

        public void SetMaxHp(int max, bool healToFull = false)
        {
            maxHp = Mathf.Max(1, max);
            currentHp = healToFull ? maxHp : Mathf.Min(currentHp, maxHp);
            OnHealthChanged?.Invoke(currentHp, maxHp);
        }

        public void HealFull()
        {
            // 상점 회복 등 — 사망(0 HP) 상태에서도 부활·풀피 가능
            IsDead = false;
            currentHp = maxHp;
            OnHealthChanged?.Invoke(currentHp, maxHp);
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || IsDead)
            {
                return;
            }

            currentHp = Mathf.Min(maxHp, currentHp + amount);
            OnHealthChanged?.Invoke(currentHp, maxHp);
        }

        /// <summary>강제 체력 설정 (세이브 로드·마을 귀환용).</summary>
        public void ForceSet(int max, int current)
        {
            maxHp = Mathf.Max(1, max);
            currentHp = Mathf.Clamp(current, 0, maxHp);
            IsDead = currentHp <= 0;
            OnHealthChanged?.Invoke(currentHp, maxHp);
        }

        public void GrantInvulnerability(float seconds)
        {
            _invulnUntil = Mathf.Max(_invulnUntil, Time.time + Mathf.Max(0f, seconds));
        }

        public void TakeDamage(int amount)
        {
            TakeDamage(amount, Vector2.zero, false);
        }

        public void TakeDamage(int amount, Vector2 hitDirection)
        {
            TakeDamage(amount, hitDirection, false);
        }

        public void TakeDamage(int amount, Vector2 hitDirection, bool critical)
        {
            if (IsDead || amount <= 0 || IsInvulnerable)
            {
                return;
            }

            currentHp = Mathf.Max(0, currentHp - amount);
            _invulnUntil = Time.time + invulnDuration;
            OnHealthChanged?.Invoke(currentHp, maxHp);
            OnDamaged?.Invoke(amount, hitDirection);

            var dying = currentHp <= 0;
            if (_reactor == null)
            {
                _reactor = GetComponent<CombatReactor>();
            }

            _reactor?.PlayHit(
                hitDirection.sqrMagnitude > 0.01f ? hitDirection : Vector2.right,
                amount, dying, critical);

            if (dying)
            {
                IsDead = true;
                OnDied?.Invoke();
            }
        }
    }
}
