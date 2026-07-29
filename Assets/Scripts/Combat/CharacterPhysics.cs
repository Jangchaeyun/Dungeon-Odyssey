using DungeonOdyssey.Player;
using UnityEngine;

namespace DungeonOdyssey.Combat
{
    /// <summary>
    /// 3D 캐릭터 물리 + 겹침 방지.
    /// </summary>
    public class CharacterPhysics : MonoBehaviour
    {
        [SerializeField] private float separateRadius = 0.7f;
        [SerializeField] private float separateForce = 14f;

        private Rigidbody _rb;
        private CapsuleCollider _col;

        public static void Apply3D(GameObject go, float radius, float height, Vector3 center, float mass = 1f)
        {
            var rb = go.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = go.AddComponent<Rigidbody>();
            }

            rb.mass = mass;
            rb.linearDamping = 5f;
            rb.angularDamping = 8f;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.useGravity = false;

            var col = go.GetComponent<CapsuleCollider>();
            if (col == null)
            {
                col = go.AddComponent<CapsuleCollider>();
            }

            col.isTrigger = false;
            col.radius = radius;
            col.height = height;
            col.center = center;
            col.direction = 1; // Y

            var physics = go.GetComponent<CharacterPhysics>();
            if (physics == null)
            {
                physics = go.AddComponent<CharacterPhysics>();
            }

            physics.separateRadius = radius * 1.25f;
            EnsureLayersCollide();
        }

        // 구 API 호환 (호출부 정리용 빈 구현)
        public static void Apply(GameObject go, float colliderRadius, Vector2 colliderOffset, float mass = 1f)
        {
            Apply3D(go, colliderRadius, colliderRadius * 2.2f, new Vector3(0f, colliderRadius * 1.1f, 0f), mass);
        }

        public static void EnsureLayersCollide()
        {
            var enemy = LayerMask.NameToLayer("Enemy");
            if (enemy >= 0)
            {
                Physics.IgnoreLayerCollision(0, enemy, false);
            }
        }

        public static void MoveWithCollision(Rigidbody rb, Vector3 delta)
        {
            if (rb == null || delta.sqrMagnitude < 0.0001f)
            {
                return;
            }

            var dist = delta.magnitude;
            var dir = delta / dist;
            if (CapsuleSweep(rb, dir, dist, out var hit))
            {
                var allowed = Mathf.Max(0f, hit.distance - 0.03f);
                rb.MovePosition(rb.position + dir * allowed);
            }
            else
            {
                rb.MovePosition(rb.position + delta);
            }
        }

        /// <summary>
        /// 장애물에 막히면 벽면을 따라 미끄러지며 이동.
        /// 바닥(Earth 등) 겹침으로 이동이 죽지 않게 distance≈0·수직법선은 무시.
        /// </summary>
        public static void MoveAndSlide(Rigidbody rb, Vector3 planarVelocity, float skin = 0.06f)
        {
            if (rb == null)
            {
                return;
            }

            var delta = planarVelocity * Time.fixedDeltaTime;
            delta.y = 0f;
            if (delta.sqrMagnitude < 1e-8f)
            {
                var stop = rb.linearVelocity;
                stop.x = 0f;
                stop.z = 0f;
                rb.linearVelocity = stop;
                return;
            }

            var originalY = rb.position.y;
            var pos = rb.position;
            var remaining = delta;
            for (var i = 0; i < 3; i++)
            {
                var dist = remaining.magnitude;
                if (dist < 1e-5f)
                {
                    break;
                }

                var dir = remaining / dist;
                rb.position = pos;
                if (CapsuleSweep(rb, dir, dist + skin, out var hit))
                {
                    var travel = Mathf.Max(0f, hit.distance - skin);
                    pos += dir * travel;
                    remaining -= dir * travel;

                    var n = hit.normal;
                    n.y = 0f;
                    if (n.sqrMagnitude < 1e-4f)
                    {
                        remaining = Vector3.zero;
                        break;
                    }

                    n.Normalize();
                    remaining = Vector3.ProjectOnPlane(remaining, n);
                }
                else
                {
                    pos += remaining;
                    remaining = Vector3.zero;
                }
            }

            pos.y = originalY;
            rb.MovePosition(pos);
            var v = rb.linearVelocity;
            v.x = 0f;
            v.z = 0f;
            rb.linearVelocity = v;
        }

        private static bool CapsuleSweep(Rigidbody rb, Vector3 dir, float distance, out RaycastHit best)
        {
            best = default;
            var col = rb.GetComponent<CapsuleCollider>();
            if (col == null)
            {
                if (!rb.SweepTest(dir, out best, distance, QueryTriggerInteraction.Ignore))
                {
                    return false;
                }

                return IsBlockingHit(rb.transform, best);
            }

            var t = rb.transform;
            var center = t.TransformPoint(col.center);
            var lossy = t.lossyScale;
            var radius = col.radius * Mathf.Max(lossy.x, lossy.z) * 0.9f;
            var height = Mathf.Max(col.height * lossy.y, radius * 2.05f);
            var half = Mathf.Max(0.01f, height * 0.5f - radius);
            // 바닥 스침 방지: 캡슐을 살짝 띄워 캐스트
            var lift = 0.08f;
            var p1 = center + Vector3.up * half + Vector3.up * lift;
            var p2 = center - Vector3.up * half + Vector3.up * lift;

            var hits = Physics.CapsuleCastAll(p1, p2, radius, dir, distance, Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            var found = false;
            var bestDist = float.MaxValue;
            foreach (var hit in hits)
            {
                if (!IsBlockingHit(t, hit))
                {
                    continue;
                }

                if (hit.distance < bestDist)
                {
                    bestDist = hit.distance;
                    best = hit;
                    found = true;
                }
            }

            return found;
        }

        private static bool IsBlockingHit(Transform self, RaycastHit hit)
        {
            if (hit.collider == null)
            {
                return false;
            }

            var ht = hit.collider.transform;
            if (ht == self || ht.IsChildOf(self))
            {
                return false;
            }

            if (hit.rigidbody != null && hit.rigidbody.transform == self)
            {
                return false;
            }

            // 이미 파고든 상태(distance≈0)는 바닥 겹침이 대부분이므로 이동 차단에 쓰지 않음
            if (hit.distance <= 0.001f)
            {
                return false;
            }

            // 바닥·천장
            if (Mathf.Abs(hit.normal.y) > 0.55f)
            {
                return false;
            }

            // 넓은 지면 슬랩
            var n = hit.collider.gameObject.name;
            if (n == "Earth" || n.StartsWith("Grass") || n.StartsWith("Road") || n.StartsWith("Plaza")
                || n.StartsWith("Tile_") || n.StartsWith("Market") || n.StartsWith("DoorPad")
                || n.StartsWith("Dirt") || n.StartsWith("Patch"))
            {
                return false;
            }

            var size = hit.collider.bounds.size;
            if (size.x > 16f && size.z > 16f && size.y < 2.5f)
            {
                return false;
            }

            return true;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _col = GetComponent<CapsuleCollider>();
        }

        private void FixedUpdate()
        {
            if (_rb == null || _rb.isKinematic)
            {
                return;
            }

            var controller = GetComponent<PlayerController>();
            if (controller != null && controller.IsScriptedWalking)
            {
                return;
            }

            var hits = Physics.OverlapSphere(_rb.position + Vector3.up * 0.5f, separateRadius);
            foreach (var hit in hits)
            {
                if (hit == null || hit.isTrigger || hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    continue;
                }

                var otherRb = hit.attachedRigidbody;
                if (otherRb == null || otherRb == _rb || !otherRb.GetComponent<CharacterPhysics>())
                {
                    continue;
                }

                var delta = _rb.position - otherRb.position;
                delta.y = 0f;
                if (delta.sqrMagnitude < 0.0001f)
                {
                    delta = Random.insideUnitSphere;
                    delta.y = 0f;
                }

                var dist = delta.magnitude;
                var minDist = separateRadius + 0.08f;
                if (dist < minDist)
                {
                    var push = delta.normalized * (minDist - dist) * separateForce;
                    _rb.AddForce(push, ForceMode.Force);
                    otherRb.AddForce(-push * 0.65f, ForceMode.Force);
                }
            }
        }
    }
}
