using System.Collections;
using DungeonOdyssey.View3D;
using UnityEngine;

namespace DungeonOdyssey.Combat
{
    public class CombatReactor : MonoBehaviour
    {
        [SerializeField] private float hurtDuration = 0.32f;
        [SerializeField] private float knockbackForce = 3.5f;
        [SerializeField] private bool isPlayer;

        private Rigidbody _rb;
        private Renderer[] _renderers;
        private Color[] _baseColors;
        private Coroutine _routine;
        private bool _reacting;
        private Vector3 _baseScale;
        private Quaternion _baseRot;

        public bool IsReacting => _reacting;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            CacheRenderers();
            _baseScale = transform.localScale;
            _baseRot = transform.rotation;
        }

        private void CacheRenderers()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _baseColors = new Color[_renderers.Length];
            for (var i = 0; i < _renderers.Length; i++)
            {
                var m = _renderers[i].material;
                _baseColors[i] = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : m.color;
            }
        }

        public void Configure3D(bool player, float knockback = 3.5f)
        {
            isPlayer = player;
            knockbackForce = knockback;
        }

        public void PlayHit(Vector2 hitFromDirection, int damage, bool dying, bool critical = false)
        {
            if (_renderers == null || _renderers.Length == 0)
            {
                CacheRenderers();
            }

            if (_routine != null)
            {
                StopCoroutine(_routine);
            }

            _routine = StartCoroutine(HitRoutine(hitFromDirection, damage, dying, critical));
        }

        private IEnumerator HitRoutine(Vector2 fromDir, int damage, bool dying, bool critical)
        {
            _reacting = true;
            FloatingText.Damage(transform.position + Vector3.up * 1.35f, damage, critical);

            var hitDir = new Vector3(fromDir.x, 0f, fromDir.y);
            if (hitDir.sqrMagnitude < 0.01f)
            {
                hitDir = transform.forward;
            }

            hitDir.Normalize();
            // 적 피격 스파크는 공격자(PlayerCombat)가 담당 — 플레이어 피격만 여기서
            if (isPlayer)
            {
                AttackFx.PlayImpact3D(transform.position + Vector3.up * 1f - hitDir * 0.15f, false, false);
                CombatAudio.PlayerHurt();
                Core.SessionStats.NoteDamageTaken(Mathf.Max(1, damage));
            }

            // 피격 스쿼시 + 화이트 플래시
            var squash = critical ? 1.22f : 1.14f;
            transform.localScale = new Vector3(_baseScale.x * squash, _baseScale.y * 0.76f, _baseScale.z * squash);
            SetTint(Color.white);
            yield return new WaitForSecondsRealtime(0.035f);

            SetTint(new Color(1f, 0.3f, 0.3f));

            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                var force = knockbackForce * (dying ? 1.35f : critical ? 1.15f : 1f);
                _rb.AddForce(-hitDir * force, ForceMode.Impulse);
            }

            // 몸이 타격 반대쪽으로 기울어짐
            var lean = Quaternion.LookRotation(hitDir, Vector3.up) * Quaternion.Euler(-18f, 0f, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, lean, 0.65f);

            Camera.main?.GetComponent<CameraRig3D>()?.Shake(
                isPlayer ? 0.22f : critical ? 0.12f : 0.08f, 0.11f);
            if (isPlayer)
            {
                Camera.main?.GetComponent<CameraRig3D>()?.Punch(0.22f);
            }

            if (dying && !isPlayer)
            {
                HitStop.Pulse(0.1f, 0.04f);
                yield return DeathRoutine(hitDir);
                yield break;
            }

            var end = Time.unscaledTime + hurtDuration;
            var blink = false;
            while (Time.unscaledTime < end)
            {
                blink = !blink;
                SetTint(blink ? new Color(1f, 0.55f, 0.55f) : new Color(1f, 0.25f, 0.25f));
                transform.localScale = Vector3.Lerp(transform.localScale, _baseScale, 0.35f);
                yield return new WaitForSecondsRealtime(0.035f);
            }

            transform.localScale = _baseScale;
            RestoreTint();
            _reacting = false;
            _routine = null;
        }

        private IEnumerator DeathRoutine(Vector3 hitDir)
        {
            CombatAudio.Kill();
            var t = 0f;
            var spin = Random.Range(-120f, 120f);
            while (t < 0.55f)
            {
                t += Time.unscaledDeltaTime;
                var p = t / 0.55f;
                transform.localScale = Vector3.Lerp(_baseScale,
                    new Vector3(_baseScale.x * 1.4f, _baseScale.y * 0.12f, _baseScale.z * 1.4f), p);
                transform.Rotate(0f, spin * Time.unscaledDeltaTime, 0f, Space.World);
                transform.position += (-hitDir * 2.2f + Vector3.up * (1f - p) * 0.8f) * Time.unscaledDeltaTime;
                SetTint(new Color(1f, 0.35f, 0.3f, 1f - p));
                yield return null;
            }

            // 잔해 파티클
            AttackFx.PlayImpact3D(transform.position + Vector3.up * 0.4f, true, true);
            Destroy(gameObject);
        }

        private void SetTint(Color c)
        {
            if (_renderers == null)
            {
                return;
            }

            foreach (var r in _renderers)
            {
                if (r == null)
                {
                    continue;
                }

                var m = r.material;
                m.color = c;
                if (m.HasProperty("_BaseColor"))
                {
                    m.SetColor("_BaseColor", c);
                }
            }
        }

        private void RestoreTint()
        {
            if (_renderers == null)
            {
                return;
            }

            for (var i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null)
                {
                    continue;
                }

                var m = _renderers[i].material;
                m.color = _baseColors[i];
                if (m.HasProperty("_BaseColor"))
                {
                    m.SetColor("_BaseColor", _baseColors[i]);
                }
            }
        }
    }
}
