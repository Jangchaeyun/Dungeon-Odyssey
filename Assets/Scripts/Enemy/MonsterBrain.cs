using System.Collections;
using DungeonOdyssey.Combat;
using DungeonOdyssey.Core;
using DungeonOdyssey.Dungeon;
using DungeonOdyssey.Player;
using DungeonOdyssey.View3D;
using UnityEngine;

namespace DungeonOdyssey.Enemy
{
    public enum MonsterState
    {
        Idle,
        Chase,
        Windup,
        Attack,
        Hurt,
        Dead
    }

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Health))]
    public class MonsterBrain : MonoBehaviour
    {
        [SerializeField] private float detectRange = 3.2f;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float moveSpeed = 2.8f;
        [SerializeField] private float attackCooldown = 1.2f;
        [SerializeField] private int attackDamage = 10;
        [SerializeField] private int expReward = 14;

        private Rigidbody _rb;
        private Health _health;
        private CombatReactor _reactor;
        private MonsterAnimator _anim;
        private Transform _player;
        private float _nextAttackTime;
        private MonsterState _state = MonsterState.Idle;
        private bool _rewarded;
        private bool _attacking;
        private Vector3 _baseScale;
        private bool _elite;
        private bool _boss;
        private ThreatStyle _threat = ThreatStyle.Balanced;
        private int _comboHits = 1;
        private float _slowMul = 1f;
        private float _slowUntil;
        private int _patternPhase;

        public MonsterState State => _state;
        public bool IsBoss => _boss;
        public bool IsElite => _elite;

        public void ApplySlow(float speedMul, float duration)
        {
            _slowMul = Mathf.Clamp(speedMul, 0.2f, 1f);
            _slowUntil = Time.time + Mathf.Max(0.1f, duration);
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _health = GetComponent<Health>();
            _reactor = GetComponent<CombatReactor>();
            _anim = GetComponent<MonsterAnimator>();
            _baseScale = transform.localScale;
        }

        private void OnEnable()
        {
            _health.OnDied += HandleDeath;
            _health.OnDamaged += HandleDamaged;
        }

        private void OnDisable()
        {
            _health.OnDied -= HandleDeath;
            _health.OnDamaged -= HandleDamaged;
        }

        private void Start()
        {
            var player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                _player = player.transform;
            }
        }

        private void FixedUpdate()
        {
            if (_state == MonsterState.Dead || _player == null || _attacking)
            {
                if (_state == MonsterState.Dead || _player == null)
                {
                    SetVelocity(Vector3.zero);
                }

                return;
            }

            if (_reactor != null && _reactor.IsReacting)
            {
                _state = MonsterState.Hurt;
                return;
            }

            if (_state == MonsterState.Hurt)
            {
                _state = MonsterState.Chase;
            }

            var toPlayer = _player.position - transform.position;
            toPlayer.y = 0f;
            var distance = toPlayer.magnitude;

            switch (_state)
            {
                case MonsterState.Idle:
                    SetVelocity(Vector3.zero);
                    if (distance <= detectRange)
                    {
                        _state = MonsterState.Chase;
                    }

                    break;

                case MonsterState.Chase:
                    if (distance > detectRange * 1.4f)
                    {
                        _state = MonsterState.Idle;
                        SetVelocity(Vector3.zero);
                        break;
                    }

                    if (distance <= attackRange * (_boss ? 1.15f : 1f) && Time.time >= _nextAttackTime)
                    {
                        if (_boss)
                        {
                            StartCoroutine(BossAttackPattern(toPlayer.normalized));
                        }
                        else
                        {
                            StartCoroutine(AttackCombo(toPlayer.normalized));
                        }

                        break;
                    }

                    // 거리 유지하며 측면 선회 후 돌진
                    var desired = toPlayer.normalized;
                    if (distance < attackRange * 0.85f)
                    {
                        desired = -toPlayer.normalized; // 너무 붙으면 살짝 빠짐
                    }

                    var side = Vector3.Cross(Vector3.up, toPlayer.normalized) *
                               (Mathf.Sin(Time.time * 2.8f + GetInstanceID() * 0.01f) * 0.55f);
                    var spd = moveSpeed * (Time.time < _slowUntil ? _slowMul : 1f);
                    var chase = (desired + side).normalized * spd;
                    SetVelocity(chase);
                    Face(toPlayer.normalized);
                    break;
            }
        }

        private IEnumerator AttackCombo(Vector3 dir)
        {
            _attacking = true;
            _state = MonsterState.Windup;
            SetVelocity(Vector3.zero);
            Face(dir);
            _anim?.PlayBite();
            AttackFx.PlayBiteTelegraph(transform.position + dir * 0.6f);
            CombatAudio.MonsterBite();

            // 몸을 부풀리며 예고
            var t = 0f;
            while (t < 0.32f)
            {
                t += Time.deltaTime;
                if (_reactor != null && _reactor.IsReacting)
                {
                    transform.localScale = _baseScale;
                    _attacking = false;
                    _state = MonsterState.Hurt;
                    yield break;
                }

                var p = t / 0.32f;
                transform.localScale = _baseScale * (1f + p * 0.18f);
                // 살짝 뒤로 무게중심
                CharacterPhysics.MoveWithCollision(_rb, -dir * (0.015f));
                yield return null;
            }

            transform.localScale = _baseScale;
            _state = MonsterState.Attack;

            for (var hit = 0; hit < _comboHits; hit++)
            {
                var lunge = _boss ? 1.35f : _elite ? 1.15f : 1.05f;
                CharacterPhysics.MoveWithCollision(_rb, dir * lunge);
                Camera.main?.GetComponent<CameraRig3D>()?.Shake(_boss ? 0.2f : 0.12f, 0.1f);
                AttackFx.PlayImpact3D(transform.position + Vector3.up * 0.7f + dir * 0.7f, _boss, true);

                if (_player != null)
                {
                    var dist = Vector3.Distance(
                        new Vector3(transform.position.x, 0f, transform.position.z),
                        new Vector3(_player.position.x, 0f, _player.position.z));
                    if (dist <= attackRange * (_boss ? 1.6f : 1.35f))
                    {
                        var hitDir = new Vector2(dir.x, dir.z).normalized;
                        _player.GetComponent<Health>()?.TakeDamage(attackDamage, hitDir);
                        HitStop.Pulse(_boss ? 0.07f : 0.05f, 0.1f);
                        Camera.main?.GetComponent<CameraRig3D>()?.Punch(_boss ? 0.28f : 0.2f);
                    }
                }

                if (hit + 1 < _comboHits)
                {
                    yield return new WaitForSeconds(0.18f);
                    Face(dir);
                }
            }

            _nextAttackTime = Time.time + attackCooldown;
            yield return new WaitForSeconds(0.28f);
            transform.localScale = _baseScale;
            _attacking = false;
            _state = MonsterState.Chase;
        }

        public void Configure(int hp, int damage, int exp, float speed, bool elite = false, bool boss = false,
            ThreatStyle threat = ThreatStyle.Balanced)
        {
            attackDamage = damage;
            expReward = exp;
            moveSpeed = speed;
            _elite = elite;
            _boss = boss;
            _threat = threat;
            _comboHits = boss ? 3 : elite ? 2 : 1;
            attackCooldown = boss ? 1.45f : elite ? 1.05f : 1.2f;
            attackRange = boss ? 1.85f : elite ? 1.6f : 1.5f;
            detectRange = boss ? 4.5f : 3.2f;
            _health.Initialize(hp, hp);

            if (boss)
            {
                var scale = threat switch
                {
                    ThreatStyle.Siege => 1.75f,
                    ThreatStyle.Swarm => 1.4f,
                    ThreatStyle.Swift => 1.35f,
                    _ => 1.55f
                };
                transform.localScale = _baseScale * scale;
                _baseScale = transform.localScale;
                ApplyBossThemeVisual(threat);
                var (title, accent) = BossTitle(threat);
                WorldLabel.Attach(transform, title, new Vector3(0f, 2.6f, 0f),
                    accent, 0.04f, LabelShowMode.Always, 12f, priority: 2);
            }
            else if (elite)
            {
                transform.localScale = _baseScale * 1.25f;
                _baseScale = transform.localScale;
                TintRenderers(new Color(0.95f, 0.75f, 0.35f), 0.35f);
            }
        }

        private static (string title, Color accent) BossTitle(ThreatStyle style) => style switch
        {
            ThreatStyle.Swarm => ("무리의 여왕", new Color(0.55f, 0.95f, 0.55f)),
            ThreatStyle.Brutal => ("맹공의 학살자", new Color(1f, 0.35f, 0.28f)),
            ThreatStyle.Swift => ("질주의 환영", new Color(0.45f, 0.85f, 1f)),
            ThreatStyle.EliteHunt => ("정예 군주", new Color(1f, 0.82f, 0.35f)),
            ThreatStyle.Siege => ("공성의 거상", new Color(0.7f, 0.55f, 0.95f)),
            _ => ("공허의 군주", new Color(1f, 0.45f, 0.55f))
        };

        private void ApplyBossThemeVisual(ThreatStyle style)
        {
            var tint = style switch
            {
                ThreatStyle.Swarm => new Color(0.35f, 0.85f, 0.4f),
                ThreatStyle.Brutal => new Color(0.95f, 0.25f, 0.2f),
                ThreatStyle.Swift => new Color(0.35f, 0.7f, 1f),
                ThreatStyle.EliteHunt => new Color(0.95f, 0.75f, 0.25f),
                ThreatStyle.Siege => new Color(0.55f, 0.35f, 0.85f),
                _ => new Color(0.75f, 0.25f, 0.55f)
            };
            TintRenderers(tint, 0.55f);
        }

        private void TintRenderers(Color tint, float mix)
        {
            foreach (var r in GetComponentsInChildren<Renderer>(true))
            {
                if (r == null || r.sharedMaterial == null)
                {
                    continue;
                }

                var mat = r.material;
                var baseCol = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor")
                    : mat.HasProperty("_Color") ? mat.color : Color.white;
                var c = Color.Lerp(baseCol, tint, mix);
                if (mat.HasProperty("_Color"))
                {
                    mat.color = c;
                }

                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", c);
                }
            }
        }

        private IEnumerator BossAttackPattern(Vector3 dir)
        {
            _attacking = true;
            _patternPhase++;
            switch (_threat)
            {
                case ThreatStyle.Swarm:
                    yield return BossSwarmBursts(dir);
                    break;
                case ThreatStyle.Brutal:
                    yield return BossBrutalSlam(dir);
                    break;
                case ThreatStyle.Swift:
                    yield return BossSwiftDash(dir);
                    break;
                case ThreatStyle.EliteHunt:
                    yield return BossOrbitStrikes(dir);
                    break;
                case ThreatStyle.Siege:
                    yield return BossSiegeQuake(dir);
                    break;
                default:
                    yield return AttackCombo(dir);
                    break;
            }

            _nextAttackTime = Time.time + attackCooldown;
            _attacking = false;
            _state = MonsterState.Chase;
        }

        private IEnumerator BossSwarmBursts(Vector3 dir)
        {
            _state = MonsterState.Windup;
            SetVelocity(Vector3.zero);
            Face(dir);
            AttackFx.PlayBiteTelegraph(transform.position + dir * 0.5f);
            yield return new WaitForSeconds(0.22f);
            for (var i = 0; i < 4; i++)
            {
                var yaw = (i - 1.5f) * 28f;
                var d = Quaternion.Euler(0f, yaw, 0f) * dir;
                yield return LungeHit(d, 0.85f, 0.9f);
                yield return new WaitForSeconds(0.1f);
            }
        }

        private IEnumerator BossBrutalSlam(Vector3 dir)
        {
            _state = MonsterState.Windup;
            SetVelocity(Vector3.zero);
            Face(dir);
            AttackFx.PlayBiteTelegraph(transform.position + dir * 0.7f);
            var t = 0f;
            while (t < 0.45f)
            {
                t += Time.deltaTime;
                transform.localScale = _baseScale * (1f + (t / 0.45f) * 0.28f);
                yield return null;
            }

            transform.localScale = _baseScale;
            CharacterPhysics.MoveWithCollision(_rb, dir * 1.8f);
            AttackFx.PlayColoredImpact(transform.position + Vector3.up * 0.4f,
                new Color(1f, 0.3f, 0.2f), true);
            DealBossHit(dir, 1.45f, 1.35f);
            Camera.main?.GetComponent<CameraRig3D>()?.Shake(0.32f, 0.16f);
            yield return new WaitForSeconds(0.35f);
        }

        private IEnumerator BossSwiftDash(Vector3 dir)
        {
            _state = MonsterState.Windup;
            SetVelocity(Vector3.zero);
            AttackFx.PlayBiteTelegraph(transform.position + dir * 0.8f);
            yield return new WaitForSeconds(0.12f);
            for (var i = 0; i < 3; i++)
            {
                var side = i == 1 ? -dir : dir;
                if (i == 1)
                {
                    side = Vector3.Cross(Vector3.up, dir).normalized;
                }

                yield return LungeHit(side, 1.55f, 0.85f);
                yield return new WaitForSeconds(0.08f);
            }
        }

        private IEnumerator BossOrbitStrikes(Vector3 dir)
        {
            _state = MonsterState.Attack;
            SetVelocity(Vector3.zero);
            for (var i = 0; i < 3; i++)
            {
                var yaw = _patternPhase * 40f + i * 120f;
                var d = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
                Face(d);
                AttackFx.PlayBiteTelegraph(transform.position + d * 0.9f);
                yield return new WaitForSeconds(0.14f);
                yield return LungeHit(d, 1.2f, 1.05f);
            }
        }

        private IEnumerator BossSiegeQuake(Vector3 dir)
        {
            _state = MonsterState.Windup;
            SetVelocity(Vector3.zero);
            Face(dir);
            AttackFx.PlayBiteTelegraph(transform.position);
            yield return new WaitForSeconds(0.4f);
            for (var wave = 0; wave < 2; wave++)
            {
                AttackFx.PlayColoredImpact(transform.position + Vector3.up * 0.3f,
                    new Color(0.6f, 0.4f, 0.95f), true);
                DealBossHit(dir, 0f, 1.1f + wave * 0.35f);
                Camera.main?.GetComponent<CameraRig3D>()?.Shake(0.28f, 0.14f);
                yield return new WaitForSeconds(0.28f);
            }

            yield return LungeHit(dir, 1.1f, 1.2f);
        }

        private IEnumerator LungeHit(Vector3 dir, float lunge, float dmgMul)
        {
            _state = MonsterState.Attack;
            Face(dir);
            CharacterPhysics.MoveWithCollision(_rb, dir * lunge);
            AttackFx.PlayImpact3D(transform.position + Vector3.up * 0.7f + dir * 0.7f, true, true);
            DealBossHit(dir, 1.5f, dmgMul);
            yield return null;
        }

        private void DealBossHit(Vector3 dir, float rangeMul, float dmgMul)
        {
            if (_player == null)
            {
                return;
            }

            var dist = Vector3.Distance(
                new Vector3(transform.position.x, 0f, transform.position.z),
                new Vector3(_player.position.x, 0f, _player.position.z));
            var range = rangeMul <= 0.01f
                ? attackRange * (1.4f + dmgMul * 0.4f)
                : attackRange * rangeMul;
            if (dist > range)
            {
                return;
            }

            var hitDir = new Vector2(dir.x, dir.z).normalized;
            var dmg = Mathf.Max(1, Mathf.RoundToInt(attackDamage * dmgMul));
            _player.GetComponent<Health>()?.TakeDamage(dmg, hitDir);
            HitStop.Pulse(0.07f, 0.1f);
            Camera.main?.GetComponent<CameraRig3D>()?.Punch(0.28f);
        }

        private void HandleDamaged(int amount, Vector2 dir)
        {
            _state = MonsterState.Hurt;
            // 피격 시 공격 캔슬
            if (_attacking)
            {
                StopAllCoroutines();
                _attacking = false;
                transform.localScale = _baseScale;
            }
        }

        private void HandleDeath()
        {
            _state = MonsterState.Dead;
            SetVelocity(Vector3.zero);

            // 보스 이름표 등 — 시체 위에 남아 다른 라벨과 겹치지 않게 제거
            foreach (var label in GetComponentsInChildren<WorldLabel>(true))
            {
                Destroy(label.gameObject);
            }

            if (_rewarded)
            {
                return;
            }

            _rewarded = true;
            var stats = _player != null ? _player.GetComponent<PlayerStats>() : null;
            stats?.AddExperience(expReward);
            CombatAudio.Kill();
            DungeonManager.Instance?.NotifyMonsterDefeated(this);
        }

        private void Face(Vector3 dir)
        {
            if (dir.sqrMagnitude < 0.01f)
            {
                return;
            }

            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir, Vector3.up), 14f * Time.fixedDeltaTime);
        }

        private void SetVelocity(Vector3 velocity)
        {
            if (_rb == null)
            {
                return;
            }

            _rb.linearVelocity = new Vector3(velocity.x, 0f, velocity.z);
        }
    }
}
