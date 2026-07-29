using DungeonOdyssey.Combat;
using DungeonOdyssey.Core;
using DungeonOdyssey.View3D;
using UnityEngine;

namespace DungeonOdyssey.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5.5f;
        [SerializeField] private float accel = 38f;
        [SerializeField] private float decel = 48f;
        [SerializeField] private float turnSpeed = 14f;
        [SerializeField] private PlayerCombat combat;

        private Rigidbody _rb;
        private CombatReactor _reactor;
        private PlayerStats _stats;
        private Vector3 _moveInput;
        private Vector3 _planarVel;
        private bool _canControl = true;
        private bool _scriptedWalk;
        private bool _scriptedHasTarget;
        private bool _wasKinematic;
        private Vector3 _scriptedVel;
        private Vector3 _scriptedTarget;
        private Vector3 _scriptedDir = Vector3.forward;
        private float _scriptedMaxSpeed = 4.2f;
        private float _scriptedRamp;
        private float _facingLockUntil;
        private Vector3 _lockedFacing = Vector3.forward;
        private CameraRig3D _cam;

        public Vector2 FacingDirection { get; private set; } = Vector2.up;
        public Vector3 Facing3D { get; private set; } = Vector3.forward;
        public bool IsScriptedWalking => _scriptedWalk;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _reactor = GetComponent<CombatReactor>();
            _stats = GetComponent<PlayerStats>();
            if (combat == null)
            {
                combat = GetComponent<PlayerCombat>();
            }
        }

        private void Start()
        {
            _cam = Camera.main != null ? Camera.main.GetComponent<CameraRig3D>() : null;
        }

        private void Update()
        {
            if (GameUi.IsBlocking || GameUi.IsPaused || !_canControl)
            {
                _moveInput = Vector3.zero;
                return;
            }

            if (GameInput.PotionDown)
            {
                if (_stats != null && _stats.TryUsePotion())
                {
                    FindFirstObjectByType<UI.HudUI>()?.SetHint("포션 사용!");
                }
                else if (_stats != null)
                {
                    var msg = _stats.Potions <= 0 ? "포션이 없다" : "체력이 이미 가득하다";
                    FindFirstObjectByType<UI.HudUI>()?.SetHint(msg);
                }
            }

            var slotKey = GameInput.WeaponSlotKeyDown;
            if (slotKey > 0)
            {
                if (_stats != null && _stats.TryEquipOwnedSlot(slotKey, out var smsg))
                {
                    CombatAudio.UiClick();
                    FindFirstObjectByType<UI.HudUI>()?.SetHint(smsg);
                }
            }
            else
            {
                var scroll = GameInput.WeaponScrollDelta;
                var cycleDir = 0;
                if (scroll != 0)
                {
                    cycleDir = scroll;
                }
                else if (GameInput.WeaponCyclePrevDown)
                {
                    cycleDir = -1;
                }
                else if (GameInput.WeaponCycleDown)
                {
                    cycleDir = 1;
                }

                if (cycleDir != 0)
                {
                    if (_stats != null && _stats.TryCycleWeapon(cycleDir, out var wmsg))
                    {
                        CombatAudio.UiClick();
                        FindFirstObjectByType<UI.HudUI>()?.SetHint(wmsg);
                    }
                    else if (_stats != null)
                    {
                        FindFirstObjectByType<UI.HudUI>()?.SetHint(
                            "교체할 무기가 없다 · 대장간에서 구매");
                    }
                }
            }

            var hurt = _reactor != null && _reactor.IsReacting;
            if (hurt)
            {
                _moveInput = Vector3.zero;
                return;
            }

            var axis = GameInput.MoveAxis;
            _moveInput = new Vector3(axis.x, 0f, axis.y);

            if (Time.time < _facingLockUntil)
            {
                // 이동 입력이 있으면 잠금 해제 — 옆걸음(크랩) 방지
                if (_moveInput.sqrMagnitude > 0.01f)
                {
                    _facingLockUntil = 0f;
                    ApplyFacing(_moveInput.normalized, instant: false);
                }
                else
                {
                    ForceFacing(_lockedFacing);
                }
            }
            else if (_moveInput.sqrMagnitude > 0.01f)
            {
                var turn = (combat != null && combat.IsAttacking) ? turnSpeed * 0.45f : turnSpeed;
                ApplyFacing(_moveInput.normalized, instant: false, turnSpeedMul: turn / turnSpeed);
            }

            if (GameInput.AttackDown)
            {
                combat?.TryAttack(FacingDirection);
            }
        }

        private void FixedUpdate()
        {
            if (_scriptedWalk)
            {
                UpdateScriptedWalk();
                return;
            }

            if (GameUi.IsBlocking || GameUi.IsPaused)
            {
                _planarVel = Vector3.zero;
                _rb.linearVelocity = Vector3.zero;
                return;
            }

            if (_reactor != null && _reactor.IsReacting)
            {
                _planarVel = Vector3.Lerp(_planarVel, Vector3.zero, 10f * Time.fixedDeltaTime);
                return;
            }

            var speed = moveSpeed + (_stats != null ? _stats.MoveSpeedBonus : 0f);

            // 공격 중에는 이동 입력·관성을 완전히 차단
            if (combat != null && combat.IsAttacking)
            {
                _planarVel = Vector3.zero;
                _rb.linearVelocity = Vector3.zero;
                return;
            }

            var target = _moveInput * speed;
            var rate = target.sqrMagnitude > _planarVel.sqrMagnitude ? accel : decel;
            _planarVel = Vector3.MoveTowards(_planarVel, target, rate * Time.fixedDeltaTime);

            // PhysX 속도 이동 — 장애물 BoxCollider에 부딪히면 자연스럽게 미끄러짐
            _rb.linearVelocity = new Vector3(_planarVel.x, 0f, _planarVel.z);
        }

        private void UpdateScriptedWalk()
        {
            _scriptedRamp = Mathf.MoveTowards(_scriptedRamp, 1f, Time.fixedDeltaTime * 3.2f);
            var spd = _scriptedMaxSpeed * Mathf.Lerp(0.55f, 1f, _scriptedRamp);

            if (_scriptedHasTarget)
            {
                var pos = _rb != null ? _rb.position : transform.position;
                var onLine = ProjectOnScriptedLine(pos);
                var along = Vector3.Dot(_scriptedTarget - onLine, _scriptedDir);
                if (along <= 0.08f)
                {
                    SnapToScriptedTarget();
                    _scriptedVel = Vector3.zero;
                    return;
                }

                var step = Mathf.Min(along, spd * Time.fixedDeltaTime);
                var next = onLine + _scriptedDir * step;
                next.y = pos.y;
                if (_rb != null)
                {
                    _rb.MovePosition(next);
                }
                else
                {
                    transform.position = next;
                }

                _scriptedVel = _scriptedDir * spd;
                _planarVel = _scriptedVel;
                return;
            }

            if (_scriptedDir.sqrMagnitude > 0.01f)
            {
                var pos = _rb != null ? _rb.position : transform.position;
                var next = ProjectOnScriptedLine(pos) + _scriptedDir * (spd * Time.fixedDeltaTime);
                next.y = pos.y;
                if (_rb != null)
                {
                    _rb.MovePosition(next);
                }

                _scriptedVel = _scriptedDir * spd;
                _planarVel = _scriptedVel;
            }
        }

        private Vector3 ProjectOnScriptedLine(Vector3 pos)
        {
            var origin = _scriptedHasTarget ? _scriptedTarget : pos;
            var onLine = origin + _scriptedDir * Vector3.Dot(pos - origin, _scriptedDir);
            onLine.y = pos.y;
            return onLine;
        }

        private void SnapToScriptedTarget()
        {
            var p = _scriptedTarget;
            p.y = transform.position.y;
            if (_rb != null)
            {
                _rb.MovePosition(p);
            }
            else
            {
                transform.position = p;
            }
        }

        public void SetControlEnabled(bool enabled)
        {
            _canControl = enabled;
            if (!enabled)
            {
                _moveInput = Vector3.zero;
                _planarVel = Vector3.zero;
                if (!_scriptedWalk && _rb != null && !_rb.isKinematic)
                {
                    _rb.linearVelocity = Vector3.zero;
                }
            }
            else if (_scriptedWalk)
            {
                // 제어 복구 시 연출 이동이 남아 있으면 강제 해제 (kinematic 고착 방지)
                EndScriptedWalk();
            }
        }

        private void LateUpdate()
        {
            if (_scriptedWalk)
            {
                ForceFacing(_scriptedDir.sqrMagnitude > 0.01f ? _scriptedDir : _lockedFacing);
            }
            else if (Time.time < _facingLockUntil)
            {
                ForceFacing(_lockedFacing);
            }
        }

        /// <summary>문 통과 등 — 입력 없이 일직선으로 걷기.</summary>
        public void BeginScriptedWalk(Vector3 worldDir, float speed)
        {
            worldDir.y = 0f;
            if (worldDir.sqrMagnitude < 0.01f)
            {
                return;
            }

            _scriptedHasTarget = false;
            _scriptedDir = Cardinalize(worldDir);
            _scriptedMaxSpeed = Mathf.Max(0.5f, speed);
            _scriptedRamp = 0.3f;
            BeginScriptedCommon(_scriptedDir);
            _scriptedVel = _scriptedDir * (_scriptedMaxSpeed * 0.4f);
            _planarVel = _scriptedVel;
        }

        /// <summary>목표까지 걸어감 (동서남북 축으로 자동 고정).</summary>
        public void BeginScriptedWalkTo(Vector3 worldTarget, float speed)
        {
            var dir = worldTarget - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f)
            {
                dir = Facing3D.sqrMagnitude > 0.01f ? Facing3D : Vector3.forward;
            }

            BeginScriptedWalkAlong(dir, worldTarget, speed);
        }

        /// <summary>지정 축을 따라 목표까지 완전 직선 이동.</summary>
        public void BeginScriptedWalkAlong(Vector3 axis, Vector3 worldTarget, float speed)
        {
            axis.y = 0f;
            if (axis.sqrMagnitude < 0.01f)
            {
                return;
            }

            _scriptedDir = Cardinalize(axis);
            worldTarget.y = transform.position.y;
            var pos = transform.position;
            // 북/남 이동이면 X만 라인에 고정, 동/서면 Z 고정
            if (Mathf.Abs(_scriptedDir.z) > 0.5f)
            {
                worldTarget.x = pos.x;
            }
            else
            {
                worldTarget.z = pos.z;
            }

            _scriptedTarget = worldTarget;
            _scriptedHasTarget = true;
            _scriptedMaxSpeed = Mathf.Max(0.5f, speed);
            _scriptedRamp = 0.35f;

            SnapPosition(ProjectOnScriptedLine(pos));
            BeginScriptedCommon(_scriptedDir);
            _scriptedVel = _scriptedDir * (_scriptedMaxSpeed * 0.4f);
            _planarVel = _scriptedVel;
        }

        public static Vector3 Cardinalize(Vector3 dir)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f)
            {
                return Vector3.forward;
            }

            dir.Normalize();
            if (Mathf.Abs(dir.z) >= Mathf.Abs(dir.x))
            {
                return dir.z >= 0f ? Vector3.forward : Vector3.back;
            }

            return dir.x >= 0f ? Vector3.right : Vector3.left;
        }

        private void BeginScriptedCommon(Vector3 faceDir)
        {
            ForceFacing(faceDir);
            LockFacing(faceDir, 3f);
            // 이미 연출 중이면 kinematic 원본을 덮어쓰지 않음
            // (문 통과 시 Begin을 연속 호출해도 End 후 이동 가능해야 함)
            if (!_scriptedWalk)
            {
                SetCollidersEnabled(false);
                if (_rb != null)
                {
                    _wasKinematic = _rb.isKinematic;
                    _rb.linearVelocity = Vector3.zero;
                    _rb.angularVelocity = Vector3.zero;
                    _rb.isKinematic = true;
                }
            }

            _scriptedWalk = true;
        }

        public bool ScriptedNearTarget(float stopDist = 0.35f)
        {
            if (!_scriptedWalk)
            {
                return true;
            }

            if (!_scriptedHasTarget)
            {
                return false;
            }

            var flat = transform.position;
            flat.y = _scriptedTarget.y;
            var remain = Vector3.Dot(_scriptedTarget - flat, _scriptedDir);
            return remain <= stopDist;
        }

        public void EndScriptedWalk()
        {
            _scriptedWalk = false;
            _scriptedHasTarget = false;
            _scriptedVel = Vector3.zero;
            _planarVel = Vector3.zero;
            _scriptedRamp = 0f;
            SetCollidersEnabled(true);
            if (_rb != null)
            {
                _rb.isKinematic = _wasKinematic;
                if (!_rb.isKinematic)
                {
                    _rb.linearVelocity = Vector3.zero;
                    _rb.angularVelocity = Vector3.zero;
                }
            }
        }

        /// <summary>연출용 — 즉시 좌표 스냅 (Z 정렬 등).</summary>
        public void SnapPosition(Vector3 worldPos)
        {
            worldPos.y = transform.position.y;
            if (_rb != null)
            {
                if (_rb.isKinematic)
                {
                    _rb.MovePosition(worldPos);
                }
                else
                {
                    _rb.position = worldPos;
                    _rb.linearVelocity = Vector3.zero;
                }
            }

            transform.position = worldPos;
        }

        public void LockFacing(Vector3 worldDir, float seconds)
        {
            worldDir.y = 0f;
            if (worldDir.sqrMagnitude < 0.01f)
            {
                return;
            }

            _lockedFacing = worldDir.normalized;
            _facingLockUntil = Time.time + Mathf.Max(0.05f, seconds);
            ForceFacing(_lockedFacing);
        }

        public void ForceFacing(Vector3 worldDir)
        {
            ApplyFacing(worldDir, instant: true);
        }

        private void ApplyFacing(Vector3 worldDir, bool instant, float turnSpeedMul = 1f)
        {
            worldDir.y = 0f;
            if (worldDir.sqrMagnitude < 0.01f)
            {
                return;
            }

            Facing3D = worldDir.normalized;
            FacingDirection = new Vector2(Facing3D.x, Facing3D.z);
            var target = Quaternion.LookRotation(Facing3D, Vector3.up);
            if (instant)
            {
                transform.rotation = target;
            }
            else
            {
                var t = Mathf.Clamp01(turnSpeed * turnSpeedMul * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, target, t);
            }

            // FreezeRotation + Interpolate면 transform만 돌리면 rb가 시선을 덮어씀 → 옆걸음
            if (_rb != null)
            {
                if (_rb.isKinematic)
                {
                    _rb.rotation = transform.rotation;
                }
                else
                {
                    _rb.MoveRotation(transform.rotation);
                }
            }

            _cam ??= Camera.main != null ? Camera.main.GetComponent<CameraRig3D>() : null;
            _cam?.SetFacing(Facing3D);
        }

        private void SetCollidersEnabled(bool enabled)
        {
            foreach (var col in GetComponents<Collider>())
            {
                if (col != null && !col.isTrigger)
                {
                    col.enabled = enabled;
                }
            }
        }
    }
}
