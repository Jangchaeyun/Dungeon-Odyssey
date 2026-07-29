using UnityEngine;

namespace DungeonOdyssey.Dungeon
{
    public class PortalPulse : MonoBehaviour
    {
        private Vector3 _baseScale;

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        private void Update()
        {
            var pulse = 1f + Mathf.Sin(Time.time * 4f) * 0.08f;
            transform.localScale = _baseScale * pulse;
        }
    }
}
