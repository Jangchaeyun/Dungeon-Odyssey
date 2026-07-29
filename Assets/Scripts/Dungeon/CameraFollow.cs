using UnityEngine;

namespace DungeonOdyssey.Dungeon
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smooth = 7f;
        [SerializeField] private Vector3 offset = new(0f, 0.3f, -10f);
        [SerializeField] private float lookAhead = 0.55f;

        private Vector3 _shake;
        private float _shakeTimer;
        private float _shakeMagnitude;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null)
            {
                transform.position = target.position + offset;
            }
        }

        public void Shake(float magnitude = 0.12f, float duration = 0.12f)
        {
            _shakeMagnitude = magnitude;
            _shakeTimer = duration;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            var player = target.GetComponent<Player.PlayerController>();
            var ahead = player != null ? (Vector3)player.FacingDirection * lookAhead : Vector3.zero;
            var desired = target.position + offset + ahead;

            if (_shakeTimer > 0f)
            {
                _shakeTimer -= Time.deltaTime;
                _shake = (Vector3)Random.insideUnitCircle * _shakeMagnitude;
            }
            else
            {
                _shake = Vector3.zero;
            }

            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-smooth * Time.deltaTime)) + _shake;
        }
    }
}
