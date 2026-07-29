using System.Collections;
using DungeonOdyssey.Combat;
using DungeonOdyssey.Core;
using DungeonOdyssey.Enemy;
using DungeonOdyssey.View3D;
using UnityEngine;

namespace DungeonOdyssey.Player
{
    public class PlayerCombat : MonoBehaviour
    {
        [SerializeField] private PlayerStats stats;
        [SerializeField] private float attackRange = 1.85f;
        [SerializeField] private float attackRadius = 1.05f;
        [SerializeField] private float lungeDistance = 0.55f;
        [SerializeField] private float comboWindow = 0.62f;

        private float _nextAttackTime;
        private float _comboExpire;
        private int _comboIndex;
        private int _swingId;
        private Rigidbody _rb;
        private CombatReactor _reactor;
        private HumanoidAnimator _anim;
        private bool _attacking;
        private bool _lunging;
        private bool _canComboCancel;
        private bool _bufferedAttack;
        private Vector2 _bufferedFacing;
        private float _attackMoveScale = 1f;

        public bool IsAttacking => _attacking;
        public bool IsLunging => _lunging;
        /// <summary>공격 중 이동 배율 (0~1).</summary>
        public float AttackMoveScale => _attackMoveScale;

        private void Awake()
        {
            if (stats == null)
            {
                stats = GetComponent<PlayerStats>();
            }

            _rb = GetComponent<Rigidbody>();
            _reactor = GetComponent<CombatReactor>();
            _anim = GetComponent<HumanoidAnimator>();
        }

        private HumanoidAnimator Anim
        {
            get
            {
                if (_anim == null)
                {
                    _anim = GetComponent<HumanoidAnimator>();
                }

                return _anim;
            }
        }

        public bool TryAttack(Vector2 facing)
        {
            if (GameUi.IsBlocking || GameUi.IsPaused)
            {
                return false;
            }

            if (stats == null || stats.Health.IsDead || (_reactor != null && _reactor.IsReacting))
            {
                return false;
            }

            // 공격 중이면 회복 구간에서만 버퍼 → 다음 콤보로 자연스럽게 이어짐
            if (_attacking)
            {
                if (_canComboCancel && Time.time >= _nextAttackTime)
                {
                    _bufferedAttack = true;
                    _bufferedFacing = facing;
                }

                return false;
            }

            if (Time.time < _nextAttackTime)
            {
                return false;
            }

            BeginSwing(facing);
            return true;
        }

        private void BeginSwing(Vector2 facing)
        {
            if (Time.time > _comboExpire)
            {
                _comboIndex = 0;
            }

            var aim = AimAssist(facing);
            var cooldown = _comboIndex >= 2 ? 0.42f : 0.18f;
            _nextAttackTime = Time.time + cooldown;
            _swingId++;
            var combo = _comboIndex;
            _comboIndex = (_comboIndex + 1) % 3;
            _comboExpire = Time.time + comboWindow;
            StartCoroutine(AttackRoutine(aim, combo, _swingId));
        }

        private Vector2 AimAssist(Vector2 facing)
        {
            MonsterBrain best = null;
            var bestScore = 0f;
            foreach (var m in Object.FindObjectsByType<MonsterBrain>(FindObjectsSortMode.None))
            {
                if (m == null || m.State == MonsterState.Dead)
                {
                    continue;
                }

                var delta = m.transform.position - transform.position;
                delta.y = 0f;
                var dist = delta.magnitude;
                if (dist > attackRange + 1.2f || dist < 0.1f)
                {
                    continue;
                }

                var dir = delta / dist;
                var face3 = new Vector3(facing.x, 0f, facing.y);
                if (face3.sqrMagnitude < 0.01f)
                {
                    face3 = transform.forward;
                }

                var dot = Vector3.Dot(face3.normalized, dir);
                var score = (1.5f - dist * 0.2f) + dot * 1.2f;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = m;
                }
            }

            if (best != null && bestScore > 0.45f)
            {
                var d = best.transform.position - transform.position;
                d.y = 0f;
                var n = d.normalized;
                return new Vector2(n.x, n.z);
            }

            return facing.sqrMagnitude > 0.01f ? facing.normalized : new Vector2(0f, 1f);
        }

        private IEnumerator AttackRoutine(Vector2 dir2, int combo, int swingId)
        {
            _attacking = true;
            _canComboCancel = false;
            _bufferedAttack = false;
            _attackMoveScale = 0f;

            var dir = new Vector3(dir2.x, 0f, dir2.y);
            if (dir.sqrMagnitude < 0.01f)
            {
                dir = transform.forward;
            }

            dir.y = 0f;
            dir.Normalize();

            // 부드럽게 조준
            var faceEnd = Time.time + 0.06f;
            var faceTarget = Quaternion.LookRotation(dir, Vector3.up);
            while (Time.time < faceEnd)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, faceTarget, 20f * Time.deltaTime);
                yield return null;
            }

            transform.rotation = faceTarget;
            Anim?.PlayAttack(combo);

            // 애니와 동일 윈드업 — 검이 뒤로 젖혀진 뒤 치도록
            var windup = HumanoidAnimator.WindupDuration(combo);
            var strike = HumanoidAnimator.StrikeDuration(combo);
            yield return new WaitForSeconds(windup);

            CombatAudio.Swing();
            _lunging = false;
            _attackMoveScale = 0f;

            var tip = Anim != null ? Anim.GetBladeTipTransform() : null;
            var slashOrigin = Anim != null
                ? Anim.GetBladeTipWorld()
                : transform.position + Vector3.up * 1.1f + dir * 0.7f;
            AttackFx.PlaySlash3D(slashOrigin, dir, combo, tip);
            Camera.main?.GetComponent<CameraRig3D>()?.Shake(0.028f + combo * 0.01f, 0.05f);
            Camera.main?.GetComponent<CameraRig3D>()?.Punch(0.04f + combo * 0.015f);

            // 스트라이크 구간: 제자리 판정 (런지 이동 없음)
            var hitSomething = false;
            var crit = false;
            var hitPoint = slashOrigin;
            var frames = combo == 2 ? 4 : 3;
            var frameDur = strike / frames;

            for (var f = 0; f < frames; f++)
            {
                yield return new WaitForSeconds(frameDur);

                if (ApplyDamage(dir, dir2, combo, swingId, out var wasCrit, out var tipHit))
                {
                    hitSomething = true;
                    crit |= wasCrit;
                    hitPoint = tipHit;
                }
            }

            _lunging = false;
            _attackMoveScale = 0f;

            if (hitSomething)
            {
                AttackFx.PlayImpact3D(hitPoint, crit);
                if (crit)
                {
                    CombatAudio.HitCrit();
                    HitStop.Pulse(0.05f, 0.045f);
                    Camera.main?.GetComponent<CameraRig3D>()?.Shake(0.11f, 0.07f);
                    Camera.main?.GetComponent<CameraRig3D>()?.Punch(0.12f);
                }
                else
                {
                    CombatAudio.HitFlesh();
                    HitStop.Pulse(combo == 2 ? 0.038f : 0.026f, 0.045f);
                    Camera.main?.GetComponent<CameraRig3D>()?.Shake(0.055f + combo * 0.012f, 0.055f);
                    Camera.main?.GetComponent<CameraRig3D>()?.Punch(0.075f);
                }
            }
            else
            {
                CombatAudio.Whiff();
            }

            // 회복 — 이 구간부터 다음 콤보 입력 가능 (이동은 공격 종료까지 불가)
            _canComboCancel = true;
            _attackMoveScale = 0f;
            var recover = HumanoidAnimator.RecoverDuration(combo);
            var recoverEnd = Time.time + recover;
            while (Time.time < recoverEnd)
            {
                if (_bufferedAttack)
                {
                    break;
                }

                yield return null;
            }

            _attacking = false;
            _canComboCancel = false;
            _lunging = false;
            _attackMoveScale = 1f;

            if (_bufferedAttack)
            {
                _bufferedAttack = false;
                BeginSwing(_bufferedFacing);
            }
        }

        private bool ApplyDamage(Vector3 dir, Vector2 dir2, int combo, int swingId, out bool critical)
        {
            return ApplyDamage(dir, dir2, combo, swingId, out critical, out _);
        }

        private bool ApplyDamage(Vector3 dir, Vector2 dir2, int combo, int swingId, out bool critical,
            out Vector3 hitPoint)
        {
            critical = false;
            hitPoint = transform.position + Vector3.up * 1.05f + dir * 0.95f;

            // 칼끝·칼자루 사이 궤적으로 판정 — 몸통 구보다 휘두름에 맞게
            Vector3 tip;
            Vector3 root;
            if (Anim != null)
            {
                tip = Anim.GetBladeTipWorld();
                root = Anim.GetBladeRootWorld();
            }
            else
            {
                tip = transform.position + Vector3.up * 1.15f + dir * (attackRange * 0.85f);
                root = transform.position + Vector3.up * 1f + dir * 0.4f;
            }

            hitPoint = tip;
            var mid = (tip + root) * 0.5f;
            var bladeLen = Vector3.Distance(tip, root);
            var radius = Mathf.Max(0.55f, attackRadius * 0.55f + combo * 0.05f + bladeLen * 0.15f);
            var origin = mid + dir * 0.12f;
            var hitSomething = false;
            var damage = stats.AttackPower + combo * 2;

            foreach (var col in Physics.OverlapSphere(origin, radius))
            {
                if (col.transform == transform || col.transform.IsChildOf(transform))
                {
                    continue;
                }

                if (col.GetComponentInParent<PlayerStats>() != null)
                {
                    continue;
                }

                var damageable = col.GetComponentInParent<IDamageable>();
                if (damageable == null || damageable.IsDead)
                {
                    continue;
                }

                if (damageable is not MonoBehaviour mb)
                {
                    continue;
                }

                var marker = mb.GetComponent<HitMarker>() ?? mb.gameObject.AddComponent<HitMarker>();
                if (!marker.TryMark(swingId))
                {
                    continue;
                }

                var critChance = 0.12f + (stats != null ? stats.CritChanceBonus : 0f);
                var isCrit = combo == 2 || Random.value < critChance;
                var dmg = isCrit ? Mathf.RoundToInt(damage * 1.45f) : damage;

                // 무기 원소·각인 보정
                if (stats != null)
                {
                    dmg = ApplyWeaponElement(mb, dmg, isCrit, dir2);
                }

                damageable.TakeDamage(dmg, dir2, isCrit);
                SessionStats.NoteDamageDealt(dmg, isCrit);
                hitSomething = true;
                critical |= isCrit;
                hitPoint = mb.transform.position + Vector3.up * 1.1f;
            }

            return hitSomething;
        }

        private int ApplyWeaponElement(MonoBehaviour target, int damage, bool isCrit, Vector2 dir2)
        {
            var def = stats.EquippedWeaponDef;
            var power = def.ElementPower * (0.85f + stats.WeaponUpgradeLevel * 0.04f);
            if (stats.HasWeaponSigil)
            {
                power *= 1.35f;
            }

            var brain = target.GetComponentInParent<MonsterBrain>();
            var pos = target.transform.position + Vector3.up * 1.2f;

            switch (def.Element)
            {
                case WeaponElement.Flame:
                {
                    var burn = Mathf.Max(1, Mathf.RoundToInt(damage * (0.12f + power * 0.25f)));
                    if (stats.HasWeaponSigil)
                    {
                        burn = Mathf.RoundToInt(burn * 1.35f);
                        var heal = Mathf.Max(1, burn / 4);
                        stats.Health.Heal(heal);
                    }

                    FloatingText.Spawn(pos + Vector3.right * 0.2f, $"화염+{burn}", FloatTextKind.Info);
                    return damage + burn;
                }
                case WeaponElement.Frost:
                    brain?.ApplySlow(stats.HasWeaponSigil ? 0.45f : 0.62f, stats.HasWeaponSigil ? 1.8f : 1.1f);
                    FloatingText.Spawn(pos, "동결", FloatTextKind.Info);
                    return damage;
                case WeaponElement.Storm:
                {
                    var splash = Mathf.Max(1, Mathf.RoundToInt(damage * (0.18f + power * 0.2f)));
                    if (stats.HasWeaponSigil)
                    {
                        splash = Mathf.RoundToInt(splash * 1.4f);
                    }

                    ChainSplash(target.transform.position, splash, dir2, target);
                    return damage;
                }
                case WeaponElement.Void:
                {
                    var rate = 0.04f + power * 0.08f + (stats.HasWeaponSigil ? 0.06f : 0f);
                    var heal = Mathf.Max(1, Mathf.RoundToInt(damage * rate));
                    stats.Health.Heal(heal);
                    if (stats.HasWeaponSigil && brain != null && brain.IsBoss)
                    {
                        damage = Mathf.RoundToInt(damage * 1.12f);
                    }

                    return damage;
                }
                case WeaponElement.Steel:
                    if (isCrit)
                    {
                        var extra = Mathf.Max(1, Mathf.RoundToInt(damage * (0.1f + power * 0.15f)));
                        if (stats.HasWeaponSigil)
                        {
                            extra = Mathf.RoundToInt(extra * 1.18f);
                        }

                        FloatingText.Spawn(pos, $"강철+{extra}", FloatTextKind.Crit);
                        return damage + extra;
                    }

                    return damage;
                case WeaponElement.Holy:
                {
                    var holy = Mathf.Max(1, Mathf.RoundToInt(damage * (0.08f + power * 0.18f)));
                    if (brain != null && brain.IsBoss)
                    {
                        holy = Mathf.RoundToInt(holy * (stats.HasWeaponSigil ? 1.5f : 1.25f));
                    }

                    if (stats.HasWeaponSigil || def.Id == WeaponId.HolyLongsword)
                    {
                        stats.Health.Heal(Mathf.Max(1, holy / 5));
                    }

                    FloatingText.Spawn(pos + Vector3.right * 0.15f, $"신성+{holy}", FloatTextKind.Info);
                    return damage + holy;
                }
                case WeaponElement.Nature:
                {
                    var poison = Mathf.Max(1, Mathf.RoundToInt(damage * (0.1f + power * 0.22f)));
                    if (stats.HasWeaponSigil)
                    {
                        poison = Mathf.RoundToInt(poison * 1.4f);
                    }

                    FloatingText.Spawn(pos, $"독+{poison}", FloatTextKind.Info);
                    return damage + poison;
                }
                case WeaponElement.Blood:
                {
                    var rate = 0.05f + power * 0.1f + (stats.HasWeaponSigil ? 0.07f : 0f);
                    var heal = Mathf.Max(1, Mathf.RoundToInt(damage * rate));
                    stats.Health.Heal(heal);
                    if (stats.HasWeaponSigil && stats.Health.CurrentHp < stats.Health.MaxHp * 0.4f)
                    {
                        damage = Mathf.RoundToInt(damage * 1.1f);
                    }

                    return damage;
                }
                case WeaponElement.Earth:
                {
                    var crush = Mathf.Max(1, Mathf.RoundToInt(damage * (0.1f + power * 0.2f)));
                    if (stats.HasWeaponSigil)
                    {
                        crush = Mathf.RoundToInt(crush * 1.2f);
                    }

                    if (def.Id == WeaponId.TitanHammer)
                    {
                        ChainSplash(target.transform.position, Mathf.Max(1, crush / 2), dir2, target);
                    }

                    FloatingText.Spawn(pos, $"파쇄+{crush}", FloatTextKind.Info);
                    return damage + crush;
                }
                default:
                    return damage;
            }
        }

        private void ChainSplash(Vector3 origin, int splashDamage, Vector2 dir2, MonoBehaviour exclude)
        {
            foreach (var col in Physics.OverlapSphere(origin, 2.4f))
            {
                var mb = col.GetComponentInParent<MonsterBrain>();
                if (mb == null || mb == exclude || mb.State == MonsterState.Dead)
                {
                    continue;
                }

                var dmg = mb.GetComponent<IDamageable>();
                if (dmg == null || dmg.IsDead)
                {
                    continue;
                }

                dmg.TakeDamage(splashDamage, dir2, false);
                FloatingText.Spawn(mb.transform.position + Vector3.up * 1.1f, $"번개{splashDamage}",
                    FloatTextKind.Info);
                SessionStats.NoteDamageDealt(splashDamage, false);
            }
        }
    }

    public class HitMarker : MonoBehaviour
    {
        private int _lastKey = int.MinValue;

        public bool TryMark(int key)
        {
            if (_lastKey == key)
            {
                return false;
            }

            _lastKey = key;
            return true;
        }
    }
}
