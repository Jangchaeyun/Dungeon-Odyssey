using System.Collections;
using System.Collections.Generic;
using DungeonOdyssey.Combat;
using DungeonOdyssey.Core;
using DungeonOdyssey.Enemy;
using DungeonOdyssey.View3D;
using UnityEngine;

namespace DungeonOdyssey.Player
{
    /// <summary>
    /// R: 장착 무기의 고유 스킬. 무기마다 모션·수치·부가효과가 다르다.
    /// </summary>
    public class PlayerSkill : MonoBehaviour
    {
        private readonly float[] _readyTimes = new float[WeaponCatalog.WeaponCount];
        private Rigidbody _rb;
        private PlayerStats _stats;
        private PlayerController _controller;
        private CombatReactor _reactor;
        private HumanoidAnimator _anim;
        private bool _busy;
        private WeaponId _weapon = WeaponId.IronSword;

        public float CooldownRemaining => ReadyLeft(_weapon);
        public float CooldownMax => SkillCatalog.Spec(_weapon).Cooldown;
        public bool IsReady => ReadyLeft(_weapon) <= 0f && !_busy;
        public SkillId Equipped => SkillCatalog.ForWeapon(_weapon);
        public string EquippedLabel => SkillCatalog.WeaponSkillName(_weapon);
        public int OwnedSkillCount => SkillCatalog.SkillCount;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _stats = GetComponent<PlayerStats>();
            _controller = GetComponent<PlayerController>();
            _reactor = GetComponent<CombatReactor>();
            _anim = GetComponent<HumanoidAnimator>();
        }

        /// <summary>스킬 VFX·히트 원점 — 칼끝. 너무 몸 안쪽/바닥이면 전방으로 보정.</summary>
        private Vector3 BladeTip(Vector3 dir, float along = 0f)
        {
            if (dir.sqrMagnitude < 0.01f)
            {
                dir = transform.forward;
            }

            dir = dir.normalized;
            var safe = transform.position + Vector3.up * 1.15f + dir * (0.75f + along);

            if (_anim == null)
            {
                _anim = GetComponent<HumanoidAnimator>();
            }

            if (_anim == null)
            {
                return safe;
            }

            var tip = _anim.GetBladeTipWorld() + dir * along;
            var fromBody = tip - transform.position;
            fromBody.y = 0f;
            // 칼끝이 몸 안·바닥에 있으면 카메라에 안 보이므로 전방 보정
            if (fromBody.sqrMagnitude < 0.15f || tip.y < transform.position.y + 0.35f)
            {
                return safe;
            }

            tip.y = Mathf.Max(tip.y, transform.position.y + 0.85f);
            return tip;
        }

        private void Start()
        {
            RefreshFromStats();
        }

        public void RefreshFromStats()
        {
            if (_stats == null)
            {
                return;
            }

            // 가방에서 고른 스킬 출처 무기 (없으면 장착 무기)
            _weapon = _stats.SkillSourceWeapon;
        }

        private void Update()
        {
            if (GameUi.IsBlocking || GameUi.IsPaused)
            {
                return;
            }

            RefreshFromStats();

            if (GameInput.SkillCycleDown)
            {
                var follow = _stats != null && _stats.SkillFollowsWeapon;
                FindFirstObjectByType<UI.HudUI>()?.SetHint(follow
                    ? $"스킬  ·  {EquippedLabel}  (장착 무기 따라감 · 가방에서 교체 가능)"
                    : $"스킬  ·  {EquippedLabel}  (가방에서 장착한 스킬)");
            }

            if (GameInput.SkillDown)
            {
                TryUseSkill();
            }
        }

        public bool TryUseSkill()
        {
            if (_busy || _stats == null || _stats.Health.IsDead
                || (_reactor != null && _reactor.IsReacting))
            {
                return false;
            }

            RefreshFromStats();

            if (ReadyLeft(_weapon) > 0f)
            {
                FindFirstObjectByType<UI.HudUI>()?.SetHint(
                    $"{EquippedLabel} 쿨다운 {ReadyLeft(_weapon):0.0}초");
                return false;
            }

            StartCoroutine(UseRoutine(_weapon));
            return true;
        }

        private float ReadyLeft(WeaponId weapon)
        {
            var i = (int)weapon;
            if (i < 0 || i >= _readyTimes.Length)
            {
                return 0f;
            }

            return Mathf.Max(0f, _readyTimes[i] - Time.time);
        }

        private IEnumerator UseRoutine(WeaponId weapon)
        {
            _busy = true;
            var spec = SkillCatalog.Spec(weapon);
            _readyTimes[(int)weapon] = Time.time + spec.Cooldown;
            var label = spec.Name;
            FloatingText.Spawn(transform.position + Vector3.up * 1.85f, label, FloatTextKind.Info);
            FindFirstObjectByType<UI.HudUI>()?.SetHint($"스킬  ·  {label}");

            switch (spec.Motion)
            {
                case SkillMotion.LinePierce:
                    yield return LinePierce(spec, label);
                    break;
                case SkillMotion.TripleStab:
                    yield return TripleStab(spec, label);
                    break;
                case SkillMotion.BlinkCut:
                    yield return BlinkCut(spec, label);
                    break;
                case SkillMotion.ChaosRush:
                    yield return ChaosRush(spec, label);
                    break;
                case SkillMotion.Spin:
                    yield return Spin(spec, label, false);
                    break;
                case SkillMotion.SolarSpin:
                    yield return Spin(spec, label, true);
                    break;
                case SkillMotion.Crescent:
                    yield return Crescent(spec, label);
                    break;
                case SkillMotion.StormBolt:
                    yield return StormBolt(spec, label);
                    break;
                case SkillMotion.FrostAura:
                    yield return FrostAura(spec, label);
                    break;
                case SkillMotion.HolyLight:
                    yield return HolyLight(spec, label);
                    break;
                case SkillMotion.VoidBurst:
                    yield return VoidBurst(spec, label);
                    break;
                case SkillMotion.Ward:
                    yield return HolyLight(spec, label);
                    break;
                case SkillMotion.Slam:
                    yield return Slam(spec, label, false);
                    break;
                case SkillMotion.BloodSlam:
                    yield return Slam(spec, label, true);
                    break;
                case SkillMotion.Quake:
                    yield return Quake(spec, label);
                    break;
                case SkillMotion.ConeCleave:
                    yield return ConeCleave(spec, label);
                    break;
                default:
                    yield return DashCut(spec, label);
                    break;
            }

            _busy = false;
        }

        private static SkillFxStyle StyleOf(SkillMotion motion) => motion switch
        {
            SkillMotion.LinePierce => SkillFxStyle.Beam,
            SkillMotion.TripleStab => SkillFxStyle.Stab,
            SkillMotion.BlinkCut => SkillFxStyle.Blink,
            SkillMotion.ChaosRush => SkillFxStyle.Chaos,
            SkillMotion.Spin or SkillMotion.SolarSpin => SkillFxStyle.Spin,
            SkillMotion.Crescent => SkillFxStyle.Crescent,
            SkillMotion.StormBolt => SkillFxStyle.Bolt,
            SkillMotion.FrostAura => SkillFxStyle.Frost,
            SkillMotion.HolyLight or SkillMotion.Ward => SkillFxStyle.Holy,
            SkillMotion.VoidBurst => SkillFxStyle.Void,
            SkillMotion.Slam => SkillFxStyle.Slam,
            SkillMotion.BloodSlam => SkillFxStyle.Blood,
            SkillMotion.Quake => SkillFxStyle.Quake,
            SkillMotion.ConeCleave => SkillFxStyle.Cone,
            _ => SkillFxStyle.DashSlash
        };

        private void PlaySkillFx(WeaponSkillSpec spec, Vector3 dir, float scale = 1f, float along = 0f)
        {
            var origin = BladeTip(dir, along);
            var color = SkillCatalog.FxColor(spec.Weapon);
            AttackFx.PlaySkill(StyleOf(spec.Motion), color, origin, dir, scale * 1.25f);
            // 머티리얼/스타일 실패해도 최소 플래시는 보이게
            AttackFx.PlayColoredImpact(origin, color, true);
        }

        private IEnumerator DashCut(WeaponSkillSpec spec, string label)
        {
            var dir = AimDir();
            CombatAudio.Swing();
            PlaySkillFx(spec, dir, 1f);
            CharacterPhysics.MoveWithCollision(_rb, dir * spec.Dash);
            Camera.main?.GetComponent<CameraRig3D>()?.Shake(0.16f + spec.Dash * 0.02f, 0.12f);
            Camera.main?.GetComponent<CameraRig3D>()?.Punch(0.28f);

            var origin = BladeTip(dir, 0.15f);
            var hits = DealInSphere(origin, spec.Radius, Damage(spec), dir, true, spec);
            if (hits > 0)
            {
                AttackFx.PlayColoredImpact(origin, SkillCatalog.FxColor(spec.Weapon), true);
            }

            FindFirstObjectByType<UI.HudUI>()?.SetHint(hits > 0 ? $"{label}!" : $"{label}");
            yield return new WaitForSeconds(0.18f);
        }

        private IEnumerator LinePierce(WeaponSkillSpec spec, string label)
        {
            var dir = AimDir();
            CombatAudio.Swing();
            CharacterPhysics.MoveWithCollision(_rb, dir * (spec.Dash * 0.55f));
            Camera.main?.GetComponent<CameraRig3D>()?.Shake(0.2f, 0.12f);
            Camera.main?.GetComponent<CameraRig3D>()?.Punch(0.32f);
            PlaySkillFx(spec, dir, 0.85f + spec.Dash * 0.15f);

            var steps = Mathf.Max(2, spec.Hits);
            for (var i = 0; i < steps; i++)
            {
                var t = (i + 1f) / steps;
                var origin = BladeTip(dir, spec.Dash * t * 0.35f);
                DealInSphere(origin, spec.Radius, Damage(spec) - (steps - 1 - i), dir, i == steps - 1, spec);
                yield return new WaitForSeconds(0.05f);
            }

            FindFirstObjectByType<UI.HudUI>()?.SetHint($"{label}!");
            yield return new WaitForSeconds(0.1f);
        }

        private IEnumerator TripleStab(WeaponSkillSpec spec, string label)
        {
            var dir = AimDir();
            CharacterPhysics.MoveWithCollision(_rb, dir * spec.Dash);
            Camera.main?.GetComponent<CameraRig3D>()?.Punch(0.22f);

            var hits = Mathf.Max(2, spec.Hits);
            for (var i = 0; i < hits; i++)
            {
                CombatAudio.Swing();
                var origin = BladeTip(dir, 0.1f + i * 0.12f);
                PlaySkillFx(spec, dir, 0.85f, 0.1f + i * 0.12f);
                DealInSphere(origin, spec.Radius, Damage(spec), dir, i == hits - 1, spec);
                yield return new WaitForSeconds(0.07f);
            }

            FindFirstObjectByType<UI.HudUI>()?.SetHint($"{label}!");
            yield return new WaitForSeconds(0.08f);
        }

        private IEnumerator BlinkCut(WeaponSkillSpec spec, string label)
        {
            var dir = AimDir();
            CombatAudio.Swing();
            PlaySkillFx(spec, dir, 1f);
            CharacterPhysics.MoveWithCollision(_rb, dir * spec.Dash);
            Camera.main?.GetComponent<CameraRig3D>()?.Shake(0.2f, 0.11f);
            DealInSphere(BladeTip(dir, 0.2f), spec.Radius, Damage(spec), dir, true, spec);
            yield return new WaitForSeconds(0.08f);

            var back = -dir;
            CharacterPhysics.MoveWithCollision(_rb, back * (spec.Dash * 0.55f));
            PlaySkillFx(spec, back, 0.85f);
            DealInSphere(BladeTip(back, 0.15f), spec.Radius * 0.9f, Damage(spec) - 2, back, false, spec);
            FindFirstObjectByType<UI.HudUI>()?.SetHint($"{label}!");
            yield return new WaitForSeconds(0.12f);
        }

        private IEnumerator ChaosRush(WeaponSkillSpec spec, string label)
        {
            var baseDir = AimDir();
            Camera.main?.GetComponent<CameraRig3D>()?.Shake(0.3f, 0.18f);
            Camera.main?.GetComponent<CameraRig3D>()?.Punch(0.42f);
            PlaySkillFx(spec, baseDir, 1.15f);
            var yaws = new[] { -62f, 62f, 0f };
            for (var i = 0; i < yaws.Length; i++)
            {
                var dir = Quaternion.Euler(0f, yaws[i], 0f) * baseDir;
                CombatAudio.Swing();
                CharacterPhysics.MoveWithCollision(_rb, dir * (spec.Dash * (i == 2 ? 1.15f : 1f)));
                DealInSphere(BladeTip(dir, 0.2f), spec.Radius, Damage(spec) + i, dir, true, spec);
                yield return new WaitForSeconds(0.12f);
            }

            FindFirstObjectByType<UI.HudUI>()?.SetHint($"{label}!");
            yield return new WaitForSeconds(0.08f);
        }

        private IEnumerator Spin(WeaponSkillSpec spec, string label, bool healAtEnd)
        {
            CombatAudio.Swing();
            Camera.main?.GetComponent<CameraRig3D>()?.Shake(0.22f, 0.14f);
            Camera.main?.GetComponent<CameraRig3D>()?.Punch(0.35f);
            PlaySkillFx(spec, AimDir(), 0.9f + spec.Radius * 0.15f);

            var hits = Mathf.Max(2, spec.Hits);
            for (var i = 0; i < hits; i++)
            {
                var yaw = i * (360f / hits);
                var dir = Quaternion.Euler(0f, yaw, 0f) * transform.forward;
                DealInSphere(BladeTip(dir), spec.Radius, Damage(spec), dir, i == hits - 1, spec);
                yield return new WaitForSeconds(0.07f);
            }

            if (healAtEnd && spec.HealPct > 0f)
            {
                ApplyHeal(spec.HealPct);
                AttackFx.PlayColoredImpact(BladeTip(AimDir()), SkillCatalog.FxColor(spec.Weapon), false);
            }

            FindFirstObjectByType<UI.HudUI>()?.SetHint($"{label}!");
            yield return new WaitForSeconds(0.1f);
        }

        private IEnumerator Crescent(WeaponSkillSpec spec, string label)
        {
            var aim = AimDir();
            CombatAudio.Swing();
            Camera.main?.GetComponent<CameraRig3D>()?.Shake(0.2f, 0.13f);
            Camera.main?.GetComponent<CameraRig3D>()?.Punch(0.33f);
            PlaySkillFx(spec, aim, 1.1f);

            var waves = Mathf.Max(1, spec.Hits);
            for (var i = 0; i < waves; i++)
            {
                DealInCone(BladeTip(aim), aim, 110f, spec.Radius,
                    Damage(spec), aim, i == waves - 1, spec);
                yield return new WaitForSeconds(0.09f);
            }

            FindFirstObjectByType<UI.HudUI>()?.SetHint($"{label}!");
            yield return new WaitForSeconds(0.1f);
        }

        private IEnumerator StormBolt(WeaponSkillSpec spec, string label)
        {
            var dir = AimDir();
            CombatAudio.Swing();
            Camera.main?.GetComponent<CameraRig3D>()?.Shake(0.22f, 0.13f);
            PlaySkillFx(spec, dir, 1f);
            DealInSphere(BladeTip(dir), spec.Radius, Damage(spec) - 2, dir, false, spec);
            yield return new WaitForSeconds(0.08f);

            dir = AimDir();
            CharacterPhysics.MoveWithCollision(_rb, dir * spec.Dash);
            AttackFx.PlaySkill(SkillFxStyle.Beam, SkillCatalog.FxColor(spec.Weapon),
                BladeTip(dir), dir, 1f);
            DealInSphere(BladeTip(dir, 0.35f), spec.Radius * 0.75f, Damage(spec) + 2, dir, true, spec);
            FindFirstObjectByType<UI.HudUI>()?.SetHint($"{label}!");
            yield return new WaitForSeconds(0.12f);
        }

        private IEnumerator FrostAura(WeaponSkillSpec spec, string label)
        {
            CombatAudio.Heal();
            if (spec.Invuln > 0f)
            {
                _stats.Health.GrantInvulnerability(spec.Invuln);
            }

            if (spec.HealPct > 0f)
            {
                ApplyHeal(spec.HealPct);
            }

            // 장막류는 칼끝 높이에서 펼침
            PlaySkillFx(spec, AimDir(), 0.85f + spec.Radius * 0.2f);
            ApplySlowAura(spec.Radius, spec.SlowMul, spec.SlowDur);

            if (spec.Hits > 0 && spec.BonusDamage > 0)
            {
                yield return new WaitForSeconds(0.08f);
                DealInSphere(BladeTip(AimDir()), spec.Radius, Damage(spec), AimDir(), true, spec);
            }

            Camera.main?.GetComponent<CameraRig3D>()?.Punch(0.18f);
            FindFirstObjectByType<UI.HudUI>()?.SetHint($"{label}! 한기 장막");
            yield return new WaitForSeconds(0.22f);
        }

        private IEnumerator HolyLight(WeaponSkillSpec spec, string label)
        {
            CombatAudio.Heal();
            if (spec.Invuln > 0f)
            {
                _stats.Health.GrantInvulnerability(spec.Invuln);
            }

            ApplyHeal(spec.HealPct > 0f ? spec.HealPct : 0.18f);
            PlaySkillFx(spec, AimDir(), 1.1f);
            Camera.main?.GetComponent<CameraRig3D>()?.Punch(0.15f);
            FindFirstObjectByType<UI.HudUI>()?.SetHint($"{label}! 잠시 무적");
            yield return new WaitForSeconds(0.25f);
        }

        private IEnumerator VoidBurst(WeaponSkillSpec spec, string label)
        {
            CombatAudio.Heal();
            if (spec.Invuln > 0f)
            {
                _stats.Health.GrantInvulnerability(spec.Invuln);
            }

            PlaySkillFx(spec, AimDir(), 0.9f + spec.Radius * 0.15f);
            Camera.main?.GetComponent<CameraRig3D>()?.Shake(0.2f, 0.12f);
            var hits = DealInSphere(BladeTip(AimDir()), spec.Radius, Damage(spec), AimDir(), true, spec);
            if (hits > 0 || spec.HealPct > 0f)
            {
                var bonus = hits * Mathf.Max(1, Mathf.RoundToInt(_stats.AttackPower * 0.04f));
                ApplyHeal(spec.HealPct);
                if (bonus > 0)
                {
                    _stats.Health.Heal(bonus);
                    FloatingText.Heal(transform.position + Vector3.up * 1.5f, bonus);
                }
            }

            FindFirstObjectByType<UI.HudUI>()?.SetHint($"{label}!");
            yield return new WaitForSeconds(0.22f);
        }

        private IEnumerator Slam(WeaponSkillSpec spec, string label, bool lifeSteal)
        {
            CombatAudio.Swing();
            Camera.main?.GetComponent<CameraRig3D>()?.Shake(0.26f, 0.15f);
            Camera.main?.GetComponent<CameraRig3D>()?.Punch(0.4f);

            var waves = Mathf.Max(1, spec.Hits);
            var totalHits = 0;
            for (var i = 0; i < waves; i++)
            {
                var r = spec.Radius * (i == 0 ? 1f : 0.85f);
                var bonus = i == 0 ? spec.BonusDamage : Mathf.Max(2, spec.BonusDamage / 2);
                var tip = BladeTip(AimDir());
                // 찍기 연출은 칼끝 → 지면
                var ground = new Vector3(tip.x, transform.position.y + 0.15f, tip.z);
                PlaySkillFx(spec, AimDir(), 0.85f + r * 0.2f);
                totalHits += DealInSphere(ground, r, _stats.AttackPower + bonus, AimDir(), i == 0, spec);
                yield return new WaitForSeconds(0.09f);
            }

            if (lifeSteal && totalHits > 0)
            {
                var heal = Mathf.Max(1, Mathf.RoundToInt(
                    _stats.Health.MaxHp * Mathf.Max(0.03f, spec.HealPct) * totalHits));
                _stats.Health.Heal(heal);
                FloatingText.Heal(transform.position + Vector3.up * 1.45f, heal);
            }

            FindFirstObjectByType<UI.HudUI>()?.SetHint($"{label}!");
            yield return new WaitForSeconds(0.12f);
        }

        private IEnumerator Quake(WeaponSkillSpec spec, string label)
        {
            CombatAudio.Swing();
            Camera.main?.GetComponent<CameraRig3D>()?.Shake(0.3f, 0.16f);
            Camera.main?.GetComponent<CameraRig3D>()?.Punch(0.42f);

            var waves = Mathf.Max(2, spec.Hits);
            for (var i = 0; i < waves; i++)
            {
                var r = spec.Radius + i * 0.55f;
                var tip = BladeTip(AimDir());
                var ground = new Vector3(tip.x, transform.position.y + 0.12f, tip.z);
                PlaySkillFx(spec, AimDir(), 0.8f + i * 0.25f);
                DealInSphere(ground, r, Damage(spec) - i, AimDir(), i == waves - 1, spec);
                yield return new WaitForSeconds(0.08f);
            }

            FindFirstObjectByType<UI.HudUI>()?.SetHint($"{label}!");
            yield return new WaitForSeconds(0.12f);
        }

        private IEnumerator ConeCleave(WeaponSkillSpec spec, string label)
        {
            var dir = AimDir();
            CombatAudio.Swing();
            Camera.main?.GetComponent<CameraRig3D>()?.Shake(0.24f, 0.14f);
            Camera.main?.GetComponent<CameraRig3D>()?.Punch(0.38f);
            PlaySkillFx(spec, dir, 1.15f);
            DealInCone(BladeTip(dir), dir, 95f, spec.Radius, Damage(spec), dir, true, spec);
            FindFirstObjectByType<UI.HudUI>()?.SetHint($"{label}!");
            yield return new WaitForSeconds(0.2f);
        }

        private int Damage(WeaponSkillSpec spec) =>
            _stats.AttackPower + spec.BonusDamage;

        private void ApplyHeal(float pct)
        {
            if (pct <= 0f)
            {
                return;
            }

            var heal = Mathf.Max(1, Mathf.RoundToInt(_stats.Health.MaxHp * pct));
            _stats.Health.Heal(heal);
            FloatingText.Heal(transform.position + Vector3.up * 1.4f, heal);
        }

        private void ApplySlowAura(float radius, float mul, float dur)
        {
            if (dur <= 0f || mul >= 0.99f)
            {
                return;
            }

            foreach (var col in Physics.OverlapSphere(transform.position + Vector3.up * 0.8f, radius))
            {
                if (col.transform == transform || col.transform.IsChildOf(transform))
                {
                    continue;
                }

                col.GetComponentInParent<MonsterBrain>()?.ApplySlow(mul, dur);
            }
        }

        private Vector3 AimDir()
        {
            var dir = _controller != null ? _controller.Facing3D : transform.forward;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f)
            {
                dir = transform.forward;
            }

            return dir.normalized;
        }

        private int DealInSphere(Vector3 origin, float radius, int dmg, Vector3 dir, bool bigFx,
            WeaponSkillSpec spec)
        {
            return DealFiltered(origin, radius, dmg, dir, bigFx, spec, null, 0f);
        }

        private int DealInCone(Vector3 origin, Vector3 coneDir, float angleDeg, float radius, int dmg,
            Vector3 knockDir, bool bigFx, WeaponSkillSpec spec)
        {
            return DealFiltered(origin, radius, dmg, knockDir, bigFx, spec, coneDir.normalized, angleDeg);
        }

        private int DealFiltered(Vector3 origin, float radius, int dmg, Vector3 dir, bool bigFx,
            WeaponSkillSpec spec, Vector3? coneDir, float angleDeg)
        {
            var hitCount = 0;
            var seen = new HashSet<IDamageable>();
            foreach (var col in Physics.OverlapSphere(origin, radius))
            {
                if (col.transform == transform || col.transform.IsChildOf(transform))
                {
                    continue;
                }

                var d = col.GetComponentInParent<IDamageable>();
                if (d == null || d.IsDead || col.GetComponentInParent<PlayerStats>() != null)
                {
                    continue;
                }

                if (!seen.Add(d))
                {
                    continue;
                }

                if (coneDir.HasValue)
                {
                    var to = col.transform.position - origin;
                    to.y = 0f;
                    if (to.sqrMagnitude < 0.01f)
                    {
                        continue;
                    }

                    if (Vector3.Angle(coneDir.Value, to.normalized) > angleDeg * 0.5f)
                    {
                        continue;
                    }
                }

                d.TakeDamage(dmg, new Vector2(dir.x, dir.z), bigFx);
                if (spec.SlowDur > 0f && spec.SlowMul < 0.99f)
                {
                    col.GetComponentInParent<MonsterBrain>()?.ApplySlow(spec.SlowMul, spec.SlowDur);
                }

                hitCount++;
            }

            if (hitCount > 0)
            {
                AttackFx.PlayColoredImpact(origin + dir * 0.4f, SkillCatalog.FxColor(spec.Weapon), bigFx);
                CombatAudio.HitCrit();
                HitStop.Pulse(0.07f, 0.05f);
            }
            else
            {
                CombatAudio.Whiff();
            }

            return hitCount;
        }
    }
}
