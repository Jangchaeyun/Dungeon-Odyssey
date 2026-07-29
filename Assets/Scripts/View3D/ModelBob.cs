using UnityEngine;

namespace DungeonOdyssey.View3D
{
    /// <summary>
    /// AI/외부 메시용 간단한 숨쉬기·이동 바운스.
    /// </summary>
    public class ModelBob : MonoBehaviour
    {
        private Transform _visual;
        private Rigidbody _rb;
        private Vector3 _basePos;
        private float _phase;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _visual = transform.Find("Visual") ?? transform;
            _basePos = _visual.localPosition;
        }

        private void LateUpdate()
        {
            if (_visual == null)
            {
                return;
            }

            _phase += Time.deltaTime;
            var speed = 0f;
            if (_rb != null)
            {
                var v = _rb.linearVelocity;
                v.y = 0f;
                speed = v.magnitude;
            }

            var bob = Mathf.Sin(_phase * (2.5f + speed * 1.5f)) * (0.02f + speed * 0.015f);
            _visual.localPosition = _basePos + Vector3.up * bob;
            var squash = 1f + Mathf.Sin(_phase * 3f) * 0.02f;
            _visual.localScale = new Vector3(squash, 1f / squash, squash);
        }
    }
}
