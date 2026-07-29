using System.Collections.Generic;
using DungeonOdyssey.Enemy;
using DungeonOdyssey.Player;
using UnityEngine;

namespace DungeonOdyssey.View3D
{
    /// <summary>
    /// 플레이어 추종 + 속도 선행 + FOV 호흡 + 타격 킥/쉐이크 + 가림 벽 숨김.
    /// </summary>
    public class CameraRig3D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        // 조금 더 위에서 내려다봐 벽 끝이 플레이어를 덜 가리게
        [SerializeField] private Vector3 offset = new(0f, 9.4f, -6.8f);
        [SerializeField] private float followSmooth = 8.5f;
        [SerializeField] private float lookSmooth = 6.2f;
        [SerializeField] private float lookAhead = 1.6f;
        [SerializeField] private float velocityAhead = 0.18f;
        [SerializeField] private float maxAhead = 2.2f;
        [SerializeField] private float lookHeight = 1.05f;
        [SerializeField] private float runFovBoost = 4.5f;
        [SerializeField] private float idleSettle = 0.35f;
        [SerializeField] private float occludeRadius = 0.28f;

        private Vector3 _shake;
        private float _shakeTimer;
        private float _shakeMag;
        private float _shakeSeed;
        private Vector3 _facing = Vector3.forward;
        private Vector3 _aheadSmoothed;
        private Vector3 _velSmoothed;
        private Vector3 _posVelocity;
        private float _punch;
        private float _punchVel;
        private float _fovBase = 42f;
        private float _fovCurrent;
        private Camera _cam;
        private Rigidbody _rb;

        private float _snapTimer;
        private float _snapDuration;
        private float _introTimer;
        private Vector3 _introFrom;

        private readonly List<Renderer> _occluded = new(24);
        private readonly RaycastHit[] _occludeHits = new RaycastHit[24];

        public void SetTarget(Transform t)
        {
            target = t;
            _cam = GetComponent<Camera>();
            _rb = target != null ? target.GetComponent<Rigidbody>() : null;
            if (_cam != null)
            {
                _fovBase = _cam.fieldOfView;
                _fovCurrent = _fovBase;
            }

            _shakeSeed = Random.value * 100f;
            _aheadSmoothed = Vector3.zero;
            _velSmoothed = Vector3.zero;
            _posVelocity = Vector3.zero;

            if (target != null)
            {
                // 등장: 살짝 위에서 내려오며 안착
                var settled = target.position + offset;
                _introFrom = settled + new Vector3(0f, 2.4f, -1.2f);
                transform.position = _introFrom;
                _introTimer = 0.55f;
                transform.LookAt(target.position + Vector3.up * lookHeight);
            }
        }

        public void SetFacing(Vector3 facing)
        {
            if (facing.sqrMagnitude > 0.01f)
            {
                _facing = facing.normalized;
            }
        }

        /// <summary>방 이동 직후 등 — 부드럽게 목표로 재정렬.</summary>
        public void SoftSnap(float duration = 0.5f)
        {
            _snapDuration = Mathf.Max(0.15f, duration);
            _snapTimer = _snapDuration;
            _posVelocity = Vector3.zero;
        }

        /// <summary>방 전환 시 카메라가 플레이어와 같이 이동해 끊김을 줄임.</summary>
        public void PreserveRelativeToPlayer(Vector3 oldPlayerPos, Vector3 newPlayerPos)
        {
            var delta = newPlayerPos - oldPlayerPos;
            delta.y = 0f;
            transform.position += delta;
            _introTimer = 0f;
            _snapTimer = 0f;
            _posVelocity = Vector3.zero;
            _aheadSmoothed = Vector3.zero;
            _velSmoothed = Vector3.zero;
        }

        public void Shake(float mag = 0.12f, float duration = 0.1f)
        {
            var scale = Core.GameManager.Instance?.CurrentSave != null
                ? Mathf.Clamp01(Core.GameManager.Instance.CurrentSave.shakeIntensity)
                : 1f;
            mag *= scale;
            if (mag <= 0.001f)
            {
                return;
            }

            _shakeMag = Mathf.Max(_shakeMag, mag);
            _shakeTimer = Mathf.Max(_shakeTimer, duration);
        }

        /// <summary>타격 시 살짝 줌인 킥.</summary>
        public void Punch(float amount = 0.2f)
        {
            var scale = Core.GameManager.Instance?.CurrentSave != null
                ? Mathf.Clamp01(Core.GameManager.Instance.CurrentSave.shakeIntensity)
                : 1f;
            amount *= scale;
            if (amount <= 0.001f)
            {
                return;
            }

            _punch = Mathf.Max(_punch, amount);
            _punchVel += amount * 3.5f;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            var dt = Time.unscaledDeltaTime;
            if (dt <= 0f)
            {
                return;
            }

            // 속도 / 바라보는 방향 → 선행
            var worldVel = _rb != null
                ? new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z)
                : Vector3.zero;
            _velSmoothed = Vector3.Lerp(_velSmoothed, worldVel, 1f - Mathf.Exp(-10f * dt));

            if (_velSmoothed.sqrMagnitude > 0.08f)
            {
                _facing = Vector3.Lerp(_facing, _velSmoothed.normalized, 1f - Mathf.Exp(-6f * dt)).normalized;
            }

            var speed = _velSmoothed.magnitude;
            var aheadTarget = _facing * lookAhead + _velSmoothed * velocityAhead;
            if (aheadTarget.sqrMagnitude > maxAhead * maxAhead)
            {
                aheadTarget = aheadTarget.normalized * maxAhead;
            }

            // 정지 시 살짝 되돌아오는 여운
            var aheadLerp = speed > 0.4f ? 7f : 3.2f;
            _aheadSmoothed = Vector3.Lerp(_aheadSmoothed, aheadTarget, 1f - Mathf.Exp(-aheadLerp * dt));
            if (speed < 0.15f)
            {
                _aheadSmoothed = Vector3.Lerp(_aheadSmoothed, Vector3.zero, idleSettle * dt);
            }

            // 이동 중 살짝 낮아지는 앵글 (액션감)
            var dynamicOffset = offset;
            dynamicOffset.y -= Mathf.Clamp01(speed / 6f) * 0.35f;
            dynamicOffset.z -= Mathf.Clamp01(speed / 6f) * 0.45f;

            var desired = target.position + dynamicOffset + _aheadSmoothed;

            UpdateShake(dt);
            UpdatePunch(dt);

            // 인트로 / 스냅: 더 빠른 추종
            var smooth = followSmooth;
            if (_introTimer > 0f)
            {
                _introTimer -= dt;
                var t = 1f - Mathf.Clamp01(_introTimer / 0.55f);
                t = t * t * (3f - 2f * t);
                desired = Vector3.Lerp(_introFrom, desired, t);
                smooth = Mathf.Lerp(14f, followSmooth, t);
            }

            if (_snapTimer > 0f)
            {
                _snapTimer -= dt;
                var snapT = 1f - Mathf.Clamp01(_snapTimer / _snapDuration);
                smooth = Mathf.Lerp(18f, followSmooth, snapT);
            }

            var follow = Vector3.SmoothDamp(transform.position - _shake, desired, ref _posVelocity,
                1f / Mathf.Max(0.01f, smooth), Mathf.Infinity, dt);

            var punchPull = transform.forward * (_punch * 0.28f);
            transform.position = follow + _shake - punchPull;

            // 시선: 가슴 높이 + 선행의 일부
            var lookPoint = target.position + Vector3.up * lookHeight + _aheadSmoothed * 0.45f;

            // 카메라가 몸에 파고들면 near-clip으로 하반신이 잘림 → 최소 거리 유지
            EnforceMinDistance(lookPoint);

            var lookRot = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
            // 이동 시 아주 약한 롤 (카메라 뱅크)
            var bank = Mathf.Clamp(-_velSmoothed.x * 0.35f, -2.2f, 2.2f);
            lookRot *= Quaternion.Euler(0f, 0f, bank);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 1f - Mathf.Exp(-lookSmooth * dt));

            UpdateFov(dt, speed);
            UpdateOcclusion();
        }

        /// <summary>플레이어와 너무 가까우면 뒤로 밀어 near-clip 절단을 막음.</summary>
        private void EnforceMinDistance(Vector3 lookPoint)
        {
            const float minDist = 5.2f;
            var toCam = transform.position - lookPoint;
            var dist = toCam.magnitude;
            if (dist >= minDist || dist < 0.001f)
            {
                return;
            }

            var dir = toCam / dist;
            // 기본 오프셋 방향과 섞어 급격한 점프 방지
            var preferred = offset.normalized;
            if (preferred.sqrMagnitude > 0.01f && Vector3.Dot(dir, preferred) < 0.2f)
            {
                dir = Vector3.Slerp(dir, preferred, 0.65f).normalized;
            }

            transform.position = lookPoint + dir * minDist;
        }

        private void OnDisable()
        {
            RestoreOcclusion();
        }

        /// <summary>카메라~플레이어 사이 벽/기둥이 시야를 가리면 렌더러만 잠시 숨김.</summary>
        private void UpdateOcclusion()
        {
            RestoreOcclusion();

            var lookAt = target.position + Vector3.up * lookHeight;
            var origin = transform.position;
            var toTarget = lookAt - origin;
            var dist = toTarget.magnitude;
            if (dist < 0.5f)
            {
                return;
            }

            var dir = toTarget / dist;
            var castDist = Mathf.Max(0.1f, dist - 0.85f);
            var count = Physics.SphereCastNonAlloc(origin, occludeRadius, dir, _occludeHits, castDist,
                ~0, QueryTriggerInteraction.Ignore);

            for (var i = 0; i < count; i++)
            {
                var hit = _occludeHits[i];
                if (hit.collider == null)
                {
                    continue;
                }

                var t = hit.collider.transform;
                if (!ShouldOcclude(t))
                {
                    continue;
                }

                // 맞은 조각만 숨김 (문 전체·방 전체가 한 번에 사라지지 않게)
                var ren = hit.collider.GetComponent<Renderer>();
                if (ren == null || !ren.enabled)
                {
                    continue;
                }

                ren.enabled = false;
                _occluded.Add(ren);
            }
        }

        private bool ShouldOcclude(Transform t)
        {
            if (t == null || target == null)
            {
                return false;
            }

            if (t == target || t.IsChildOf(target))
            {
                return false;
            }

            if (t.GetComponentInParent<PlayerStats>() != null)
            {
                return false;
            }

            if (t.GetComponentInParent<MonsterBrain>() != null)
            {
                return false;
            }

            var n = t.name;
            return !n.StartsWith("Floor") && !n.StartsWith("Rug") && !n.StartsWith("Shadow")
                   && !n.Contains("Label") && !n.Contains("HpBar");
        }

        private void RestoreOcclusion()
        {
            for (var i = 0; i < _occluded.Count; i++)
            {
                if (_occluded[i] != null)
                {
                    _occluded[i].enabled = true;
                }
            }

            _occluded.Clear();
        }

        private void UpdateShake(float dt)
        {
            if (_shakeTimer > 0f)
            {
                _shakeTimer -= dt;
                var falloff = Mathf.Clamp01(_shakeTimer * 8f);
                var t = Time.unscaledTime * 28f + _shakeSeed;
                _shake = new Vector3(
                    (Mathf.PerlinNoise(t, _shakeSeed) - 0.5f) * 2f,
                    (Mathf.PerlinNoise(_shakeSeed, t * 0.8f) - 0.5f) * 1.1f,
                    (Mathf.PerlinNoise(t * 0.7f, t) - 0.5f) * 2f) * (_shakeMag * falloff);
            }
            else
            {
                _shake = Vector3.Lerp(_shake, Vector3.zero, 1f - Mathf.Exp(-20f * dt));
                _shakeMag = 0f;
            }
        }

        private void UpdatePunch(float dt)
        {
            // 스프링으로 0에 안착 (살짝 오버슈트 후 복귀)
            _punchVel += -_punch * 48f * dt;
            _punchVel *= Mathf.Exp(-7f * dt);
            _punch += _punchVel * dt;
            if (_punch < 0f)
            {
                _punch *= 0.5f;
            }

            if (Mathf.Abs(_punch) < 0.001f && Mathf.Abs(_punchVel) < 0.01f)
            {
                _punch = 0f;
                _punchVel = 0f;
            }
        }

        private void UpdateFov(float dt, float speed)
        {
            if (_cam == null)
            {
                _cam = GetComponent<Camera>();
            }

            if (_cam == null)
            {
                return;
            }

            var speedNorm = Mathf.Clamp01(speed / 5.8f);
            var targetFov = _fovBase + speedNorm * runFovBoost - _punch * 7.5f;
            _fovCurrent = Mathf.Lerp(_fovCurrent, targetFov, 1f - Mathf.Exp(-10f * dt));
            _cam.fieldOfView = _fovCurrent;
        }
    }
}
