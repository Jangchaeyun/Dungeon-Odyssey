using UnityEngine;

namespace DungeonOdyssey.View3D
{
    public class MonsterAnimator : MonoBehaviour
    {
        public Transform Body;
        public Transform Jaw;
        public Transform TentacleL;
        public Transform TentacleR;
        public Transform EyeL;
        public Transform EyeR;

        private Rigidbody _rb;
        private Vector3 _body0;
        private Quaternion _jaw0;
        private Quaternion _tentL0;
        private Quaternion _tentR0;
        private float _phase;
        private float _biteT = -1f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void Bind()
        {
            _body0 = Body != null ? Body.localScale : Vector3.one;
            _jaw0 = Jaw != null ? Jaw.localRotation : Quaternion.identity;
            _tentL0 = TentacleL != null ? TentacleL.localRotation : Quaternion.identity;
            _tentR0 = TentacleR != null ? TentacleR.localRotation : Quaternion.identity;
        }

        public void PlayBite()
        {
            _biteT = 0f;
        }

        private void LateUpdate()
        {
            _phase += Time.deltaTime;
            var speed = 0f;
            if (_rb != null)
            {
                var v = _rb.linearVelocity;
                v.y = 0f;
                speed = v.magnitude;
            }

            var hop = Mathf.Abs(Mathf.Sin(_phase * (2.2f + speed))) * (0.04f + speed * 0.02f);
            var squash = 1f + Mathf.Sin(_phase * 3.4f) * 0.06f;

            if (Body != null)
            {
                Body.localScale = new Vector3(_body0.x * squash, _body0.y / squash + hop * 0.5f, _body0.z * squash);
                Body.localPosition = new Vector3(0f, 0.55f + hop, 0f);
            }

            if (TentacleL != null)
            {
                TentacleL.localRotation = _tentL0 * Quaternion.Euler(0f, 0f, Mathf.Sin(_phase * 4f) * 18f);
            }

            if (TentacleR != null)
            {
                TentacleR.localRotation = _tentR0 * Quaternion.Euler(0f, 0f, -Mathf.Sin(_phase * 4f + 1f) * 18f);
            }

            if (EyeL != null && EyeR != null)
            {
                var blink = Mathf.PingPong(_phase * 0.35f, 1f) > 0.92f ? 0.15f : 1f;
                EyeL.localScale = new Vector3(0.18f, 0.18f * blink, 0.18f);
                EyeR.localScale = new Vector3(0.18f, 0.18f * blink, 0.18f);
            }

            if (_biteT >= 0f && Jaw != null)
            {
                _biteT += Time.deltaTime;
                if (_biteT < 0.15f)
                {
                    Jaw.localRotation = Quaternion.Slerp(Jaw.localRotation,
                        _jaw0 * Quaternion.Euler(35f, 0f, 0f), 20f * Time.deltaTime);
                }
                else if (_biteT < 0.35f)
                {
                    Jaw.localRotation = Quaternion.Slerp(Jaw.localRotation, _jaw0, 18f * Time.deltaTime);
                }
                else
                {
                    Jaw.localRotation = _jaw0;
                    _biteT = -1f;
                }
            }
        }
    }
}
